using System;
using GParse;
using GParse.Lexing;
using GParse.Parsing;
using GParse.Parsing.Parselets;
using Loretta.Lexing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Modules
{
    public class ArrowFunctionExpression : IPrefixParselet<LuaTokenType, Expression>
    {
        public Boolean TryParse ( IPrattParser<LuaTokenType, Expression> parser, IProgress<Diagnostic> diagnosticReporter, out Expression expression ) => throw new NotImplementedException ( );
    }
}