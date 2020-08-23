using System;
using System.Collections.Immutable;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor.ReadWrite;

namespace Loretta.Parsing.Visitor
{
    public class UselessAssignmentRemover : TreeFolderBase
    {
        private readonly SimpleReadWriteTracker _readWriteTracker;

        public UselessAssignmentRemover ( SimpleReadWriteTracker readWriteTracker )
        {
            this._readWriteTracker = readWriteTracker;
        }

        public override LuaASTNode VisitAssignment ( AssignmentStatement node )
        {
            var variables = node.Variables.Select ( var => ( Expression ) this.VisitNode ( var ) )
                                          .ToList ( );
            var values = node.Values.Select ( val => ( Expression ) this.VisitNode ( val ) )
                                    .Take ( variables.Count )
                                    .ToList ( );

            Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
            var variablesTokens = new ArraySegment<Token<LuaTokenType>> ( tokens, 0, variables.Count - 1 ).ToList ( );
            Token<LuaTokenType> equals = tokens[variablesTokens.Count];
            var valueTokens = new ArraySegment<Token<LuaTokenType>> ( tokens, variablesTokens.Count + 1, values.Count - 1 ).ToList ( );

            var i = 0;
            for ( ; i < variables.Count; i++ )
            {
                if ( values.Count > i
                     && this._readWriteTracker.GetContainerForTree ( variables[i] ) is ReadWriteContainer left
                     && ( left.Reads.Count == 0
                          || ( this._readWriteTracker.GetContainerForTree ( values[i] ) is ReadWriteContainer right
                               && left == right ) ) )
                {
                    variables.RemoveAt ( i );
                    values.RemoveAt ( i );
                    variablesTokens.RemoveAt ( i );
                    valueTokens.RemoveAt ( i );
                    i--;
                }
            }

            if ( !variables.SequenceEqual ( node.Variables ) || !values.SequenceEqual ( node.Values ) )
            {
                return new AssignmentStatement ( variables, variablesTokens, equals, values, valueTokens ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public override LuaASTNode VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node )
        {
            if ( node.Values.Length > 0 )
            {
                var values = node.Values.Select ( val => ( Expression ) this.VisitNode ( val ) )
                                        .Take ( node.Identifiers.Length )
                                        .ToList ( );

                if ( !values.SequenceEqual ( node.Values ) )
                {
                    Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                    Token<LuaTokenType> localKw = tokens[0];
                    var identTokens = new ArraySegment<Token<LuaTokenType>> ( tokens, 1, node.Identifiers.Length - 1 );
                    Token<LuaTokenType> equals = tokens[1 + identTokens.Count];
                    var valTokens = new ArraySegment<Token<LuaTokenType>> ( tokens, 2 + identTokens.Count, values.Count - 1 );

                    return new LocalVariableDeclarationStatement ( localKw, node.Identifiers, identTokens, equals, values, valTokens ) { Semicolon = node.Semicolon };
                }
            }

            return node;
        }
    }
}