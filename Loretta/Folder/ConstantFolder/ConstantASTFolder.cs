using System;
using System.Collections.Generic;
using GParse.Lexing;
using Loretta.Env;
using Loretta.Lexing;
using Loretta.Parsing;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;
using Loretta.Parsing.Nodes.Functions;
using Loretta.Parsing.Nodes.Indexers;
using Loretta.Parsing.Nodes.Operators;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Folder
{
    public partial class ConstantASTFolder : BaseASTFolder
    {
        private static readonly Random R = new Random ( );

        public ConstantASTFolder ( ) : base ( )
        {
        }

        #region Node Creation Utilities

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

        private static StringExpression GetStringExpression ( ASTNode prev, String result )
        {
            return new StringExpression ( prev.Parent, prev.Scope,
                GetTokenList ( "string", $"'{result.Replace ( "'", "\\'" )}'", result, TokenType.String, AdjustRange ( prev.Range, result + ".." ) )
            );
        }

        private static BooleanExpression GetBooleanExpression ( ASTNode prev, Boolean result )
        {
            var val = result ? "true" : "false";
            return new BooleanExpression ( prev.Parent, prev.Scope,
                GetTokenList ( val, val, val, TokenType.Keyword, AdjustRange ( prev.Range, val ) ) );
        }

        private static DoStatement GetDoStatement ( ASTNode prev, StatementList body )
        {
            var stat = new DoStatement ( prev.Parent, prev.Scope, new List<LToken> ( new[]
            {
                new LToken ( "do", "do", "do", TokenType.Keyword, AdjustRange ( prev.Range, "do" ) ),
                new LToken ( "end", "end", "end", TokenType.Keyword, AdjustRange ( prev.Range, "end" ) )
            } ) );
            stat.SetBody ( body );
            return stat;
        }

        private static AnonymousFunctionExpression GetAnonymousFunctionExpression ( ASTNode prev, Scope scope, StatementList body )
        {
            var expr = new AnonymousFunctionExpression ( prev.Parent, scope, new List<LToken> ( new[]
            {
                new LToken ( "function", "function", "function", TokenType.Keyword, AdjustRange ( prev.Range, "function" ) ),
                new LToken ( "end", "end", "end", TokenType.Keyword, AdjustRange ( prev.Range, "end" ) )
            } ) );
            expr.SetBody ( body );
            return expr;
        }

        #endregion Node Creation Utilities

        private static Double? StringToNumber ( String str )
        {
            try
            {
                var lexer = new GLuaLexer ( str );
                foreach ( LToken token in lexer.Lex ( ) )
                {
                    return ( Double ) token.Value;
                }
                return null;
            }
            catch ( Exception )
            {
                return null;
            }
        }

        protected override ASTNode FoldParenthesisExpression ( ParenthesisExpression node, params Object[] args )
        {
            node = ( ParenthesisExpression ) base.FoldParenthesisExpression ( node, args );

            if ( node.Parent is ParenthesisExpression )
                return node.Expression;
            else if ( node.Expression is ConstantExpression && !( node.Parent is MemberExpression || node.Parent is IndexExpression ) )
                return node.Expression;
            if ( node.Expression is BinaryOperatorExpression childExpr && node.Parent is BinaryOperatorExpression parentExpr )
            {
                return ParserData.OpPriorities[childExpr.Operator][0] < ParserData.OpPriorities[parentExpr.Operator][0]
                    ? node : node.Expression;
            }

            return node;
        }

        protected override ASTNode FoldFunctionCallExpression ( FunctionCallExpression node, params Object[] args )
        {
            // Fold args and base
            node = ( FunctionCallExpression ) base.FoldFunctionCallExpression ( node, args );
            // Fold some functions
            if ( node.Base is VariableExpression varExpr )
            {
                if ( varExpr.Variable.Name == "RunString"
                   // Assert we have enough arguments for RunString
                   && node.Arguments.Count > 0 )
                {
                    if ( !( node.Arguments[0] is StringExpression )
                        || ( node.Arguments.Count > 1 && !( node.Arguments[1] is StringExpression ) )
                        || ( node.Arguments.Count > 2 && !( node.Arguments[2] is BooleanExpression ) ) )
                        return node;

                    var code = ( node.Arguments[0] as StringExpression ).Value;
                    var name = node.Arguments.Count > 1
                    ? ( node.Arguments[1] as StringExpression ).Value
                    : "runstring_" + R.Next ( );
                    // Dispose of useless RunString calls
                    if ( code.Trim ( ) == "" )
                        return null;

                    EnvFile file = this.Environment.ProcessFile ( name, code );

                    if ( file.Successful && file.AST != null )
                    {
                        file.AST.Scope.InternalData.RemoveValue ( "isRoot" );
                        file.AST.Scope.InternalData.RemoveValue ( "isFunction" );
                        // Enclose it in a do...end statement so that it doesn't
                        // breaks other analysers/folders too bad
                        return GetDoStatement ( node, file.AST );
                    }
                }
                // CompileString is almost the same as RunString
                // except it's an anonymous function definition
                else if ( varExpr.Variable.Name == "CompileString" && node.Arguments.Count > 0 )
                {
                    if ( !( node.Arguments[0] is StringExpression )
                        || ( node.Arguments.Count > 1 && !( node.Arguments[1] is StringExpression ) ) )
                        return node;

                    var code = ( node.Arguments[0] as StringExpression ).Value.Trim ( );
                    var id = node.Arguments.Count > 1
                        ? ( node.Arguments[1] as StringExpression ).Value
                        : "compilestring_" + R.Next ( );
                    // It'll be an empty function anyways
                    if ( code == "" )
                    {
                        var scope = new Scope ( node.Scope.Parser, node.Scope );
                        return GetAnonymousFunctionExpression ( node,
                            scope,
                            new StatementList ( null, scope, new List<LToken> ( ) )
                        );
                    }

                    EnvFile file = this.Environment.ProcessFile ( id, code );
                    if ( file.Successful && file.AST != null )
                    {
                        file.AST.InternalData.RemoveValue ( "isRoot" );
                        var scope = new Scope ( node.Scope.Parser, node.Scope );
                        AnonymousFunctionExpression func = GetAnonymousFunctionExpression ( node, scope, file.AST );
                        // Add ... as argument because idk what
                        // people might pass to it and there're
                        // no ways to specify argument names so
                        // ¯\_(ツ)_/¯
                        func.AddArgument ( new VarArgExpression (
                            func,
                            scope,
                            GetTokenList ( "...", "...", "...", TokenType.Punctuation, AdjustRange ( node.Range, "..." ) )
                        ) );
                        return func;
                    }
                }
            }
            return node;
        }
    }
}
