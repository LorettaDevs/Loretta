using System;
using System.Collections.Generic;
using System.Text;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed class LexerCache
    {
        private static readonly ObjectPool<CachingIdentityFactory<string, SyntaxKind>> s_keywordKindPool =
            CachingIdentityFactory<string, SyntaxKind>.CreatePool(512, key => SyntaxFacts.GetKeywordKind(key));
        private readonly TextKeyedCache<SyntaxTrivia> _triviaMap;
        private readonly CachingIdentityFactory<string, SyntaxKind> _keywordKindMap;

        internal LexerCache()
        {
            _triviaMap = TextKeyedCache<SyntaxTrivia>.GetInstance();
            _keywordKindMap = s_keywordKindPool.Allocate();
        }

        internal void Free()
        {
            _keywordKindMap.Free();
            _triviaMap.Free();
        }

        internal bool TryGetKeywordKind(string key, out SyntaxKind kind)
        {
            if (key.Length < SyntaxFacts.MinKeywordLength || key.Length > SyntaxFacts.MaxKeywordLength)
            {
                kind = SyntaxKind.IdentifierToken;
                return false;
            }

            kind = _keywordKindMap.GetOrMakeValue(key);
            return kind != SyntaxKind.IdentifierToken;
        }

        internal SyntaxTrivia LookupTrivia(
            string key,
            int hashCode,
            Func<SyntaxTrivia> createTriviaFunction)
        {
            var value = _triviaMap.FindItem(key.AsSpan(), hashCode);

            if (value == null)
            {
                value = createTriviaFunction();
                _triviaMap.AddItem(key.AsSpan(), hashCode, value);
            }

            return value;
        }
    }
}
