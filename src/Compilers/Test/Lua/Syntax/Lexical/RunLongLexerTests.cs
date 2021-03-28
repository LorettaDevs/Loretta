using System;
using Loretta.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Lexical
{
    public class RunLongLexerTests : ExecutionCondition
    {
        public override bool ShouldSkip =>
            Environment.GetEnvironmentVariable("LORETTA_LONG_LEXER_TESTS") is not ("1" or "true" or "TRUE" or "True");

        public override string SkipReason =>
            "LORETTA_LONG_LEXER_TESTS environment variable was not found.";
    }
}