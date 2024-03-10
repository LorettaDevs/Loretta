// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax
{
    // This *slightly* based on VB's SyntaxNormalizer as it is more similar to Lua than C#
    // It has been heavily modified as maintaining the token pair comparisons was too much of a pain and wasn't worth it.
    // It makes a lot of references to the rules in ../Generated/Lua.Generated.g4
    internal class SyntaxNormalizer : LuaSyntaxRewriter
    {
        private readonly TextSpan _consideredSpan;
        private readonly string _indentWhitespace;
        private readonly SyntaxTrivia _eolTrivia;
        private readonly bool _useElasticTrivia;

        private bool _isInStructuredTrivia;

        private SyntaxToken _previousToken;

        private bool _afterLineBreak;
        private bool _afterIndentation;

        private readonly Dictionary<SyntaxToken, int> _lineBreaksAfterToken = new();
        private readonly HashSet<SyntaxToken> _tokensWithSpaceRequiredBefore = new();
        private readonly HashSet<SyntaxToken> _tokensWithSpaceRequiredAfter = new();

        private int _indentationDepth;

        private ArrayBuilder<SyntaxTrivia>? _indentations;

        private readonly record struct State(bool InStructuredTrivia, SyntaxToken PreviousToken, bool AfterLineBreak, bool AfterIndentation, int IndentationDepth);

        private SyntaxNormalizer(TextSpan consideredSpan, string indentWhitespace, string eolWhitespace, bool useElasticTrivia)
            : base(visitIntoStructuredTrivia: true)
        {
            _consideredSpan = consideredSpan;
            _indentWhitespace = indentWhitespace;
            _useElasticTrivia = useElasticTrivia;
            _eolTrivia = useElasticTrivia ? SyntaxFactory.ElasticEndOfLine(eolWhitespace) : SyntaxFactory.EndOfLine(eolWhitespace);
            _afterLineBreak = true;
        }

        internal static TNode Normalize<TNode>(TNode node, string indentWhitespace, string eolWhitespace, bool useElasticTrivia = false)
            where TNode : SyntaxNode
        {
            var normalizer = new SyntaxNormalizer(node.FullSpan, indentWhitespace, eolWhitespace, useElasticTrivia);
            var result = (TNode) normalizer.Visit(node);
            normalizer.Free();
            return result;
        }

        internal static SyntaxToken Normalize(SyntaxToken token, string indentWhitespace, string eolWhitespace, bool useElasticTrivia = false)
        {
            var normalizer = new SyntaxNormalizer(token.FullSpan, indentWhitespace, eolWhitespace, useElasticTrivia);
            var result = normalizer.VisitToken(token);
            normalizer.Free();
            return result;
        }

        internal static SyntaxTriviaList Normalize(SyntaxTriviaList trivia, string indentWhitespace, string eolWhitespace, bool useElasticTrivia = false)
        {
            var normalizer = new SyntaxNormalizer(trivia.FullSpan, indentWhitespace, eolWhitespace, useElasticTrivia);
            var result = normalizer.RewriteTrivia(
                trivia,
                normalizer.GetIndentationDepth(),
                isTrailing: false,
                mustBeIndented: false,
                mustHaveSeparator: false,
                lineBreaksAfter: 0, lineBreaksBefore: 0);
            normalizer.Free();
            return result;
        }

        private void Free() => _indentations?.Free();

        /// <summary>
        /// Obtains the indentaion for the provided depth.
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        private SyntaxTrivia GetIndentation(int depth)
        {
            var capacity = depth + 1;
            if (_indentations == null)
            {
                _indentations = ArrayBuilder<SyntaxTrivia>.GetInstance(capacity);
            }
            else
            {
                _indentations.EnsureCapacity(capacity);
            }

            // grow indentation collection if necessary
            for (var i = _indentations.Count; i <= depth; i++)
            {
                var text = i == 0
                    ? ""
                    : _indentations[i - 1].ToString() + _indentWhitespace;
                _indentations.Add(_useElasticTrivia ? SyntaxFactory.ElasticWhitespace(text) : SyntaxFactory.Whitespace(text));
            }

            return _indentations[depth];
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            // ignore tokens with no content
            if (token.IsKind(SyntaxKind.None))
                return token;

            try
            {
                var tk = token; // Make a copy of the token.

                var indentationDepth = GetIndentationDepth();

                // check if this token is first on this line
                var numLineBreaksBefore = LineBreaksBetween(_previousToken, token);

                var needsIndentation = numLineBreaksBefore > 0;

                // all line breaks except the first will be leading trivia of this token. The first line break
                // is trailing trivia of the previous token.
                if (numLineBreaksBefore > 0 && IsLastTokenOnLine(_previousToken))
                    _ = 1;

                tk = tk.WithLeadingTrivia(
                    RewriteTrivia(
                        token.LeadingTrivia,
                        indentationDepth,
                        isTrailing: false,
                        mustBeIndented: needsIndentation,
                        mustHaveSeparator: _tokensWithSpaceRequiredBefore.Contains(token),
                        lineBreaksAfter: 0,
                        lineBreaksBefore: 0));

                var nextToken = GetNextRelevantToken(token);

                _afterIndentation = false;

                // we only add one of the line breaks to trivia of this token. The remaining ones will be leading trivia
                // for the next token
                var numLineBreaksAfter = LineBreaksBetween(token, nextToken) > 0 ? 1 : 0;
                var needsSeparatorAfter = numLineBreaksAfter <= 0 && _tokensWithSpaceRequiredAfter.Contains(token);

                tk = tk.WithTrailingTrivia(
                    RewriteTrivia(
                        token.TrailingTrivia,
                        depth: 0,
                        isTrailing: true,
                        mustBeIndented: false,
                        mustHaveSeparator: needsSeparatorAfter,
                        lineBreaksAfter: numLineBreaksAfter,
                        lineBreaksBefore: 0));

                return tk;
            }
            finally
            {
                // to help debugging
                _previousToken = token;
            }
        }

        private SyntaxTriviaList RewriteTrivia(
            SyntaxTriviaList triviaList,
            int depth,
            bool isTrailing,
            bool mustBeIndented,
            bool mustHaveSeparator,
            int lineBreaksAfter,
            int lineBreaksBefore)
        {
            var currentTriviaList = ArrayBuilder<SyntaxTrivia>.GetInstance();
            try
            {
                for (var idx = 1; idx <= lineBreaksBefore; idx++)
                {
                    currentTriviaList.Add(GetEndOfLine());
                    _afterLineBreak = true;
                    _afterIndentation = false;
                }

                foreach (var trivia in triviaList)
                {
                    // keep non-whitespace trivia
                    if (trivia.Kind() is SyntaxKind.WhitespaceTrivia or SyntaxKind.EndOfLineTrivia || trivia.FullWidth == 0)
                        continue;

                    // check if there's a separator or a line break needed between the trivia itself
                    var tokenParent = trivia.Token.Parent;
                    var needsSeparator = currentTriviaList.Count == 0 && isTrailing;
                    var needsLineBreak = NeedsLineBreakBefore(trivia) || (currentTriviaList.Count > 0 && NeedsLineBreakBetween(currentTriviaList.Last(), trivia, isTrailing));

                    if (needsLineBreak && !_afterLineBreak)
                    {
                        currentTriviaList.Add(GetEndOfLine());
                        _afterLineBreak = true;
                        _afterIndentation = false;
                    }

                    if (_afterLineBreak && !isTrailing)
                    {
                        if (!_afterIndentation && NeedsIndentAfterLineBreak(trivia))
                        {
                            currentTriviaList.Add(GetIndentation(GetIndentationDepth()));
                            _afterIndentation = true;
                        }
                    }
                    else if (needsSeparator)
                    {
                        currentTriviaList.Add(GetSpace());
                        _afterLineBreak = false;
                        _afterIndentation = false;
                    }

                    if (trivia.HasStructure)
                    {
                        var structuredTrivia = VisitStructuredTrivia(trivia);
                        currentTriviaList.Add(structuredTrivia);
                    }
                    else
                    {
                        currentTriviaList.Add(trivia);
                    }

                    if (NeedsLineBreakAfter(trivia, isTrailing))
                    {
                        currentTriviaList.Add(GetEndOfLine());
                        _afterLineBreak = true;
                        _afterIndentation = false;
                    }
                    else
                    {
                        _afterLineBreak = EndsInLineBreak(trivia);
                    }
                } // end foreach

                if (lineBreaksAfter > 0)
                {
                    if (currentTriviaList.Count > 0 && EndsInLineBreak(currentTriviaList.Last()))
                        lineBreaksAfter -= 1;

                    for (var idx = 0; idx < lineBreaksAfter; idx++)
                    {
                        currentTriviaList.Add(GetEndOfLine());
                        _afterLineBreak = true;
                        _afterIndentation = false;
                    }
                }
                else if (mustHaveSeparator
                    && (!currentTriviaList.Any() || currentTriviaList.Last().Kind() is not (SyntaxKind.WhitespaceTrivia or SyntaxKind.EndOfLineTrivia)))
                {
                    currentTriviaList.Add(GetSpace());
                    _afterLineBreak = false;
                    _afterIndentation = false;
                }

                if (mustBeIndented)
                {
                    currentTriviaList.Add(GetIndentation(depth));
                    _afterIndentation = true;
                    _afterLineBreak = false;
                }

                if (currentTriviaList.Count == 0)
                    return _useElasticTrivia ? SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker) : default;
                else if (currentTriviaList.Count == 1)
                    return SyntaxFactory.TriviaList(currentTriviaList.First());
                else
                    return SyntaxFactory.TriviaList(currentTriviaList);
            }
            finally
            {
                currentTriviaList.Free();
            }
        }

        private static bool IsLastTokenOnLine(SyntaxToken token) =>
            token.TrailingTrivia.Any() && token.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia);

        private int LineBreaksBetween(SyntaxToken currentToken, SyntaxToken nextToken)
        {
            // First and last token may be of kind none
            if (currentToken.IsKind(SyntaxKind.None) || nextToken.IsKind(SyntaxKind.None))
                return 0;

            return _lineBreaksAfterToken.TryGetValue(currentToken, out var numLineBreaks)
                ? numLineBreaks
                : 0;
        }

        private int GetIndentationDepth()
        {
            LorettaDebug.Assert(_indentationDepth >= 0);
            return _indentationDepth;
        }

        private SyntaxTrivia GetSpace() => _useElasticTrivia ? SyntaxFactory.ElasticSpace : SyntaxFactory.Space;

        private SyntaxTrivia GetEndOfLine() => _eolTrivia;

        private static bool NeedsLineBreakBetween(SyntaxTrivia trivia, SyntaxTrivia nextTrivia, bool isTrailingTrivia)
        {
            if (EndsInLineBreak(trivia))
                return false;

            return nextTrivia.Kind() switch
            {
                SyntaxKind.SingleLineCommentTrivia => !isTrailingTrivia,
                _ => false,
            };
        }

        private static bool NeedsLineBreakAfter(SyntaxTrivia trivia, bool isTrailingTrivia)
        {
            var kind = trivia.Kind();
            return kind switch
            {
                SyntaxKind.SingleLineCommentTrivia => true,
                SyntaxKind.MultiLineCommentTrivia => !isTrailingTrivia,
                _ => false
            };
        }

