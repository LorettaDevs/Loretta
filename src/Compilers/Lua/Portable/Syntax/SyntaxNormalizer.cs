// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax
{
    // This is based on VB's SyntaxNormalizer as it is more similar to Lua than C#
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

        private readonly Dictionary<SyntaxToken, int> _lineBreaksAfterToken = new Dictionary<SyntaxToken, int>();
        private readonly HashSet<SyntaxNode> _lastStatementsInBlocks = new HashSet<SyntaxNode>();

        private int _indentationDepth;

        private ArrayBuilder<SyntaxTrivia>? _indentations;

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
                    numLineBreaksBefore -= 1;

                tk = tk.WithLeadingTrivia(
                    RewriteTrivia(
                        token.LeadingTrivia,
                        indentationDepth,
                        isTrailing: false,
                        mustBeIndented: needsIndentation,
                        mustHaveSeparator: false,
                        lineBreaksAfter: 0,
                        lineBreaksBefore: numLineBreaksBefore));

                var nextToken = GetNextRelevantToken(token);

                _afterIndentation = false;

                // we only add one of the line breaks to trivia of this token. The remaining ones will be leading trivia 
                // for the next token
                var numLineBreaksAfter = LineBreaksBetween(token, nextToken) > 0 ? 1 : 0;
                var needsSeparatorAfter = numLineBreaksAfter <= 0 && NeedsSeparator(token, nextToken);

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

                    if (NeedsLineBreakAfter(trivia))
                    {
                        if (!isTrailing)
                        {
                            currentTriviaList.Add(GetEndOfLine());
                            _afterLineBreak = true;
                            _afterIndentation = false;
                        }
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
                else if (mustHaveSeparator)
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
            RoslynDebug.Assert(_indentationDepth >= 0);
            return _indentationDepth;
        }

        private SyntaxTrivia GetSpace() => _useElasticTrivia ? SyntaxFactory.ElasticSpace : SyntaxFactory.Space;

        private SyntaxTrivia GetEndOfLine() => _eolTrivia;

        private static bool NeedsLineBreakBetween(SyntaxTrivia trivia, SyntaxTrivia nextTrivia, bool isTrailingTrivia)
        {
            if (EndsInLineBreak(trivia))
                return false;

            switch (nextTrivia.Kind())
            {
                case SyntaxKind.SingleLineCommentTrivia:
                    return !isTrailingTrivia;

                default:
                    return false;
            }
        }

        private static bool NeedsLineBreakAfter(SyntaxTrivia trivia) =>
            trivia.IsKind(SyntaxKind.SingleLineCommentTrivia);

        // Left here for when we add documentation comments
        private static bool NeedsLineBreakBefore(SyntaxTrivia trivia) =>
            //if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) or SyntaxKind.MultiLineDocumentationCommentTrivia)
            //    return true;
            false;

        private static bool NeedsIndentAfterLineBreak(SyntaxTrivia trivia) =>
            trivia.Kind() is SyntaxKind.SingleLineCommentTrivia;

        private static bool NeedsSeparator(SyntaxToken token, SyntaxToken nextToken)
        {
            if (token.IsKind(SyntaxKind.EndOfFileToken))
                return false;

            if (token.Parent is null || nextToken.IsKind(SyntaxKind.None))
                return false;

            if (nextToken.Parent.IsKind(SyntaxKind.AnonymousFunctionExpression))
                return true;

            if (nextToken.IsKind(SyntaxKind.EndOfFileToken))
                return false;

            // '+1' instead of '+ 1'
            // but 'not a' instead of 'nota'
            if (token.Parent is UnaryExpressionSyntax && !SyntaxFacts.IsKeyword(token.Kind()))
                return false;

            // generally 'a + b', needs to go here to make it 'b + (a + b)' instead of 'b +(a + b)'
            if (token.Parent is BinaryExpressionSyntax || nextToken.Parent is BinaryExpressionSyntax or ParenthesizedExpressionSyntax)
                return true;

            // '(a' instead of '( a'
            if (token.IsKind(SyntaxKind.OpenParenthesisToken))
                return false;

            // 'a)' instead of 'a )'
            if (nextToken.IsKind(SyntaxKind.CloseParenthesisToken))
                return false;

            // 'm(' instead of 'm ('
            if (!token.IsKind(SyntaxKind.CommaToken) && nextToken.IsKind(SyntaxKind.OpenParenthesisToken))
                return false;

            // '(,,,)' instead of '( , , ,)' or '(a, b)' instead of '(a , b)'
            if (nextToken.IsKind(SyntaxKind.CommaToken))
                return false;

            // 'a.b' instead of 'a . b' and 'a:b' instead of 'a : b'
            if (nextToken.Kind() is SyntaxKind.DotToken or SyntaxKind.ColonToken
                || token.Kind() is SyntaxKind.DotToken or SyntaxKind.ColonToken)
            {
                return false;
            }

            // '::a::' instead of ':: a ::'
            if (token.IsKind(SyntaxKind.ColonColonToken) || nextToken.IsKind(SyntaxKind.ColonColonToken))
                return false;

            // `a[a]` instead of `a [ a ]`
            if (nextToken.Kind() is SyntaxKind.OpenBracketToken or SyntaxKind.CloseBracketToken
                || token.Kind() is SyntaxKind.OpenBracketToken)
            {
                return false;
            }

            // '{}' instead of '{ }'
            if (token.IsKind(SyntaxKind.OpenBraceToken) && nextToken.IsKind(SyntaxKind.CloseBraceToken))
                return false;

            return true;
        }

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

        private void AddLineBreaksAfterElements<TNode>(SyntaxList<TNode> list, int lineBreaksBetweenElements, int lineBreaksAfterLastElement)
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

        private void AddLineBreaksAfterToken(SyntaxToken node, int lineBreaksAfterToken) =>
            _lineBreaksAfterToken[node] = lineBreaksAfterToken;

        private void MarkLastStatement<TNode>(SyntaxList<TNode> list)
            where TNode : SyntaxNode
        {
            if (list.Any())
                _lastStatementsInBlocks.Add(list.Last());
        }

        private void AddCorrectNumberOfLineBreaksToTokenIfNodeIsNotLast(SyntaxNode node, SyntaxToken token) =>
            AddLineBreaksAfterToken(token, _lastStatementsInBlocks.Contains(node) ? 1 : 2);

        public override SyntaxNode? VisitParameterList(ParameterListSyntax node)
        {
            AddLineBreaksAfterToken(node.CloseParenthesisToken, 1);
            return base.VisitParameterList(node);
        }

        public override SyntaxNode? VisitStatementList(StatementListSyntax node)
        {
            try
            {
                if (node.Parent is not CompilationUnitSyntax)
                    _indentationDepth += 1;
                AddLineBreaksAfterElements(node.Statements, 1, 1);
                MarkLastStatement(node.Statements);
                return base.VisitStatementList(node);
            }
            finally
            {
                if (node.Parent is not CompilationUnitSyntax)
                    _indentationDepth -= 1;
            }
        }

        public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
        {
            AddLineBreaksAfterToken(node.ThenKeyword, 1);

            LuaSyntaxNode? prevNode = null;
            if (node.Body.Statements.Any())
                prevNode = node.Body.Statements.Last();

            foreach (var elseifBlock in node.ElseIfClauses)
            {
                if (prevNode is not null)
                    AddLineBreaksAfterToken(prevNode.GetLastToken(), 1);
                prevNode = elseifBlock;
            }

            if (node.ElseClause is not null && prevNode is not null)
                AddLineBreaksAfterToken(prevNode.GetLastToken(), 1);

            AddCorrectNumberOfLineBreaksToTokenIfNodeIsNotLast(node, node.EndKeyword);

            return base.VisitIfStatement(node);
        }

        public override SyntaxNode? VisitElseIfClause(ElseIfClauseSyntax node)
        {
            AddLineBreaksAfterToken(node.ThenKeyword, 1);
            return base.VisitElseIfClause(node);
        }

        public override SyntaxNode? VisitElseClause(ElseClauseSyntax node)
        {
            AddLineBreaksAfterToken(node.ElseKeyword, 1);
            return base.VisitElseClause(node);
        }

        public override SyntaxNode? VisitDoStatement(DoStatementSyntax node)
        {
            AddLineBreaksAfterToken(node.DoKeyword, 1);
            AddCorrectNumberOfLineBreaksToTokenIfNodeIsNotLast(node, node.EndKeyword);
            return base.VisitDoStatement(node);
        }

        public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node)
        {
            AddLineBreaksAfterToken(node.DoKeyword, 1);
            AddCorrectNumberOfLineBreaksToTokenIfNodeIsNotLast(node, node.EndKeyword);
            return base.VisitWhileStatement(node);
        }

        public override SyntaxNode? VisitRepeatUntilStatement(RepeatUntilStatementSyntax node)
        {
            AddLineBreaksAfterToken(node.RepeatKeyword, 1);
            AddCorrectNumberOfLineBreaksToTokenIfNodeIsNotLast(node, node.GetLastToken());
            return base.VisitRepeatUntilStatement(node);
        }

        public override SyntaxNode? VisitNumericForStatement(NumericForStatementSyntax node)
        {
            AddLineBreaksAfterToken(node.DoKeyword, 1);
            AddCorrectNumberOfLineBreaksToTokenIfNodeIsNotLast(node, node.EndKeyword);
            return base.VisitNumericForStatement(node);
        }

        public override SyntaxNode? VisitGenericForStatement(GenericForStatementSyntax node)
        {
            AddLineBreaksAfterToken(node.DoKeyword, 1);
            AddCorrectNumberOfLineBreaksToTokenIfNodeIsNotLast(node, node.EndKeyword);
            return base.VisitGenericForStatement(node);
        }

        public override SyntaxNode? VisitFunctionDeclarationStatement(FunctionDeclarationStatementSyntax node)
        {
            AddLineBreaksAfterToken(node.Parameters.CloseParenthesisToken, 1);
            AddCorrectNumberOfLineBreaksToTokenIfNodeIsNotLast(node, node.EndKeyword);
            return base.VisitFunctionDeclarationStatement(node);
        }

        public override SyntaxNode? VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatementSyntax node)
        {
            AddLineBreaksAfterToken(node.Parameters.CloseParenthesisToken, 1);
            AddCorrectNumberOfLineBreaksToTokenIfNodeIsNotLast(node, node.EndKeyword);
            return base.VisitLocalFunctionDeclarationStatement(node);
        }

        public override SyntaxNode? VisitAnonymousFunctionExpression(AnonymousFunctionExpressionSyntax node)
        {
            AddLineBreaksAfterToken(node.Parameters.CloseParenthesisToken, 1);
            return base.VisitAnonymousFunctionExpression(node);
        }

        public override SyntaxNode? VisitTableConstructorExpression(TableConstructorExpressionSyntax node)
        {
            if ((node.Fields.Count == 1 && IsMultiLineNode(node.Fields.First()))
                || node.Fields.Count > 1)
            {
                AddLineBreaksAfterToken(node.OpenBraceToken, 1);
                foreach (var sep in node.Fields.GetSeparators())
                    AddLineBreaksAfterToken(sep, 1);
                if (node.Fields.Count != node.Fields.SeparatorCount)
                    AddLineBreaksAfterToken(node.Fields.Last().GetLastToken(), 1);

                var openBraceToken = VisitToken(node.OpenBraceToken);
                _indentationDepth++;
                var fields = VisitList(node.Fields);
                _indentationDepth--;
                var closeBraceToken = VisitToken(node.CloseBraceToken);
                return node.Update(openBraceToken, fields, closeBraceToken);
            }
            else
            {
                return base.VisitTableConstructorExpression(node);
            }
        }

        private static bool IsMultiLineNode(SyntaxNode node) =>
            node.DescendantTrivia().Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }
}
