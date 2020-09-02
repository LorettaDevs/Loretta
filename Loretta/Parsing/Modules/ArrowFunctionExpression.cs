using System;
using GParse;
using GParse.Parsing;
using GParse.Parsing.Parselets;
using Loretta.Lexing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Modules
{
    /// <summary>
    /// The module that parses arrow function expressions. Unimplemented.
    /// </summary>
    public class ArrowFunctionExpression : IPrefixParselet<LuaTokenType, Expression>
    {
        /// <inheritdoc />
        public Boolean TryParse ( IPrattParser<LuaTokenType, Expression> parser, IProgress<Diagnostic> diagnosticReporter, out Expression expression ) => throw new NotImplementedException ( );
    }
}