using System;
using System.Collections.Generic;
using GParse;
using GParse.IO;
using GParse.Lexing;
using GParse.Lexing.Modules;
using Loretta.Utilities;

namespace Loretta.Lexing.Modules
{
    public class IdentifierLexerModule : ILexerModule<LuaTokenType>
    {
        protected readonly HashSet<String> Keywords = new HashSet<String> ( StringComparer.Ordinal );

        protected readonly HashSet<String> Operators = new HashSet<String> ( StringComparer.Ordinal );

        protected readonly Dictionary<String, (LuaTokenType type, Object? value)> Literals = new Dictionary<String, (LuaTokenType, Object?)> ( );

        public String Name => "Identifier Lexer Module";

        public String? Prefix => null;

        public LuaOptions LuaOptions { get; }

        public IdentifierLexerModule ( LuaOptions luaOptions )
        {
            this.LuaOptions = luaOptions;
        }

        public IdentifierLexerModule ( LuaOptions luaOptions, IEnumerable<String> keywords )
            : this ( luaOptions )
        {
            this.Keywords.UnionWith ( keywords );
        }

        public IdentifierLexerModule ( LuaOptions luaOptions, IEnumerable<String> keywords, IEnumerable<String> operators )
            : this ( luaOptions, keywords )
        {
            this.Operators.UnionWith ( operators );
        }

        #region Language Identifiers Management

        public void AddKeyword ( String keyword ) => this.Keywords.Add ( keyword );

        public void AddKeywords ( IEnumerable<String> keywords ) => this.Keywords.UnionWith ( keywords );

        public void AddOperator ( String @operator ) => this.Operators.Add ( @operator );

        public void AddOperators ( IEnumerable<String> operators ) => this.Operators.UnionWith ( operators );

        public void AddLiteral ( String literal, LuaTokenType tokenType, Object? value ) => this.Literals.Add ( literal, (tokenType, value) );

        #endregion Language Identifiers Management

        public Boolean CanConsumeNext ( IReadOnlyCodeReader reader )
        {
            // Non-equals operations with Nullable<Char> should be exception safe and always return false
            var peek = reader.Peek ( );
            return peek.HasValue
                   && CharUtils.IsValidFirstIdentifierChar ( this.LuaOptions.UseLuaJitIdentifierRules, peek.Value );
        }

        public Token<LuaTokenType> ConsumeNext ( ICodeReader reader, IProgress<Diagnostic> diagnosticReporter )
        {
            SourceLocation start = reader.Location;
            var identifier = this.LuaOptions.UseLuaJitIdentifierRules
                ? reader.ReadStringWhile ( ch => CharUtils.IsValidTrailingIdentifierChar ( true, ch ) )
                : reader.ReadStringWhile ( ch => CharUtils.IsValidFirstIdentifierChar ( false, ch ) );
            SourceRange range = start.To ( reader.Location );

            if ( this.Keywords.Contains ( identifier ) )
            {
                return new Token<LuaTokenType> ( identifier, identifier, identifier, LuaTokenType.Keyword, range );
            }
            else if ( this.Operators.Contains ( identifier ) )
            {
                return new Token<LuaTokenType> ( identifier, identifier, identifier, LuaTokenType.Operator, range );
            }
            else if ( this.Literals.TryGetValue ( identifier, out (LuaTokenType type, Object? value) info ) )
            {
                return new Token<LuaTokenType> ( identifier, identifier, info.value, info.type, range );
            }
            else
            {
                return new Token<LuaTokenType> ( identifier, identifier, identifier, LuaTokenType.Identifier, range );
            }
        }
    }
}