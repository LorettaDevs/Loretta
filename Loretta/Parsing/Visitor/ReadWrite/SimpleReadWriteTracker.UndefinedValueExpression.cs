using System;
using System.Collections.Generic;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Visitor.ReadWrite
{
    public sealed class UndefinedValueExpression : Expression
    {
        public override Boolean IsConstant => false;
        public override Object? ConstantValue => throw new InvalidOperationException ( );
        public override IEnumerable<Token<LuaTokenType>> Tokens => throw new NotSupportedException ( );
        public override IEnumerable<LuaASTNode> Children => throw new NotSupportedException ( );

        internal override void Accept ( ITreeVisitor visitor ) => throw new NotSupportedException ( );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => throw new NotSupportedException ( );
    }
}