#pragma warning disable IDE0079 // Remove unnecessary suppression
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Will be used when we have doc comments.")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
        // Left here for when we add documentation comments
        private static bool NeedsLineBreakBefore(SyntaxTrivia trivia) =>
            //if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) or SyntaxKind.MultiLineDocumentationCommentTrivia)
            //    return true;
            false;

        private static bool NeedsIndentAfterLineBreak(SyntaxTrivia trivia) =>
            trivia.Kind() is SyntaxKind.SingleLineCommentTrivia;

        private static bool EndsInLineBreak(SyntaxTrivia trivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                return true;

            if (trivia.HasStructure)
            {
                var structureLastToken = trivia.GetStructure()!.GetLastToken();
                if (structureLastToken.HasTrailingTrivia && structureLastToken.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia))
                    return true;
            }

            return false;
        }

        private SyntaxTrivia VisitStructuredTrivia(SyntaxTrivia trivia)
        {
            var oldIsInStructuredTrivia = _isInStructuredTrivia;
            _isInStructuredTrivia = true;

            var oldPreviousToken = _previousToken;
            _previousToken = default;

            var result = VisitTrivia(trivia);

            _isInStructuredTrivia = oldIsInStructuredTrivia;
            _previousToken = oldPreviousToken;

            return result;
        }

        private SyntaxToken GetNextRelevantToken(SyntaxToken token)
        {
            var nextToken = token.GetNextToken(
                static t => t.Kind() != SyntaxKind.None,
                static t => false);

            return _consideredSpan.Contains(nextToken.FullSpan) ? nextToken : default;
        }

        private void AddLineBreaksAfterElements<TNode>(SyntaxList<TNode> list, int lineBreaksBetweenElements = 1, int lineBreaksAfterLastElement = 0)
            where TNode : SyntaxNode
        {
            var lastElementIndex = list.Count - 1;
            for (var idx = 0; idx < list.Count; idx++)
            {
                var listElement = list[idx];
                AddLineBreaksAfterToken(
                    listElement.GetLastToken(),
                    idx == lastElementIndex ? lineBreaksAfterLastElement : lineBreaksBetweenElements);
            }
        }

        private void AddLineBreaksAfterToken(SyntaxToken node, int lineBreaksAfterToken = 1)
        {
            // Ignore if we get provided 0.
            if (lineBreaksAfterToken == 0)
                return;
            _lineBreaksAfterToken[node] = lineBreaksAfterToken;
        }

        private void AddSpaceBeforeToken(SyntaxToken token) => _tokensWithSpaceRequiredBefore.Add(token);
        private void AddSpaceAfterToken(SyntaxToken token) => _tokensWithSpaceRequiredAfter.Add(token);
        private void AddSpacesAroundToken(SyntaxToken token)
        {
            AddSpaceBeforeToken(token);
            AddSpaceAfterToken(token);
        }

        private State GetState() =>
            new(_isInStructuredTrivia, _previousToken, _afterLineBreak, _afterIndentation, _indentationDepth);
        private void RestoreState(State state) =>
            (_isInStructuredTrivia, _previousToken, _afterLineBreak, _afterIndentation, _indentationDepth) = state;

        private TReturn WithTempState<TArg, TReturn>(Func<TArg, TReturn> func, TArg arg)
        {
            var state = GetState();
            var ret = func(arg);
            RestoreState(state);
            return ret;
        }

        // statement_list
        //   : statement*
        //   ;
        public override SyntaxNode? VisitStatementList(StatementListSyntax node)
        {
            var shouldNotIndentBody = node.Parent is CompilationUnitSyntax or null;
            try
            {
                if (!shouldNotIndentBody)
                    _indentationDepth++;
                AddLineBreaksAfterElements(node.Statements);

                return base.VisitStatementList(node);
            }
            finally
            {
                if (!shouldNotIndentBody)
                    _indentationDepth--;
            }
        }

        // statement
        //   : assignment_statement
        //   | break_statement
        //   | compound_assignment_statement
        //   | continue_statement
        //   | do_statement
        //   | empty_statement
        //   | expression_statement
        //   | function_declaration_statement
        //   | generic_for_statement
        //   | goto_label_statement
        //   | goto_statement
        //   | if_statement
        //   | local_function_declaration_statement
        //   | local_variable_declaration_statement
        //   | numeric_for_statement
        //   | repeat_until_statement
        //   | return_statement
        //   | type_declaration_statement
        //   | while_statement
        //   ;
        #region Statements

        // assignment_statement
        //   : prefix_expression (',' prefix_expression)* equals_values_clause ';'?
        //   ;
        public override SyntaxNode? VisitAssignmentStatement(AssignmentStatementSyntax node)
        {
            foreach (var assigneeSeparator in node.Variables.GetSeparators())
                AddSpaceAfterToken(assigneeSeparator);

            return base.VisitAssignmentStatement(node);
        }

        // equals_values_clause
        //   : '=' expression (',' expression)*
        //   ;
        public override SyntaxNode? VisitEqualsValuesClause(EqualsValuesClauseSyntax node)
        {
            AddSpacesAroundToken(node.EqualsToken);
            foreach (var valueSeparator in node.Values.GetSeparators())
                AddSpaceAfterToken(valueSeparator);

            return base.VisitEqualsValuesClause(node);
        }

        // Visiting BreakStatementSyntax isn't necessary as it has no spacing.

        // compound_assignment_statement
        //   : prefix_expression ('+=' | '-=' | syntax_token | '/=' | '%=' | '..=' | '^=') expression ';'?
        //   ;
        public override SyntaxNode? VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node)
        {
            AddSpacesAroundToken(node.AssignmentOperatorToken);

            return base.VisitCompoundAssignmentStatement(node);
        }

        // Visiting ContinueStatementSyntax isn't necessary as it has no spacing.

        // do_statement
        //   : 'do' statement_list 'end' ';'?
        //   ;
        public override SyntaxNode? VisitDoStatement(DoStatementSyntax node)
        {
            AddLineBreaksAfterToken(node.DoKeyword, 1);
            AddLineBreaksAfterToken(node.Body.GetLastToken(), 1);

            return base.VisitDoStatement(node);
        }

        // Visiting EmptyStatementSyntax isn't necessary as it has no spacing.
        // Visiting ExpressionStatementSyntax isn't necessary as it has no spacing.

        // function_declaration_statement
        //   : 'function' function_name type_parameter_list? parameter_list type_binding? statement_list 'end' ';'?
        //   ;
        public override SyntaxNode? VisitFunctionDeclarationStatement(FunctionDeclarationStatementSyntax node)
        {
            AddSpaceAfterToken(node.FunctionKeyword);
            AddLineBreaksAfterToken(node.TypeBinding is not null ? node.TypeBinding.GetLastToken() : node.Parameters.GetLastToken());
            AddLineBreaksAfterToken(node.Body.GetLastToken());

            return base.VisitFunctionDeclarationStatement(node);
        }

        // Visiting MemberFunctionNameSyntax isn't necessary as it has no spacing.
        // Visiting MethodFunctionNameSyntax isn't necessary as it has no spacing.
        // Visiting SimpleFunctionNameSyntax isn't necessary as it has no spacing.

        // parameter_list
        //   : '(' (parameter (',' parameter)*)? ')'
        //   ;
        public override SyntaxNode? VisitParameterList(ParameterListSyntax node)
        {
            foreach (var parameterSeparator in node.Parameters.GetSeparators())
                AddSpaceAfterToken(parameterSeparator);

            return base.VisitParameterList(node);
        }

        // type_parameter_list
        //   : '<' (type_parameter (',' type_parameter)*)? '>'
        //   ;
        public override SyntaxNode? VisitTypeParameterList(TypeParameterListSyntax node)
        {
            foreach (var parameterSeparator in node.Names.GetSeparators())
                AddSpaceAfterToken(parameterSeparator);

            return base.VisitTypeParameterList(node);
        }

        // Visiting TypeParameterSyntax isn't necessary as it has no spacing requirements.

        // equals_type
        //   : '=' type
        //   ;
        public override SyntaxNode? VisitEqualsType(EqualsTypeSyntax node)
        {
            AddSpacesAroundToken(node.EqualsToken);

            return base.VisitEqualsType(node);
        }

        // type_binding
        //   : ':' type
        //   ;
        public override SyntaxNode? VisitTypeBinding(TypeBindingSyntax node)
        {
            AddSpaceAfterToken(node.ColonToken);

            return base.VisitTypeBinding(node);
        }

        // generic_for_statement
        //   : 'for' typed_identifier_name (',' typed_identifier_name)* 'in' expression (',' expression)* 'do' statement_list 'end' ';'?
        //   ;
        public override SyntaxNode? VisitGenericForStatement(GenericForStatementSyntax node)
        {
            AddSpaceAfterToken(node.ForKeyword);
            foreach (var identSeparator in node.Identifiers.GetSeparators())
                AddSpaceAfterToken(identSeparator);
            AddSpacesAroundToken(node.InKeyword);
            foreach (var expressionSeparator in node.Expressions.GetSeparators())
                AddSpaceAfterToken(expressionSeparator);
            AddSpaceBeforeToken(node.DoKeyword);
            AddLineBreaksAfterToken(node.DoKeyword);
            AddLineBreaksAfterToken(node.Body.GetLastToken());

            return base.VisitGenericForStatement(node);
        }

        // Visiting TypedIdentifierName is not necessary as it has no spacing.
        // Visiting GotoLabelStatementSyntax is not necessary as it has no spacing.

        // goto_statement
        //   : 'goto' identifier_token ';'?
        //   ;
        public override SyntaxNode? VisitGotoStatement(GotoStatementSyntax node)
        {
            AddSpaceAfterToken(node.GotoKeyword);

            return base.VisitGotoStatement(node);
        }

        // if_statement
        //   : 'if' expression 'then' statement_list else_if_clause* else_clause? 'end' ';'?
        //   ;
        public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
        {
            AddSpaceAfterToken(node.IfKeyword);
            AddSpaceBeforeToken(node.ThenKeyword);
            AddLineBreaksAfterToken(node.ThenKeyword);
            AddLineBreaksAfterToken(node.Body.GetLastToken());

            return base.VisitIfStatement(node);
        }

        // else_if_clause
        //   : 'elseif' expression 'then' statement_list
        //   ;
        public override SyntaxNode? VisitElseIfClause(ElseIfClauseSyntax node)
        {
            AddSpaceAfterToken(node.ElseIfKeyword);
            AddSpaceBeforeToken(node.ThenKeyword);
            AddLineBreaksAfterToken(node.ThenKeyword);
            AddLineBreaksAfterToken(node.Body.GetLastToken());

            return base.VisitElseIfClause(node);
        }

        // else_clause
        //   : 'else' statement_list
        //   ;
        public override SyntaxNode? VisitElseClause(ElseClauseSyntax node)
        {
            AddLineBreaksAfterToken(node.ElseKeyword);
            AddLineBreaksAfterToken(node.ElseBody.GetLastToken());

            return base.VisitElseClause(node);
        }

        // local_function_declaration_statement
        //   : 'local' 'function' identifier_name type_parameter_list? parameter_list type_binding? statement_list 'end' ';'?
        //   ;
        public override SyntaxNode? VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatementSyntax node)
        {
            AddSpaceAfterToken(node.LocalKeyword);
            AddSpaceAfterToken(node.FunctionKeyword);
            AddLineBreaksAfterToken(node.TypeBinding is not null ? node.TypeBinding.GetLastToken() : node.Parameters.GetLastToken());
            AddLineBreaksAfterToken(node.Body.GetLastToken());

            return base.VisitLocalFunctionDeclarationStatement(node);
        }

        // local_variable_declaration_statement
        //   : 'local' local_declaration_name (',' local_declaration_name)* equals_values_clause? ';'?
        //   ;
        public override SyntaxNode? VisitLocalVariableDeclarationStatement(LocalVariableDeclarationStatementSyntax node)
        {
            AddSpaceAfterToken(node.LocalKeyword);
            foreach (var nameSeparator in node.Names.GetSeparators())
                AddSpaceAfterToken(nameSeparator);

            return base.VisitLocalVariableDeclarationStatement(node);
        }

        // local_declaration_name
        //   : identifier_name variable_attribute? type_binding?
        //   ;
        public override SyntaxNode? VisitLocalDeclarationName(LocalDeclarationNameSyntax node)
        {
            if (node.Attribute is not null)
                AddSpaceAfterToken(node.IdentifierName.GetLastToken());

            return base.VisitLocalDeclarationName(node);
        }

        // numeric_for_statement
        //   : 'for' typed_identifier_name '=' expression ',' expression (',' expression) 'do' statement_list 'end' ';'?
        //   ;
        public override SyntaxNode? VisitNumericForStatement(NumericForStatementSyntax node)
        {
            AddSpaceAfterToken(node.ForKeyword);
            AddSpacesAroundToken(node.EqualsToken);
            AddSpaceAfterToken(node.FinalValueCommaToken);
            AddSpaceAfterToken(node.StepValueCommaToken);
            AddSpaceBeforeToken(node.DoKeyword);
            AddLineBreaksAfterToken(node.DoKeyword);
            AddLineBreaksAfterToken(node.Body.GetLastToken());

            return base.VisitNumericForStatement(node);
        }

        // repeat_until_statement
        //   : 'repeat' statement_list 'until' expression ';'?
        //   ;
        public override SyntaxNode? VisitRepeatUntilStatement(RepeatUntilStatementSyntax node)
        {
            AddLineBreaksAfterToken(node.RepeatKeyword);
            AddLineBreaksAfterToken(node.Body.GetLastToken());
            AddSpaceAfterToken(node.UntilKeyword);

            return base.VisitRepeatUntilStatement(node);
        }

        // return_statement
        //   : 'return' (expression (',' expression)*)? ';'?
        //   ;
        public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node)
        {
            AddSpaceAfterToken(node.ReturnKeyword);
            foreach (var expressionSeparator in node.Expressions.GetSeparators())
                AddSpaceAfterToken(expressionSeparator);

            return base.VisitReturnStatement(node);
        }

        // type_declaration_statement
        //   : 'export'? 'type' identifier_token type_parameter_list? '=' type ';'?
        //   ;
        public override SyntaxNode? VisitTypeDeclarationStatement(TypeDeclarationStatementSyntax node)
        {
            AddSpaceAfterToken(node.ExportKeyword);
            AddSpaceAfterToken(node.TypeKeyword);
            AddSpacesAroundToken(node.EqualsToken);

            return base.VisitTypeDeclarationStatement(node);
        }

        // while_statement
        //   : 'while' expression 'do' statement_list 'end' ';'?
        //   ;
        public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node)
        {
            AddSpaceAfterToken(node.WhileKeyword);
            AddSpaceBeforeToken(node.DoKeyword);
            AddLineBreaksAfterToken(node.DoKeyword);
            AddLineBreaksAfterToken(node.Body.GetLastToken());

            return base.VisitWhileStatement(node);
        }

        #endregion Statements

        // type
        //   : function_type
        //   | generic_type_pack
        //   | intersection_type
        //   | literal_type
        //   | nilable_type
        //   | parenthesized_type
        //   | table_based_type
        //   | type_name
        //   | type_pack
        //   | typeof_type
        //   | union_type
        //   | variadic_type_pack
        //   ;
        #region Types

        // function_type
        //   : type_parameter_list? '(' (type (',' type)*)? ')' '->' type
        //   ;
        public override SyntaxNode? VisitFunctionType(FunctionTypeSyntax node)
        {
            foreach (var parameterSeparator in node.Parameters.GetSeparators())
                AddSpaceAfterToken(parameterSeparator);
            AddSpacesAroundToken(node.MinusGreaterThanToken);

            return base.VisitFunctionType(node);
        }

        // Visiting GenericTypePackSyntax is not necessary as it has no spacing.

        // intersection_type
        //   : type '&' type
        //   ;
        public override SyntaxNode? VisitIntersectionType(IntersectionTypeSyntax node)
        {
            AddSpacesAroundToken(node.AmpersandToken);

            return base.VisitIntersectionType(node);
        }

        // Visiting LiteralTypeSyntax is not necessary as it has no spacing.
        // Visiting NilableTypeSyntax is not necessary as it has no spacing.
        // Visiting ParenthesizedTypeSyntax is not necessary as it has no spacing.

        // array_type
        //   : '{' type '}'
        //   ;
        public override SyntaxNode? VisitArrayType(ArrayTypeSyntax node)
        {
            AddSpaceAfterToken(node.OpenBraceToken);
            AddSpaceBeforeToken(node.CloseBraceToken);

            return base.VisitArrayType(node);
        }

        // table_type
        //   : '{' (table_type_element (',' table_type_element)*)? '}'
        //   ;
        public override SyntaxNode? VisitTableType(TableTypeSyntax node)
        {
            if (node.Elements.Any())
            {
                AddSpaceAfterToken(node.OpenBraceToken);
                foreach (var elementSeparator in node.Elements.GetSeparators())
                    AddSpaceAfterToken(elementSeparator);
                AddSpaceBeforeToken(node.CloseBraceToken);
            }

            return base.VisitTableType(node);
        }

        // table_type_indexer
        //   : '[' type ']' ':' type
        //   ;
        public override SyntaxNode? VisitTableTypeIndexer(TableTypeIndexerSyntax node)
        {
            AddSpaceAfterToken(node.ColonToken);

            return base.VisitTableTypeIndexer(node);
        }

        // table_type_property
        //   : identifier_token ':' type
        //   ;
        public override SyntaxNode? VisitTableTypeProperty(TableTypePropertySyntax node)
        {
            AddSpaceAfterToken(node.ColonToken);

            return base.VisitTableTypeProperty(node);
        }

        // Visiting CompositeTypeNameSyntax is not necessary as it has no spacing.
        // Visiting SimpleTypeNameSyntax is not necessary as it has no spacing.

        // type_argument_list
        //   : '<' (type (',' type)*)? '>'
        //   ;
        public override SyntaxNode? VisitTypeArgumentList(TypeArgumentListSyntax node)
        {
            foreach (var argumentSeparator in node.Arguments.GetSeparators())
                AddSpaceAfterToken(argumentSeparator);

            return base.VisitTypeArgumentList(node);
        }

        // type_pack
        //   : '(' (type (',' type)*)? ')'
        //   ;
        public override SyntaxNode? VisitTypePack(TypePackSyntax node)
        {
            foreach (var typeSeparator in node.Types.GetSeparators())
                AddSpaceAfterToken(typeSeparator);

            return base.VisitTypePack(node);
        }

        // Visiting TypeofTypeSyntax is not necessary as it has no spacing.

        // union_type
        //   : type '|' type
        //   ;
        public override SyntaxNode? VisitUnionType(UnionTypeSyntax node)
        {
            AddSpacesAroundToken(node.PipeToken);

            return base.VisitUnionType(node);
        }

        // Visiting VariadicTypePackSyntax is not necessary as it has no spacing.

        #endregion Types

        // expression
        //   : anonymous_function_expression
        //   | binary_expression
        //   | if_expression
        //   | literal_expression
        //   | prefix_expression
        //   | table_constructor_expression
        //   | type_cast_expression
        //   | unary_expression
        //   | var_arg_expression
        //   ;
        #region Expressions

        // anonymous_function_expression
        //   : 'function' type_parameter_list? parameter_list type_binding? statement_list 'end'
        //   ;
        public override SyntaxNode? VisitAnonymousFunctionExpression(AnonymousFunctionExpressionSyntax node)
        {
            AddLineBreaksAfterToken(node.TypeBinding is not null ? node.TypeBinding.GetLastToken() : node.Parameters.GetLastToken());
            AddLineBreaksAfterToken(node.Body.GetLastToken());

            return base.VisitAnonymousFunctionExpression(node);
        }

        // binary_expression
        //   : expression ('&&' | '&' | 'and' | '!=' | '..' | '==' | '>=' | '>>' | '>' | '^' | '<=' | '<<' | '<' | '-' | 'or' | '%' | '||' | '|' | '+' | '/' | '*' | '~=' | '~' | '//') expression
        //   ;
        public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            AddSpacesAroundToken(node.OperatorToken);

            return base.VisitBinaryExpression(node);
        }

        // if_expression
        //   : 'if' expression 'then' expression else_if_expression_clause* 'else' expression
        //   ;
        public override SyntaxNode? VisitIfExpression(IfExpressionSyntax node)
        {
            AddSpaceAfterToken(node.IfKeyword);
            AddSpacesAroundToken(node.ThenKeyword);
            // We add spaces around because we aren't sure if there's an elseif clause or not
            // so we just preemptively add spaces and don't add them in the elseif visitor.
            AddSpacesAroundToken(node.ElseKeyword);

            return base.VisitIfExpression(node);
        }

        // else_if_expression_clause
        //   : 'elseif' expression 'then' expression
        //   ;
        public override SyntaxNode? VisitElseIfExpressionClause(ElseIfExpressionClauseSyntax node)
        {
            // We add around because this will always have an expression before it
            AddSpacesAroundToken(node.ElseIfKeyword);
            AddSpacesAroundToken(node.ThenKeyword);
            // No spaces after because the first line of this method and VisitIfExpression handle the space afterwards.

            return base.VisitElseIfExpressionClause(node);
        }

        // Visiting LiteralExpressionSyntax is not necessary as it has no spacing.

        // function_call_expression
        //   : prefix_expression function_argument
        //   ;
        public override SyntaxNode? VisitFunctionCallExpression(FunctionCallExpressionSyntax node)
        {
            if (node.Argument is not ExpressionListFunctionArgumentSyntax)
                AddSpaceAfterToken(node.Expression.GetLastToken());

            return base.VisitFunctionCallExpression(node);
        }

        // method_call_expression
        //   : prefix_expression ':' identifier_token function_argument
        //   ;
        public override SyntaxNode? VisitMethodCallExpression(MethodCallExpressionSyntax node)
        {
            if (node.Argument is not ExpressionListFunctionArgumentSyntax)
                AddSpaceAfterToken(node.Identifier);

            return base.VisitMethodCallExpression(node);
        }

        // expression_list_function_argument
        // : '(' (expression (',' expression)*)? ')'
        // ;
        public override SyntaxNode? VisitExpressionListFunctionArgument(ExpressionListFunctionArgumentSyntax node)
        {
            foreach (var expressionSeparator in node.Expressions.GetSeparators())
                AddSpaceAfterToken(expressionSeparator);

            return base.VisitExpressionListFunctionArgument(node);
        }

        // Visiting ParenthesizedExpressionSyntax is not necessary as it has no spacing.
        // Visiting ElementAccessExpressionSyntax is not necessary as it has no spacing.
        // Visiting MemberAccessExpressionSyntax is not necessary as it has no spacing.
        // Visiting IdentifierNameSyntax is not necessary as it has no spacing.

        // table_constructor_expression
        //   : '{' (table_field (',' table_field)* ','?)? '}'
        //   ;
        public override SyntaxNode? VisitTableConstructorExpression(TableConstructorExpressionSyntax node)
        {
            // First visit is only used to define whether or not it'll be a multi-line table.
            var multiLineTable = WithTempState(VisitList, node.Fields).Any(IsMultiLineNode);

            if (multiLineTable)
            {
                foreach (var fieldSeparator in node.Fields.GetSeparators())
                    AddLineBreaksAfterToken(fieldSeparator);
            }
            else
            {
                foreach (var fieldSeparator in node.Fields.GetSeparators())
                    AddSpaceAfterToken(fieldSeparator);
            }

            if (node.Fields.Any())
            {
                if (multiLineTable)
                {
                    AddLineBreaksAfterToken(node.OpenBraceToken);
                    // Only need to do something if there's no trailing separator
                    if (node.Fields.SeparatorCount < node.Fields.Count)
                        AddLineBreaksAfterToken(node.Fields.Last().GetLastToken());
                }
                else
                {
                    AddSpaceAfterToken(node.OpenBraceToken);
                    // Only need to do something if there's no trailing separator
                    if (node.Fields.SeparatorCount < node.Fields.Count)
                        AddSpaceBeforeToken(node.CloseBraceToken);
                }
            }

            var openBraceToken = VisitToken(node.OpenBraceToken);
            if (multiLineTable)
                _indentationDepth++;
            var fields = VisitList(node.Fields);
            if (multiLineTable)
                _indentationDepth--;
            var closeBraceToken = VisitToken(node.CloseBraceToken);

            return node.Update(openBraceToken, fields, closeBraceToken);
        }

        // expression_keyed_table_field
        //   : '[' expression ']' '=' expression
        //   ;
        public override SyntaxNode? VisitExpressionKeyedTableField(ExpressionKeyedTableFieldSyntax node)
        {
            AddSpacesAroundToken(node.EqualsToken);

            return base.VisitExpressionKeyedTableField(node);
        }

        // identifier_keyed_table_field
        //   : identifier_token '=' expression
        //   ;
        public override SyntaxNode? VisitIdentifierKeyedTableField(IdentifierKeyedTableFieldSyntax node)
        {
            AddSpacesAroundToken(node.EqualsToken);

            return base.VisitIdentifierKeyedTableField(node);
        }

        // Visiting UnkeyedTableFieldSyntax is not necessary as it has no spacing.

        // type_cast_expression
        //   : expression '::' type
        //   ;
        public override SyntaxNode? VisitTypeCastExpression(TypeCastExpressionSyntax node)
        {
            AddSpacesAroundToken(node.ColonColonToken);

            return base.VisitTypeCastExpression(node);
        }

        // unary_expression
        //   : '!' expression
        //   | '#' expression
        //   | '-' expression
        //   | 'not' expression
        //   | '~' expression
        //   ;
        public override SyntaxNode? VisitUnaryExpression(UnaryExpressionSyntax node)
        {
            if (SyntaxFacts.IsKeyword(node.OperatorToken.Kind()))
                AddSpaceAfterToken(node.OperatorToken);

            if (node.OperatorToken.Kind() is SyntaxKind.MinusToken && node.Operand is UnaryExpressionSyntax { OperatorToken.RawKind: (int) SyntaxKind.MinusToken })
                AddSpaceAfterToken(node.OperatorToken);

            return base.VisitUnaryExpression(node);
        }

        // Visiting VarArgExpressionSyntax is not necessary as it has no spacing.

        #endregion Expressions

        private static bool IsMultiLineNode(SyntaxNode node) =>
            node.DescendantTrivia().Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }
}
