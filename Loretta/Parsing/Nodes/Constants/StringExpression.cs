using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Lexing;
using Loretta.Utils;

namespace Loretta.Parsing.Nodes.Constants
{
    public class StringExpression : ConstantExpression<String>
    {
        public String UnescapedValue { get; }

        public String EscapedValue { get; }

        public String StartDelimiter { get; }

        public String EndDelimiter { get; }

        public Boolean IsLiteralString { get; }

        public StringExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
            if ( this.Tokens.Count != 1 )
                throw new Exception ( "A StringExpression should only be composed of a single LToken of type String." );
            LToken strtok = this.Tokens[0];
            if ( strtok.ID == "literalstring" )
            {
                this.UnescapedValue = ( String ) strtok.Value;
                this.EscapedValue = this.UnescapedValue;
                this.StartDelimiter = strtok.Raw.Substring ( 0, strtok.Raw.IndexOf ( '[', 1 ) + 1 );
                this.EndDelimiter = this.StartDelimiter.Replace ( '[', ']' );
                this.IsLiteralString = true;
            }
            else
            {
                this.StartDelimiter = strtok.Raw[0].ToString ( );
                this.EndDelimiter = this.StartDelimiter;
                this.UnescapedValue = ( String ) strtok.Value;

                var unescaped = new StringBuilder ( );
                foreach ( var c in ( String ) strtok.Value )
                {
                    if ( c == this.StartDelimiter[0] || c == '\n' )
                        unescaped
                              .Append ( '\\' )
                              .Append ( c );
                    else if ( !LJUtils.IsGraph ( c ) && !LJUtils.IsSpace ( c ) )
                        unescaped.AppendFormat ( "\\x{0:X2}", ( Int32 ) c );
                    else
                        unescaped.Append ( c );
                }
                this.EscapedValue = unescaped.ToString ( );
            }
        }

        public override ASTNode Clone ( )
        {
            return new StringExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
        }
    }
}
