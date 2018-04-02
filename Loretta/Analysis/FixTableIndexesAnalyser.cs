using System;
using System.Collections.Generic;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;

namespace Loretta.Analysis
{
    // Use this to fix stuff (recommended)
    public class FixTableIndexesAnalyser : BaseASTAnalyser
    {
        public FixTableIndexesAnalyser ( ) : base ( )
        {
        }

        private static IList<LToken> GetTokenList ( String ID, String Raw, Object Value, TokenType Type, SourceRange Range )
        {
            return new List<LToken> ( new[]
            {
                new LToken ( ID, Raw, Value, Type, Range )
            } );
        }

        private static SourceRange AdjustRange ( SourceRange range, String raw )
        {
            var lines = raw.Split ( '\n' );
            Int32 endLine = range.Start.Line + lines.Length,
                endCol = range.Start.Column + lines[lines.Length - 1].Length,
                endByte = range.Start.Byte + lines[lines.Length - 1].Length;
            return range.Start.To ( new SourceLocation ( endLine, endCol, endByte ) );
        }

        private static NumberExpression GetNumberExpression ( ASTNode prev, Double result )
        {
            return new NumberExpression ( prev.Parent, prev.Scope,
                GetTokenList ( "number", result.ToString ( ), result, TokenType.Number, AdjustRange ( prev.Range, result.ToString ( ) ) ) );
        }

        protected override Object[] AnalyseTableConstructorExpression ( TableConstructorExpression node, params Object[] args )
        {
            var sequentialIndex = 1;
            foreach ( TableKeyValue keyValue in node.Fields )
            {
                if ( keyValue.IsSequential )
                    keyValue.Key = GetNumberExpression ( keyValue.Key, sequentialIndex++ );
            }

            return null;
        }
    }
}
