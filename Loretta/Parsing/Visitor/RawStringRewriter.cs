using System;
using System.Linq;
using System.Text;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Utilities;

namespace Loretta.Parsing.Visitor
{
    /// <summary>
    /// A folder that rewrites the raw form of strings to only escape required (non-alphanumeric,
    /// non-punctuation and non-space) characters.
    /// </summary>
    public class RawStringRewriter : TreeFolderBase
    {
        /// <summary>
        /// Rewrites the raw form of the provided string.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
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
                    rawBuilder.Append ( CharUtils.EncodeCharToUtf8 ( ch ) );
            }
            rawBuilder.Append ( delim );

            return ASTNodeFactory.ShortString ( node.Value, rawBuilder.ToString ( ) );
        }
    }
}