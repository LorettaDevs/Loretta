using System;
using System.Collections.Generic;
using System.Text;
using GParse.Lexing;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public class TableConstructorExpression : ASTNode
    {
        public IList<TableKeyValue> Fields { get; }

        public Int32 SequentialIndex { get; private set; }

        public Boolean HasTrailingComma { get; set; }

        public TableConstructorExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
            this.Fields = new List<TableKeyValue> ( );
            this.SequentialIndex = 0;
            this.HasTrailingComma = false;
        }

        public TableKeyValue AddExplicitKeyField ( ASTNode key, ASTNode value, String separator, IList<LToken> tokenList )
        {
            TableKeyValue keyValue;
            this.Fields.Add ( keyValue = new TableKeyValue ( this, this.Scope, tokenList )
            {
                Key = key,
                Value = value,
                Separator = separator
            } );
            this.AddChild ( keyValue );

            return keyValue;
        }

        public TableKeyValue AddSequentialField ( ASTNode value, String separator, IList<LToken> tokenList )
        {
            TableKeyValue keyValue;
            this.Fields.Add ( keyValue = new TableKeyValue ( this, this.Scope, tokenList )
            {
                Value = value,
                Separator = separator,
                Sequential = true
            } );
            this.AddChild ( keyValue );

            this.SequentialIndex++;
            keyValue.Key = new NumberExpression ( keyValue, keyValue.Scope, new[] {
                new LToken ( "number", this.SequentialIndex.ToString ( ), this.SequentialIndex, TokenType.Number, SourceRange.Zero )
            } );

            return keyValue;
        }
    }

    public class TableKeyValue : ASTNode
    {
        public ASTNode Key { get; set; }

        public ASTNode Value { get; set; }

        public String Separator { get; set; }

        public Boolean Sequential { get; set; }

        public TableKeyValue ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }
    }
}
