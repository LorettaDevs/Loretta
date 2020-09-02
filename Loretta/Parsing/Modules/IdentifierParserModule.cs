using System;
using System.Diagnostics.CodeAnalysis;
using GParse;
using GParse.Lexing;
using GParse.Parsing;
using GParse.Parsing.Parselets;
using Loretta.Lexing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Modules
{
    /// <summary>
    /// The module that parses identifiers.
    /// </summary>
    public class IdentifierParserModule : IPrefixParselet<LuaTokenType, Expression>
    {
        /// <summary>
        /// The module's instance.
        /// </summary>
        public static IdentifierParserModule Instance { get; } = new IdentifierParserModule ( );

        /// <summary>
        /// Registers the module in a parser builder.
        /// </summary>
        /// <param name="builder">The builder to register in.</param>
        public static void Register ( IPrattParserBuilder<LuaTokenType, Expression> builder ) =>
            builder.Register ( LuaTokenType.Identifier, Instance );

        /// <inheritdoc/>
        public Boolean TryParse (
            IPrattParser<LuaTokenType, Expression> genParser,
            IProgress<Diagnostic> diagnosicReporter,
            [NotNullWhen ( true )] out Expression expression )
        {
            if ( !genParser.TokenReader.Accept ( LuaTokenType.Identifier, out Token<LuaTokenType> identifier ) )
            {
                expression = default!;
                return false;
            }

            expression = new IdentifierExpression ( identifier, ( ( LuaParser ) genParser ).GetOrCreateVariable ( identifier, Scope.FindMode.CheckParents ) );
            return true;
        }
    }
}