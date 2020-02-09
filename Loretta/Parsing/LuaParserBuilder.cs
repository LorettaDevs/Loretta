using System;
using GParse;
using GParse.Lexing;
using GParse.Parsing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Modules;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing
{
    public class LuaParserBuilder : PrattParserBuilder<LuaTokenType, Expression>
    {
        private static Boolean StringExpressionFactory ( LuaToken token, out Expression expression )
        {
            expression = new StringExpression ( token );
            return true;
        }

        private static Boolean NumberExpressionFactory ( LuaToken token, out Expression expression )
        {
            expression = new NumberExpression ( token );
            return true;
        }

        private static Boolean NilExpressionFactory ( LuaToken token, out Expression expression )
        {
            expression = new NilExpression ( token );
            return true;
        }

        private static Boolean BooleanExpressionFactory ( LuaToken token, out Expression expression )
        {
            expression = new BooleanExpression ( token, token.Id == "true" );
            return true;
        }

        private static Boolean VarArgExpressionFactory ( LuaToken token, out Expression expression )
        {
            expression = new VarArgExpression ( token );
            return true;
        }

        public LuaParserBuilder ( )
        {
            #region Value Expressions

            this.RegisterLiteral ( LuaTokenType.String, StringExpressionFactory );
            this.RegisterLiteral ( LuaTokenType.LongString, StringExpressionFactory );
            this.RegisterLiteral ( LuaTokenType.Number, NumberExpressionFactory );
            this.RegisterLiteral ( LuaTokenType.Nil, NilExpressionFactory );
            this.RegisterLiteral ( LuaTokenType.Boolean, "true", BooleanExpressionFactory );
            this.RegisterLiteral ( LuaTokenType.Boolean, "false", BooleanExpressionFactory );
            this.RegisterLiteral ( LuaTokenType.VarArg, VarArgExpressionFactory );

            IdentifierParserModule.Register ( this );
            AnonymousFunctionExpressionParserModule.Register ( this );
            GroupedExpressionParserModule.Register ( this );
            TableConstructorExpressionParserModule.Register ( this );

            #endregion Value Expressions

            #region Operators

            this.AddBinaryOperator ( 1, false, "or" );

            this.AddBinaryOperator ( 2, false, "and" );

            this.AddBinaryOperator ( 3, false, "==" );
            this.AddBinaryOperator ( 3, false, "~=" );
            this.AddBinaryOperator ( 3, false, "<" );
            this.AddBinaryOperator ( 3, false, "<=" );
            this.AddBinaryOperator ( 3, false, ">" );
            this.AddBinaryOperator ( 3, false, ">=" );

            this.AddBinaryOperator ( 4, true, ".." );

            this.AddBinaryOperator ( 5, false, "+" );
            this.AddBinaryOperator ( 5, false, "-" );

            this.AddBinaryOperator ( 6, false, "*" );
            this.AddBinaryOperator ( 6, false, "/" );
            this.AddBinaryOperator ( 6, false, "%" );

            this.AddPrefixOperator ( 7, "not" );
            this.AddPrefixOperator ( 7, "#" );
            this.AddPrefixOperator ( 7, "-" );

            this.AddBinaryOperator ( 8, true, "^" );

            FunctionCallExpressionParserModule.Register ( this, 9 );
            IndexExpressionParserModule.Register ( this, 9 );

            #endregion Operators
        }

        #region Operator Management

        protected virtual Boolean BinaryOperatorFactory ( Expression left, LuaToken op, Expression right, out Expression expression )
        {
            expression = new BinaryOperationExpression ( left, op, right );
            return true;
        }

        protected virtual Boolean PrefixOperatorFactory ( LuaToken op, Expression left, out Expression expression )
        {
            expression = new UnaryOperationExpression ( UnaryOperationFix.Prefix, op, left );
            return true;
        }

        protected virtual Boolean PostfixOperatorFactory ( Expression right, LuaToken op, out Expression expression )
        {
            expression = new UnaryOperationExpression ( UnaryOperationFix.Postfix, op, right );
            return true;
        }

        protected virtual void AddBinaryOperator ( Int32 precedence, Boolean isRightAssociative, String op ) =>
            this.RegisterSingleTokenInfixOperator (
                LuaTokenType.Operator,
                op,
                precedence,
                isRightAssociative,
                this.BinaryOperatorFactory );

        protected virtual void AddPrefixOperator ( Int32 precedence, String op ) =>
            this.RegisterSingleTokenPrefixOperator ( LuaTokenType.Operator, op, precedence, this.PrefixOperatorFactory );

        protected virtual void AddPostfixOperator ( Int32 precedence, String op ) =>
            this.RegisterSingleTokenPostfixOperator ( LuaTokenType.Operator, op, precedence, this.PostfixOperatorFactory );

        #endregion Operator Management

        public new LuaParser CreateParser ( ITokenReader<LuaTokenType> reader, IProgress<Diagnostic> diagnosticEmmiter ) =>
            new LuaParser ( reader, this.prefixModuleTree, this.infixModuleTree, diagnosticEmmiter );
    }
}