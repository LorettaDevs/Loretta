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
    public class IdentifierParserModule : IPrefixParselet<LuaTokenType, Expression>
    {
        public static IdentifierParserModule Instance { get; } = new IdentifierParserModule ( );

        public static void Register ( IPrattParserBuilder<LuaTokenType, Expression> builder ) =>
            builder.Register ( LuaTokenType.Identifier, Instance );

        public Boolean TryParse ( IPrattParser<LuaTokenType, Expression> genParser, IProgress<Diagnostic> diagnosicReporter, [NotNullWhen ( true )] out Expression expression )
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