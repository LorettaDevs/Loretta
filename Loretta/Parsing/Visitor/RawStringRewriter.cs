using System;
using System.Linq;
using System.Text;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Visitor
{
    public class RawStringRewriter : TreeFolderBase
    {
        public override LuaASTNode VisitString ( StringExpression node )
        {
            // Nothing to do about long strings
            if ( node.IsLong )
                return node;

            Token<LuaTokenType> token = node.Tokens.Single ( );
            var delim = token.Raw[0];
            // The most common case will (hopefully) be one where the string is composed of
            var rawBuilder = new StringBuilder ( node.Value.Length );
            rawBuilder.Append ( delim );
            foreach ( var ch in node.Value )
            {
                if ( Char.IsLetterOrDigit ( ch ) || Char.IsPunctuation ( ch ) /*|| Char.IsSymbol ( ch )*/ || ch == ' ' )
                    rawBuilder.Append ( ch );
                else
                    rawBuilder.Append ( EncodeCharToUtf8 ( ch ) );
            }
            rawBuilder.Append ( delim );

            return ASTNodeFactory.ShortString ( node.Value, rawBuilder.ToString ( ) );
        }
    }
}