using System;
using GParse;
using GParse.Lexing;
using Loretta.Lexing.Modules;

namespace Loretta.Lexing
{
    public class LuaLexerBuilder : ModularLexerBuilder<LuaTokenType>
    {
        public LuaLexerBuilder ( )
        {
            const String longStringExpr = @"\[(=*)\[([\S\s]*?)\]\1\]";

            #region Identifier Module Initialization

            var identifierModule = new IdentifierLexerModule ( new[]
            {
                // do ... end
                "do", "end",

                // while ... do ... end
                "while", "do", "end",

                // repeat ... until ...
                "repeat", "until",

                // if ... then ... elseif ... then ... else ... end
                "if", "then", "elseif", "then", "else", "end",

                // for <v> = <i>, <e>, <inc> do ... end
                "for", "do", "end",

                // for <list> in <exp> do ... end
                "for",  "in", "do", "end",

                // function <name> <args> ... end
                "function", "end",

                // local function ... end
                "local", "function", "end",

                // local <a, b, c> = <d, f, g>
                "local",

                // control flow
                "return", "break", "goto", "continue"
            }, new[]
            {
                "and", "or", "not"
            } );
            identifierModule.AddLiteral ( "nil", LuaTokenType.Nil, null );
            identifierModule.AddLiteral ( "true", LuaTokenType.Boolean, true );
            identifierModule.AddLiteral ( "false", LuaTokenType.Boolean, false );

            this.AddModule ( identifierModule );

            #endregion Identifier Module Initialization

            #region Punctuation

            this.AddRegex ( "[", LuaTokenType.LBracket, @"\[(?![=\[])" );
            this.AddLiteral ( "]", LuaTokenType.RBracket, "]" );
            this.AddLiteral ( "(", LuaTokenType.LParen, "(" );
            this.AddLiteral ( ")", LuaTokenType.RParen, ")" );
            this.AddLiteral ( "{", LuaTokenType.LCurly, "{" );
            this.AddLiteral ( "}", LuaTokenType.RCurly, "}" );
            this.AddLiteral ( ";", LuaTokenType.Semicolon, ";" );
            this.AddLiteral ( ",", LuaTokenType.Comma, "," );
            this.AddModule ( new DotLexerModule ( ) );
            this.AddLiteral ( ":", LuaTokenType.Colon, ":" );
            this.AddLiteral ( "::", LuaTokenType.GotoLabelDelimiter, "::" );

            #endregion Punctuation

            #region Operators

            #region Infix Operators

            this.AddLiteral ( "+", LuaTokenType.Operator, "+" );
            this.AddLiteral ( "-", LuaTokenType.Operator, "-" );
            this.AddLiteral ( "*", LuaTokenType.Operator, "*" );
            this.AddLiteral ( "/", LuaTokenType.Operator, "/" );
            this.AddLiteral ( "^", LuaTokenType.Operator, "^" );
            this.AddLiteral ( "..", LuaTokenType.Operator, ".." );
            this.AddLiteral ( "=", LuaTokenType.Operator, "=" );
            this.AddLiteral ( "%", LuaTokenType.Operator, "%" );

            // Roblox Lua compound assignment
            this.AddLiteral ( "+=", LuaTokenType.Operator, "+=" );
            this.AddLiteral ( "-=", LuaTokenType.Operator, "-=" );
            this.AddLiteral ( "*=", LuaTokenType.Operator, "*=" );
            this.AddLiteral ( "/=", LuaTokenType.Operator, "/=" );
            this.AddLiteral ( "^=", LuaTokenType.Operator, "^=" );
            this.AddLiteral ( "%=", LuaTokenType.Operator, "%=" );
            this.AddLiteral ( "..=", LuaTokenType.Operator, "..=" );

            #endregion Infix Operators

            #region Prefix Operators

            this.AddLiteral ( "#", LuaTokenType.Operator, "#" );

            #endregion Prefix Operators

            #region Boolean Operators

            this.AddLiteral ( "==", LuaTokenType.Operator, "==" );
            this.AddLiteral ( "~=", LuaTokenType.Operator, "~=" );
            this.AddLiteral ( ">=", LuaTokenType.Operator, ">=" );
            this.AddLiteral ( "<=", LuaTokenType.Operator, "<=" );
            this.AddLiteral ( ">", LuaTokenType.Operator, ">" );
            this.AddLiteral ( "<", LuaTokenType.Operator, "<" );

            #endregion Boolean Operators

            #region Language Version Specific Operators

            // GLua operators
            this.AddLiteral ( "~=", LuaTokenType.Operator, "!=", "~=" );
            this.AddLiteral ( "and", LuaTokenType.Operator, "&&", "and" );
            this.AddLiteral ( "or", LuaTokenType.Operator, "||", "or" );
            this.AddLiteral ( "not", LuaTokenType.Operator, "!", "not" );

            #endregion Language Version Specific Operators

            #endregion Operators

            #region Literals

            this.AddModule ( new ShortStringLexerModule ( ) );
            this.AddRegex ( "long-string", LuaTokenType.LongString, longStringExpr, "[", match => match.Groups[2].Value );

            this.AddModule ( new NumberLexerModule ( ) );
            this.AddLiteral ( "...", LuaTokenType.VarArg, "..." );

            #endregion Literals

            #region Others

            this.AddRegex ( "shebang", LuaTokenType.Shebang, @"#!([^\r\n]+)", "#!", m => m.Groups[1].Value );

            this.AddRegex ( "comment", LuaTokenType.Comment, @"--([^\r\n]*)", "--", match => match.Groups[1].Value, true );
            this.AddRegex ( "long-comment", LuaTokenType.LongComment, "--" + longStringExpr, "--[", match => match.Groups[2].Value, true );
            this.AddRegex ( "whitespace", LuaTokenType.Whitespace, @"\s+", null, ws => ws.Value, true );

            this.AddRegex ( "comment", LuaTokenType.Comment, @"\/\/([^\r\n]*)", "//", match => match.Groups[1].Value, true );
            this.AddRegex ( "long-comment", LuaTokenType.LongComment, @"\/\*([\S\s]*?)\*\/", "/*", match => match.Groups[1].Value, true );

            #endregion Others
        }

        public ILexer<LuaTokenType> CreateLexer ( String contents, IProgress<Diagnostic> diagnosticReporter ) =>
            this.BuildLexer ( contents, diagnosticReporter );
    }
}
