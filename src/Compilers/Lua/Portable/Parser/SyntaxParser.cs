// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    using Loretta.CodeAnalysis.Syntax.InternalSyntax;
    using Loretta.Utilities;

    internal abstract partial class SyntaxParser : IDisposable
    {
        protected readonly Lexer _lexer;
        private readonly bool _isIncremental;
        protected readonly CancellationToken _cancellationToken;

        private Blender _firstBlender;
        private BlendedNode _currentNode;
        private SyntaxToken _currentToken;
        private ArrayElement<SyntaxToken>[] _lexedTokens;
        private GreenNode _prevTokenTrailingTrivia;
        private int _firstToken; // The position of _lexedTokens[0] (or _blendedTokens[0]).
        private int _tokenOffset; // The index of the current token within _lexedTokens or _blendedTokens.
        private int _tokenCount;
        private int _resetCount;
        private int _resetStart;

        private static readonly ObjectPool<BlendedNode[]> s_blendedNodesPool = new ObjectPool<BlendedNode[]>(() => new BlendedNode[32], 2);

        private BlendedNode[] _blendedTokens;

        protected SyntaxParser(
            Lexer lexer,
            Lua.LuaSyntaxNode oldTree,
            IEnumerable<TextChangeRange> changes,
            bool preLexIfNotIncremental = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            _lexer = lexer;
            _cancellationToken = cancellationToken;
            _currentNode = default(BlendedNode);
            _isIncremental = oldTree != null;

            if (IsIncremental)
            {
                _firstBlender = new Blender(lexer, oldTree, changes);
                _blendedTokens = s_blendedNodesPool.Allocate();
            }
            else
            {
                _firstBlender = default(Blender);
                _lexedTokens = new ArrayElement<SyntaxToken>[32];
            }

            // PreLex is not cancellable.
            //      If we may cancel why would we aggressively lex ahead?
            //      Cancellations in a constructor make disposing complicated
            //
            // So, if we have a real cancellation token, do not do prelexing.
            if (preLexIfNotIncremental && !IsIncremental && !cancellationToken.CanBeCanceled)
            {
                PreLex();
            }
        }

        public void Dispose()
        {
            var blendedTokens = _blendedTokens;
            if (blendedTokens != null)
            {
                _blendedTokens = null;
                if (blendedTokens.Length < 4096)
                {
                    Array.Clear(blendedTokens, 0, blendedTokens.Length);
                    s_blendedNodesPool.Free(blendedTokens);
                }
                else
                {
                    s_blendedNodesPool.ForgetTrackedObject(blendedTokens);
                }
            }
        }

        protected void ReInitialize()
        {
            _firstToken = 0;
            _tokenOffset = 0;
            _tokenCount = 0;
            _resetCount = 0;
            _resetStart = 0;
            _currentToken = null;
            _prevTokenTrailingTrivia = null;
            if (IsIncremental)
            {
                _firstBlender = new Blender(_lexer, null, null);
            }
        }

        protected bool IsIncremental => _isIncremental;

        private void PreLex()
        {
            // NOTE: Do not cancel in this method. It is called from the constructor.
            var size = Math.Min(4096, Math.Max(32, _lexer.Length / 2));
            _lexedTokens = new ArrayElement<SyntaxToken>[size];
            var lexer = _lexer;

            for (var i = 0; i < size; i++)
            {
                var token = lexer.Lex();
                AddLexedToken(token);
                if (token.Kind == SyntaxKind.EndOfFileToken)
                {
                    break;
                }
            }
        }

        protected ResetPoint GetResetPoint()
        {
            var pos = CurrentTokenPosition;
            if (_resetCount == 0)
            {
                _resetStart = pos; // low water mark
            }

            _resetCount++;
            return new ResetPoint(_resetCount, pos, _prevTokenTrailingTrivia);
        }

        protected void Reset(ref ResetPoint point)
        {
            var offset = point.Position - _firstToken;
            RoslynDebug.Assert(offset >= 0);

            if (offset >= _tokenCount)
            {
                // Re-fetch tokens to the position in the reset point
                PeekToken(offset - _tokenOffset);

                // Re-calculate new offset in case tokens got shifted to the left while we were peeking.
                offset = point.Position - _firstToken;
            }

            RoslynDebug.Assert(offset >= 0 && offset < _tokenCount);
            _tokenOffset = offset;
            _currentToken = null;
            _currentNode = default(BlendedNode);
            _prevTokenTrailingTrivia = point.PrevTokenTrailingTrivia;
            if (_blendedTokens != null)
            {
                // look forward for slots not holding a token
                for (var i = _tokenOffset; i < _tokenCount; i++)
                {
                    if (_blendedTokens[i].Token == null)
                    {
                        // forget anything after and including any slot not holding a token
                        _tokenCount = i;
                        if (_tokenCount == _tokenOffset)
                        {
                            FetchCurrentToken();
                        }
                        break;
                    }
                }
            }
        }

        protected void Release(ref ResetPoint point)
        {
            RoslynDebug.Assert(_resetCount == point.ResetCount);
            _resetCount--;
            if (_resetCount == 0)
            {
                _resetStart = -1;
            }
        }

        public LuaParseOptions Options => _lexer.Options;

        protected Lua.LuaSyntaxNode CurrentNode
        {
            get
            {
                // we will fail anyways. Assert is just to catch that earlier.
                RoslynDebug.Assert(_blendedTokens != null);

                //PERF: currentNode is a BlendedNode, which is a fairly large struct.
                // the following code tries not to pull the whole struct into a local
                // we only need .Node
                var node = _currentNode.Node;
                if (node != null)
                {
                    return node;
                }

                ReadCurrentNode();
                return _currentNode.Node;
            }
        }

        protected SyntaxKind CurrentNodeKind
        {
            get
            {
                var cn = CurrentNode;
                return cn != null ? cn.Kind() : SyntaxKind.None;
            }
        }

        private void ReadCurrentNode()
        {
            if (_tokenOffset == 0)
            {
                _currentNode = _firstBlender.ReadNode();
            }
            else
            {
                _currentNode = _blendedTokens[_tokenOffset - 1].Blender.ReadNode();
            }
        }

        protected GreenNode EatNode()
        {
            // we will fail anyways. Assert is just to catch that earlier.
            RoslynDebug.Assert(_blendedTokens != null);

            // remember result
            var result = CurrentNode.Green;

            // store possible non-token in token sequence
            if (_tokenOffset >= _blendedTokens.Length)
            {
                AddTokenSlot();
            }

            _blendedTokens[_tokenOffset++] = _currentNode;
            _tokenCount = _tokenOffset; // forget anything after this slot

            // erase current state
            _currentNode = default(BlendedNode);
            _currentToken = null;

            return result;
        }

        protected SyntaxToken CurrentToken => _currentToken ?? (_currentToken = FetchCurrentToken());

        private SyntaxToken FetchCurrentToken()
        {
            if (_tokenOffset >= _tokenCount)
            {
                AddNewToken();
            }

            if (_blendedTokens != null)
            {
                return _blendedTokens[_tokenOffset].Token;
            }
            else
            {
                return _lexedTokens[_tokenOffset];
            }
        }

        private void AddNewToken()
        {
            if (_blendedTokens != null)
            {
                if (_tokenCount > 0)
                {
                    AddToken(_blendedTokens[_tokenCount - 1].Blender.ReadToken());
                }
                else
                {
                    if (_currentNode.Token != null)
                    {
                        AddToken(_currentNode);
                    }
                    else
                    {
                        AddToken(_firstBlender.ReadToken());
                    }
                }
            }
            else
            {
                AddLexedToken(_lexer.Lex());
            }
        }

        // adds token to end of current token array
        private void AddToken(in BlendedNode tokenResult)
        {
            RoslynDebug.Assert(tokenResult.Token != null);
            if (_tokenCount >= _blendedTokens.Length)
            {
                AddTokenSlot();
            }

            _blendedTokens[_tokenCount] = tokenResult;
            _tokenCount++;
        }

        private void AddLexedToken(SyntaxToken token)
        {
            RoslynDebug.Assert(token != null);
            if (_tokenCount >= _lexedTokens.Length)
            {
                AddLexedTokenSlot();
            }

            _lexedTokens[_tokenCount].Value = token;
            _tokenCount++;
        }

        private void AddTokenSlot()
        {
            // shift tokens to left if we are far to the right
            // don't shift if reset points have fixed locked the starting point at the token in the window
            if (_tokenOffset > (_blendedTokens.Length >> 1)
                && (_resetStart == -1 || _resetStart > _firstToken))
            {
                var shiftOffset = (_resetStart == -1) ? _tokenOffset : _resetStart - _firstToken;
                var shiftCount = _tokenCount - shiftOffset;
                RoslynDebug.Assert(shiftOffset > 0);
                _firstBlender = _blendedTokens[shiftOffset - 1].Blender;
                if (shiftCount > 0)
                {
                    Array.Copy(_blendedTokens, shiftOffset, _blendedTokens, 0, shiftCount);
                }

                _firstToken += shiftOffset;
                _tokenCount -= shiftOffset;
                _tokenOffset -= shiftOffset;
            }
            else
            {
                var old = _blendedTokens;
                Array.Resize(ref _blendedTokens, _blendedTokens.Length * 2);
                s_blendedNodesPool.ForgetTrackedObject(old, replacement: _blendedTokens);
            }
        }

        private void AddLexedTokenSlot()
        {
            // shift tokens to left if we are far to the right
            // don't shift if reset points have fixed locked the starting point at the token in the window
            if (_tokenOffset > (_lexedTokens.Length >> 1)
                && (_resetStart == -1 || _resetStart > _firstToken))
            {
                var shiftOffset = (_resetStart == -1) ? _tokenOffset : _resetStart - _firstToken;
                var shiftCount = _tokenCount - shiftOffset;
                RoslynDebug.Assert(shiftOffset > 0);
                if (shiftCount > 0)
                {
                    Array.Copy(_lexedTokens, shiftOffset, _lexedTokens, 0, shiftCount);
                }

                _firstToken += shiftOffset;
                _tokenCount -= shiftOffset;
                _tokenOffset -= shiftOffset;
            }
            else
            {
                var tmp = new ArrayElement<SyntaxToken>[_lexedTokens.Length * 2];
                Array.Copy(_lexedTokens, tmp, _lexedTokens.Length);
                _lexedTokens = tmp;
            }
        }

        protected SyntaxToken PeekToken(int n)
        {
            RoslynDebug.Assert(n >= 0);
            while (_tokenOffset + n >= _tokenCount)
            {
                AddNewToken();
            }

            if (_blendedTokens != null)
            {
                return _blendedTokens[_tokenOffset + n].Token;
            }
            else
            {
                return _lexedTokens[_tokenOffset + n];
            }
        }

        //this method is called very frequently
        //we should keep it simple so that it can be inlined.
        protected SyntaxToken EatToken()
        {
            var ct = CurrentToken;
            MoveToNextToken();
            return ct;
        }

        /// <summary>
        /// Returns and consumes the current token if it has the requested <paramref name="kind"/>.
        /// Otherwise, returns <see langword="null"/>.
        /// </summary>
        protected SyntaxToken TryEatToken(SyntaxKind kind)
            => CurrentToken.Kind == kind ? EatToken() : null;

        private void MoveToNextToken()
        {
            _prevTokenTrailingTrivia = _currentToken.GetTrailingTrivia();

            _currentToken = null;

            if (_blendedTokens != null)
            {
                _currentNode = default(BlendedNode);
            }

            _tokenOffset++;
        }

        protected void ForceEndOfFile() => _currentToken = SyntaxFactory.Token(SyntaxKind.EndOfFileToken);

        //this method is called very frequently
        //we should keep it simple so that it can be inlined.
        protected SyntaxToken EatToken(SyntaxKind kind)
        {
            RoslynDebug.Assert(SyntaxFacts.IsToken(kind));

            var ct = CurrentToken;
            if (ct.Kind == kind)
            {
                MoveToNextToken();
                return ct;
            }

            //slow part of EatToken(SyntaxKind kind)
            return CreateMissingToken(kind, CurrentToken.Kind, reportError: true);
        }

        // Consume a token if it is the right kind. Otherwise skip a token and replace it with one of the correct kind.
        protected SyntaxToken EatTokenAsKind(SyntaxKind expected)
        {
            RoslynDebug.Assert(SyntaxFacts.IsToken(expected));

            var ct = CurrentToken;
            if (ct.Kind == expected)
            {
                MoveToNextToken();
                return ct;
            }

            var replacement = CreateMissingToken(expected, CurrentToken.Kind, reportError: true);
            return AddTrailingSkippedSyntax(replacement, EatToken());
        }

        private SyntaxToken CreateMissingToken(SyntaxKind expected, SyntaxKind actual, bool reportError)
        {
            // should we eat the current ParseToken's leading trivia?
            var token = SyntaxFactory.MissingToken(expected);
            if (reportError)
            {
                token = WithAdditionalDiagnostics(token, GetExpectedTokenError(expected, actual));
            }

            return token;
        }

        private SyntaxToken CreateMissingToken(SyntaxKind expected, ErrorCode code, bool reportError)
        {
            // should we eat the current ParseToken's leading trivia?
            var token = SyntaxFactory.MissingToken(expected);
            if (reportError)
            {
                token = AddError(token, code);
            }

            return token;
        }

        protected SyntaxToken EatToken(SyntaxKind kind, bool reportError)
        {
            if (reportError)
            {
                return EatToken(kind);
            }

            RoslynDebug.Assert(SyntaxFacts.IsToken(kind));
            if (CurrentToken.Kind != kind)
            {
                // should we eat the current ParseToken's leading trivia?
                return SyntaxFactory.MissingToken(kind);
            }
            else
            {
                return EatToken();
            }
        }

        protected SyntaxToken EatToken(SyntaxKind kind, ErrorCode code, bool reportError = true)
        {
            RoslynDebug.Assert(SyntaxFacts.IsToken(kind));
            if (CurrentToken.Kind != kind)
            {
                return CreateMissingToken(kind, code, reportError);
            }
            else
            {
                return EatToken();
            }
        }

        protected SyntaxToken EatTokenWithPrejudice(SyntaxKind kind)
        {
            var token = CurrentToken;
            RoslynDebug.Assert(SyntaxFacts.IsToken(kind));
            if (token.Kind != kind)
            {
                token = WithAdditionalDiagnostics(token, GetExpectedTokenError(kind, token.Kind));
            }

            MoveToNextToken();
            return token;
        }

        protected SyntaxToken EatTokenWithPrejudice(ErrorCode errorCode, params object[] args)
        {
            var token = EatToken();
            token = WithAdditionalDiagnostics(token, MakeError(token.GetLeadingTriviaWidth(), token.Width, errorCode, args));
            return token;
        }

        protected SyntaxToken EatContextualToken(SyntaxKind kind, ErrorCode code, bool reportError = true)
        {
            RoslynDebug.Assert(SyntaxFacts.IsToken(kind));

            if (CurrentToken.ContextualKind != kind)
            {
                return CreateMissingToken(kind, code, reportError);
            }
            else
            {
                return ConvertToKeyword(EatToken());
            }
        }

        protected SyntaxToken EatContextualToken(SyntaxKind kind, bool reportError = true)
        {
            RoslynDebug.Assert(SyntaxFacts.IsToken(kind));

            var contextualKind = CurrentToken.ContextualKind;
            if (contextualKind != kind)
            {
                return CreateMissingToken(kind, contextualKind, reportError);
            }
            else
            {
                return ConvertToKeyword(EatToken());
            }
        }

        protected virtual SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual, int offset, int width)
        {
            var code = GetExpectedTokenErrorCode(expected, actual);
            if (code is ErrorCode.ERR_SyntaxError or ErrorCode.ERR_IdentifierExpectedKW)
            {
                return new SyntaxDiagnosticInfo(offset, width, code, SyntaxFacts.GetText(expected), SyntaxFacts.GetText(actual));
            }
            else
            {
                return new SyntaxDiagnosticInfo(offset, width, code);
            }
        }

        protected virtual SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual)
        {
            GetDiagnosticSpanForMissingToken(out var offset, out var width);

            return GetExpectedTokenError(expected, actual, offset, width);
        }

        private static ErrorCode GetExpectedTokenErrorCode(SyntaxKind expected, SyntaxKind actual)
        {
            switch (expected)
            {
                case SyntaxKind.IdentifierToken:
                    if (SyntaxFacts.IsKeyword(actual)) // If we actually get to this, then continue is a reserved keyword
                    {
                        return ErrorCode.ERR_IdentifierExpectedKW;   // A keyword -- use special message.
                    }
                    else
                    {
                        return ErrorCode.ERR_IdentifierExpected;
                    }

                case SyntaxKind.SemicolonToken:
                    return ErrorCode.ERR_SemicolonExpected;

                case SyntaxKind.CloseParenthesisToken:
                    return ErrorCode.ERR_CloseParenExpected;
                case SyntaxKind.OpenBraceToken:
                    return ErrorCode.ERR_LbraceExpected;
                case SyntaxKind.CloseBraceToken:
                    return ErrorCode.ERR_RbraceExpected;

                default:
                    return ErrorCode.ERR_SyntaxError;
            }
        }

        protected void GetDiagnosticSpanForMissingToken(out int offset, out int width)
        {
            // If the previous token has a trailing EndOfLineTrivia,
            // the missing token diagnostic position is moved to the
            // end of line containing the previous token and
            // its width is set to zero.
            // Otherwise the diagnostic offset and width is set
            // to the corresponding values of the current token

            var trivia = _prevTokenTrailingTrivia;
            if (trivia != null)
            {
                var triviaList = new SyntaxList<LuaSyntaxNode>(trivia);
                var prevTokenHasEndOfLineTrivia = triviaList.Any((int) SyntaxKind.EndOfLineTrivia);
                if (prevTokenHasEndOfLineTrivia)
                {
                    offset = -trivia.FullWidth;
                    width = 0;
                    return;
                }
            }

            var ct = CurrentToken;
            offset = ct.GetLeadingTriviaWidth();
            width = ct.Width;
        }

        protected virtual TNode WithAdditionalDiagnostics<TNode>(TNode node, params DiagnosticInfo[] diagnostics) where TNode : GreenNode
        {
            var existingDiags = node.GetDiagnostics();
            var existingLength = existingDiags.Length;
            if (existingLength == 0)
            {
                return node.WithDiagnosticsGreen(diagnostics);
            }
            else
            {
                var result = new DiagnosticInfo[existingDiags.Length + diagnostics.Length];
                existingDiags.CopyTo(result, 0);
                diagnostics.CopyTo(result, existingLength);
                return node.WithDiagnosticsGreen(result);
            }
        }

        protected TNode AddError<TNode>(TNode node, ErrorCode code) where TNode : GreenNode => AddError(node, code, Array.Empty<object>());

        protected TNode AddError<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : GreenNode
        {
            if (!node.IsMissing)
            {
                return WithAdditionalDiagnostics(node, MakeError(node, code, args));
            }

            int offset, width;

            if (node is SyntaxToken token && token.ContainsSkippedText)
            {
                // This code exists to clean up an anti-pattern:
                //   1) an undesirable token is parsed,
                //   2) a desirable missing token is created and the parsed token is appended as skipped text,
                //   3) an error is attached to the missing token describing the problem.
                // If this occurs, then this.previousTokenTrailingTrivia is still populated with the trivia
                // of the undesirable token (now skipped text).  Since the trivia no longer precedes the
                // node to which the error is to be attached, the computed offset will be incorrect.

                offset = token.GetLeadingTriviaWidth(); // Should always be zero, but at least we'll do something sensible if it's not.
                RoslynDebug.Assert(offset == 0, "Why are we producing a missing token that has both skipped text and leading trivia?");

                width = 0;
                var seenSkipped = false;
                foreach (var trivia in token.TrailingTrivia)
                {
                    if (trivia.Kind == SyntaxKind.SkippedTokensTrivia)
                    {
                        seenSkipped = true;
                        width += trivia.Width;
                    }
                    else if (seenSkipped)
                    {
                        break;
                    }
                    else
                    {
                        offset += trivia.Width;
                    }
                }
            }
            else
            {
                GetDiagnosticSpanForMissingToken(out offset, out width);
            }

            return WithAdditionalDiagnostics(node, MakeError(offset, width, code, args));
        }

        protected TNode AddError<TNode>(TNode node, int offset, int length, ErrorCode code, params object[] args) where TNode : LuaSyntaxNode => WithAdditionalDiagnostics(node, MakeError(offset, length, code, args));

        protected TNode AddError<TNode>(TNode node, LuaSyntaxNode location, ErrorCode code, params object[] args) where TNode : LuaSyntaxNode
        {
            // assumes non-terminals will at most appear once in sub-tree
            FindOffset(node, location, out var offset);
            return WithAdditionalDiagnostics(node, MakeError(offset, location.Width, code, args));
        }

        protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code) where TNode : LuaSyntaxNode
        {
            var firstToken = node.GetFirstToken();
            return WithAdditionalDiagnostics(node, MakeError(firstToken.GetLeadingTriviaWidth(), firstToken.Width, code));
        }

        protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : LuaSyntaxNode
        {
            var firstToken = node.GetFirstToken();
            return WithAdditionalDiagnostics(node, MakeError(firstToken.GetLeadingTriviaWidth(), firstToken.Width, code, args));
        }

        protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code) where TNode : LuaSyntaxNode
        {
            GetOffsetAndWidthForLastToken(node, out var offset, out var width);
            return WithAdditionalDiagnostics(node, MakeError(offset, width, code));
        }

        protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : LuaSyntaxNode
        {
            GetOffsetAndWidthForLastToken(node, out var offset, out var width);
            return WithAdditionalDiagnostics(node, MakeError(offset, width, code, args));
        }

        private static void GetOffsetAndWidthForLastToken<TNode>(TNode node, out int offset, out int width) where TNode : LuaSyntaxNode
        {
            var lastToken = node.GetLastNonmissingToken();
            offset = node.FullWidth; //advance to end of entire node
            width = 0;
            if (lastToken != null) //will be null if all tokens are missing
            {
                offset -= lastToken.FullWidth; //rewind past last token
                offset += lastToken.GetLeadingTriviaWidth(); //advance past last token leading trivia - now at start of last token
                width += lastToken.Width;
            }
        }

        protected static SyntaxDiagnosticInfo MakeError(int offset, int width, ErrorCode code) => new SyntaxDiagnosticInfo(offset, width, code);

        protected static SyntaxDiagnosticInfo MakeError(int offset, int width, ErrorCode code, params object[] args) => new SyntaxDiagnosticInfo(offset, width, code, args);

        protected static SyntaxDiagnosticInfo MakeError(GreenNode node, ErrorCode code, params object[] args) => new SyntaxDiagnosticInfo(node.GetLeadingTriviaWidth(), node.Width, code, args);

        protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args) => new SyntaxDiagnosticInfo(code, args);

        protected TNode AddLeadingSkippedSyntax<TNode>(TNode node, GreenNode skippedSyntax) where TNode : LuaSyntaxNode
        {
            var oldToken = node as SyntaxToken ?? node.GetFirstToken();
            var newToken = AddSkippedSyntax(oldToken, skippedSyntax, trailing: false);
            return SyntaxFirstTokenReplacer.Replace(node, oldToken, newToken, skippedSyntax.FullWidth);
        }

        protected void AddTrailingSkippedSyntax(SyntaxListBuilder list, GreenNode skippedSyntax) => list[list.Count - 1] = AddTrailingSkippedSyntax((LuaSyntaxNode) list[list.Count - 1], skippedSyntax);

        protected void AddTrailingSkippedSyntax<TNode>(SyntaxListBuilder<TNode> list, GreenNode skippedSyntax) where TNode : LuaSyntaxNode => list[list.Count - 1] = AddTrailingSkippedSyntax(list[list.Count - 1], skippedSyntax);

        protected TNode AddTrailingSkippedSyntax<TNode>(TNode node, GreenNode skippedSyntax) where TNode : LuaSyntaxNode
        {
            if (node is SyntaxToken token)
            {
                return (TNode) (object) AddSkippedSyntax(token, skippedSyntax, trailing: true);
            }
            else
            {
                var lastToken = node.GetLastToken();
                var newToken = AddSkippedSyntax(lastToken, skippedSyntax, trailing: true);
                return SyntaxLastTokenReplacer.Replace(node, newToken);
            }
        }

        /// <summary>
        /// Converts skippedSyntax node into tokens and adds these as trivia on the target token.
        /// Also adds the first error (in depth-first preorder) found in the skipped syntax tree to the target token.
        /// </summary>
        internal SyntaxToken AddSkippedSyntax(SyntaxToken target, GreenNode skippedSyntax, bool trailing)
        {
            var builder = new SyntaxListBuilder(4);

            // the error in we'll attach to the node
            SyntaxDiagnosticInfo diagnostic = null;

            // the position of the error within the skippedSyntax node full tree
            var diagnosticOffset = 0;

            var currentOffset = 0;
            foreach (var node in skippedSyntax.EnumerateNodes())
            {
                if (node is SyntaxToken token)
                {
                    builder.Add(token.GetLeadingTrivia());

                    if (token.Width > 0)
                    {
                        // separate trivia from the tokens
                        var tk = token.TokenWithLeadingTrivia(null).TokenWithTrailingTrivia(null);

                        // adjust relative offsets of diagnostics attached to the token:
                        var leadingWidth = token.GetLeadingTriviaWidth();
                        if (leadingWidth > 0)
                        {
                            var tokenDiagnostics = tk.GetDiagnostics();
                            for (var i = 0; i < tokenDiagnostics.Length; i++)
                            {
                                var d = (SyntaxDiagnosticInfo) tokenDiagnostics[i];
                                tokenDiagnostics[i] = new SyntaxDiagnosticInfo(d.Offset - leadingWidth, d.Width, (ErrorCode) d.Code, d.Arguments);
                            }
                        }

                        builder.Add(SyntaxFactory.SkippedTokensTrivia(tk));
                    }
                    else
                    {
                        // do not create zero-width structured trivia, GetStructure doesn't work well for them
                        var existing = (SyntaxDiagnosticInfo) token.GetDiagnostics().FirstOrDefault();
                        if (existing != null)
                        {
                            diagnostic = existing;
                            diagnosticOffset = currentOffset;
                        }
                    }
                    builder.Add(token.GetTrailingTrivia());

                    currentOffset += token.FullWidth;
                }
                else if (node.ContainsDiagnostics && diagnostic == null)
                {
                    // only propagate the first error to reduce noise:
                    var existing = (SyntaxDiagnosticInfo) node.GetDiagnostics().FirstOrDefault();
                    if (existing != null)
                    {
                        diagnostic = existing;
                        diagnosticOffset = currentOffset;
                    }
                }
            }

            var triviaWidth = currentOffset;
            var trivia = builder.ToListNode();

            // total width of everything preceding the added trivia
            int triviaOffset;
            if (trailing)
            {
                var trailingTrivia = target.GetTrailingTrivia();
                triviaOffset = target.FullWidth; //added trivia is full width (before addition)
                target = target.TokenWithTrailingTrivia(SyntaxList.Concat(trailingTrivia, trivia));
            }
            else
            {
                // Since we're adding triviaWidth before the token, we have to add that much to
                // the offset of each of its diagnostics.
                if (triviaWidth > 0)
                {
                    var targetDiagnostics = target.GetDiagnostics();
                    for (var i = 0; i < targetDiagnostics.Length; i++)
                    {
                        var d = (SyntaxDiagnosticInfo) targetDiagnostics[i];
                        targetDiagnostics[i] = new SyntaxDiagnosticInfo(d.Offset + triviaWidth, d.Width, (ErrorCode) d.Code, d.Arguments);
                    }
                }

                var leadingTrivia = target.GetLeadingTrivia();
                target = target.TokenWithLeadingTrivia(SyntaxList.Concat(trivia, leadingTrivia));
                triviaOffset = 0; //added trivia is first, so offset is zero
            }

            if (diagnostic != null)
            {
                var newOffset = triviaOffset + diagnosticOffset + diagnostic.Offset;

                target = WithAdditionalDiagnostics(target,
                    new SyntaxDiagnosticInfo(newOffset, diagnostic.Width, (ErrorCode) diagnostic.Code, diagnostic.Arguments)
                );
            }

            return target;
        }

        /// <summary>
        /// This function searches for the given location node within the subtree rooted at root node.
        /// If it finds it, the function computes the offset span of that child node within the root and returns true,
        /// otherwise it returns false.
        /// </summary>
        /// <param name="root">Root node</param>
        /// <param name="location">Node to search in the subtree rooted at root node</param>
        /// <param name="offset">Offset of the location node within the subtree rooted at child</param>
        /// <returns></returns>
        private bool FindOffset(GreenNode root, LuaSyntaxNode location, out int offset)
        {
            var currentOffset = 0;
            offset = 0;
            if (root != null)
            {
                for (int i = 0, n = root.SlotCount; i < n; i++)
                {
                    var child = root.GetSlot(i);
                    if (child == null)
                    {
                        // ignore null slots
                        continue;
                    }

                    // check if the child node is the location node
                    if (child == location)
                    {
                        // Found the location node in the subtree
                        // Initialize offset with the offset of the location node within its parent
                        // and walk up the stack of recursive calls adding the offset of each node
                        // within its parent
                        offset = currentOffset;
                        return true;
                    }

                    // search for the location node in the subtree rooted at child node
                    if (FindOffset(child, location, out offset))
                    {
                        // Found the location node in child's subtree
                        // Add the offset of child node within its parent to offset
                        // and continue walking up the stack
                        offset += child.GetLeadingTriviaWidth() + currentOffset;
                        return true;
                    }

                    // We didn't find the location node in the subtree rooted at child
                    // Move on to the next child
                    currentOffset += child.FullWidth;
                }
            }

            // We didn't find the location node within the subtree rooted at root node
            return false;
        }

        protected static SyntaxToken ConvertToKeyword(SyntaxToken token)
        {
            if (token.Kind != token.ContextualKind)
            {
                var kw = token.IsMissing
                        ? SyntaxFactory.MissingToken(token.LeadingTrivia.Node, token.ContextualKind, token.TrailingTrivia.Node)
                        : SyntaxFactory.Token(token.LeadingTrivia.Node, token.ContextualKind, token.TrailingTrivia.Node);
                var d = token.GetDiagnostics();
                if (d != null && d.Length > 0)
                {
                    kw = kw.WithDiagnosticsGreen(d);
                }

                return kw;
            }

            return token;
        }

        protected static SyntaxToken ConvertToIdentifier(SyntaxToken token)
        {
            RoslynDebug.Assert(!token.IsMissing);
            return SyntaxToken.Identifier(token.Kind, token.LeadingTrivia.Node, token.Text, token.TrailingTrivia.Node);
        }

        /// <summary>
        /// Whenever parsing in a <c>while (true)</c> loop and a bug could prevent the loop from making progress,
        /// this method can prevent the parsing from hanging.
        /// Use as:
        ///     int tokenProgress = -1;
        ///     while (IsMakingProgress(ref tokenProgress))
        /// It should be used as a guardrail, not as a crutch, so it asserts if no progress was made.
        /// </summary>
        protected bool IsMakingProgress(ref int lastTokenPosition, bool assertIfFalse = true)
        {
            var pos = CurrentTokenPosition;
            if (pos > lastTokenPosition)
            {
                lastTokenPosition = pos;
                return true;
            }

            RoslynDebug.Assert(!assertIfFalse);
            return false;
        }

        private int CurrentTokenPosition => _firstToken + _tokenOffset;
    }
}
