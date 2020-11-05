using System;
using System.Collections.Generic;
using GParse;
using GParse.IO;
using GParse.Lexing;
using GParse.Lexing.Modules;
using Loretta.Utilities;

namespace Loretta.Lexing.Modules
{
    /// <summary>
    /// Parses identifiers in the lexer.
    /// </summary>
    public class IdentifierLexerModule : ILexerModule<LuaTokenType>
    {
        /// <summary>
        /// The identifier keywords registered with the module.
        /// </summary>
        protected readonly HashSet<String> Keywords = new HashSet<String> ( StringComparer.Ordinal );

        /// <summary>
        /// The identifier operators registered with the module.
        /// </summary>
        protected readonly HashSet<String> Operators = new HashSet<String> ( StringComparer.Ordinal );

        /// <summary>
        /// The identifier literals registered with the module.
        /// </summary>
        protected readonly Dictionary<String, (LuaTokenType type, Object? value)> Literals =
            new Dictionary<String, (LuaTokenType, Object?)> ( );

        /// <inheritdoc />
        public String Name => "Identifier Lexer Module";

        /// <inheritdoc />
        public String? Prefix => null;

        /// <summary>
        /// The <see cref="Loretta.LuaOptions" /> being used by this module.
        /// </summary>
        public LuaOptions LuaOptions { get; }

        /// <summary>
        /// Initializes this module with the provided options.
        /// </summary>
        /// <param name="luaOptions">The options to be used by this module.</param>
        public IdentifierLexerModule ( LuaOptions luaOptions )
        {
            this.LuaOptions = luaOptions;
        }

        /// <summary>
        /// Initializes this module with the provided options and keywords.
        /// </summary>
        /// <param name="luaOptions"><inheritdoc cref="IdentifierLexerModule(LuaOptions)" /></param>
        /// <param name="keywords">The keywords to be recognized by this module.</param>
        public IdentifierLexerModule ( LuaOptions luaOptions, IEnumerable<String> keywords )
            : this ( luaOptions )
        {
            this.Keywords.UnionWith ( keywords );
        }

        /// <summary>
        /// Initializes this module with the provided options, keywords and operators.
        /// </summary>
        /// <param name="luaOptions">
        /// <inheritdoc cref="IdentifierLexerModule(LuaOptions,IEnumerable{String})" />
        /// </param>
        /// <param name="keywords">
        /// <inheritdoc cref="IdentifierLexerModule(LuaOptions,IEnumerable{String})" />
        /// </param>
        /// <param name="operators">The operators to be recognized by this module.</param>
        public IdentifierLexerModule ( LuaOptions luaOptions, IEnumerable<String> keywords, IEnumerable<String> operators )
            : this ( luaOptions, keywords )
        {
            this.Operators.UnionWith ( operators );
        }

        #region Language Identifiers Management

        /// <summary>
        /// Adds an identifier that should be recognized as a keyword by this module.
        /// </summary>
        /// <param name="keyword">The identifier to be recognized as a keyword.</param>
        public void AddKeyword ( String keyword ) => this.Keywords.Add ( keyword );

        /// <summary>
        /// Adds identifiers that should be recognized as keywords by this module.
        /// </summary>
        /// <param name="keywords">The identifiers to be rocognized as keywords.</param>
        public void AddKeywords ( IEnumerable<String> keywords ) => this.Keywords.UnionWith ( keywords );

        /// <summary>
        /// Adds an identifier that should be recognized as an operator by this module.
        /// </summary>
        /// <param name="operator">The identifier that should be recognized as an operator.</param>
        public void AddOperator ( String @operator ) => this.Operators.Add ( @operator );

        /// <summary>
        /// Adds identifiers that should be recognized as operators by this module.
        /// </summary>
        /// <param name="operators">The identifiers that should be recognized as operators.</param>
        public void AddOperators ( IEnumerable<String> operators ) => this.Operators.UnionWith ( operators );

        /// <summary>
        /// Adds an identifier that should be recognized as a literal by this module.
        /// </summary>
        /// <param name="literal">The identifier that should be recognized as a literal.</param>
        /// <param name="tokenType">The type of the literal token.</param>
        /// <param name="value">The value of the literal token.</param>
        public void AddLiteral ( String literal, LuaTokenType tokenType, Object? value ) => this.Literals.Add ( literal, (tokenType, value) );

        #endregion Language Identifiers Management

        /// <inheritdoc />
        public Boolean CanConsumeNext ( IReadOnlyCodeReader reader )
        {
            // Non-equals operations with Nullable<Char> should be exception safe and always return false
            var peek = reader.Peek ( );
            return peek.HasValue
                   && CharUtils.IsValidFirstIdentifierChar ( this.LuaOptions.UseLuaJitIdentifierRules, peek.Value );
        }

        /// <inheritdoc />
        public Token<LuaTokenType> ConsumeNext ( ICodeReader reader, IProgress<Diagnostic> diagnosticReporter )
        {
            SourceLocation start = reader.Location;
            var identifier = this.LuaOptions.UseLuaJitIdentifierRules
                ? reader.ReadStringWhile ( ch => CharUtils.IsValidTrailingIdentifierChar ( true, ch ) )
                : reader.ReadStringWhile ( ch => CharUtils.IsValidTrailingIdentifierChar ( false, ch ) );
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