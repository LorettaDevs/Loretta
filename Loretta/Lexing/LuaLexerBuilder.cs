using System;
using System.Collections.Generic;
using GParse;
using GParse.Lexing;
using Loretta.Lexing.Modules;

namespace Loretta.Lexing
{
    /// <summary>
    /// The lexer builder for loretta's lexer.
    /// </summary>
    public class LuaLexerBuilder : ModularLexerBuilder<LuaTokenType>
    {
        /// <summary>
        /// Initializes this lexer builder with the provided options.
        /// </summary>
        /// <param name="luaOptions">The options to be used by this builder and the lexer.</param>
        public LuaLexerBuilder ( LuaOptions luaOptions )
        {
            const String longStringExpr = /*lang=regex*/@"\[(=*)\[([\S\s]*?)\]\1\]";

            #region Identifier Module Initialization

            var keywords = new List<String> ( 30 )
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

                // for <list> in <list> do ... end
                "for",  "in", "do", "end",

                // function <name> <args> ... end
                "function", "end",

                // local function ... end
                "local", "function", "end",

                // local <a, b, c> = <d, f, g>
                "local",

                // control flow
                "return", "break", "goto"
            };
            if ( luaOptions.ContinueType == ContinueType.Keyword )
                keywords.Add ( "continue" );
            var identifierModule = new IdentifierLexerModule ( luaOptions, keywords, new[]
            {
                "and", "or", "not"
            } );
            identifierModule.AddLiteral ( "nil", LuaTokenType.Nil, null );
            identifierModule.AddLiteral ( "true", LuaTokenType.Boolean, true );
            identifierModule.AddLiteral ( "false", LuaTokenType.Boolean, false );

            this.AddModule ( identifierModule );

            #endregion Identifier Module Initialization

            this.AddModule ( new DotLexerModule ( ) );

            #region Punctuation

            this.AddRegex ( "[", LuaTokenType.LBracket, /*lang=regex*/@"\[(?![=\[])" );
            this.AddLiteral ( "]", LuaTokenType.RBracket, "]" );
            this.AddLiteral ( "(", LuaTokenType.LParen, "(" );
            this.AddLiteral ( ")", LuaTokenType.RParen, ")" );
            this.AddLiteral ( "{", LuaTokenType.LCurly, "{" );
            this.AddLiteral ( "}", LuaTokenType.RCurly, "}" );
            this.AddLiteral ( ";", LuaTokenType.Semicolon, ";" );
            this.AddLiteral ( ",", LuaTokenType.Comma, "," );
            this.AddLiteral ( ":", LuaTokenType.Colon, ":" );
            if ( luaOptions.AcceptGoto )
                this.AddLiteral ( "::", LuaTokenType.GotoLabelDelimiter, "::" );

            #endregion Punctuation

            #region Operators

            #region Infix Operators

            this.AddLiteral ( "+", LuaTokenType.Operator, "+" );
            this.AddLiteral ( "-", LuaTokenType.Operator, "-" );
            this.AddLiteral ( "*", LuaTokenType.Operator, "*" );
            this.AddLiteral ( "/", LuaTokenType.Operator, "/" );
            this.AddLiteral ( "^", LuaTokenType.Operator, "^" );
            this.AddLiteral ( "=", LuaTokenType.Operator, "=" );
            this.AddLiteral ( "%", LuaTokenType.Operator, "%" );

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
            if ( luaOptions.AcceptGModCOperators )
            {
                this.AddLiteral ( "~=", LuaTokenType.Operator, "!=", "~=" );
                this.AddLiteral ( "and", LuaTokenType.Operator, "&&", "and" );
                this.AddLiteral ( "or", LuaTokenType.Operator, "||", "or" );
                this.AddLiteral ( "not", LuaTokenType.Operator, "!", "not" );
            }

            // Roblox Lua compound assignment
            if ( luaOptions.AcceptCompoundAssignment )
            {
                this.AddLiteral ( "+=", LuaTokenType.Operator, "+=" );
                this.AddLiteral ( "-=", LuaTokenType.Operator, "-=" );
                this.AddLiteral ( "*=", LuaTokenType.Operator, "*=" );
                this.AddLiteral ( "/=", LuaTokenType.Operator, "/=" );
                this.AddLiteral ( "^=", LuaTokenType.Operator, "^=" );
                this.AddLiteral ( "%=", LuaTokenType.Operator, "%=" );
                this.AddLiteral ( "..=", LuaTokenType.Operator, "..=" );
            }

            #endregion Language Version Specific Operators

            #endregion Operators

            #region Literals

            this.AddModule ( new ShortStringLexerModule ( luaOptions ) );
            this.AddRegex ( "long-string", LuaTokenType.LongString, longStringExpr, "[", match => match.Groups[2].Value );

            this.AddModule ( new NumberLexerModule ( luaOptions ) );

            #endregion Literals

            #region Others

            if ( luaOptions.AcceptShebang )
            {
                this.AddRegex ( "shebang", LuaTokenType.Shebang, /*lang=regex*/@"#!([^\r\n]+)", "#!", m => m.Groups[1].Value );
            }

            this.AddRegex ( "comment", LuaTokenType.Comment, /*lang=regex*/@"--([^\r\n]*)", "--", match => match.Groups[1].Value, true );
            this.AddRegex ( "long-comment", LuaTokenType.LongComment, "--" + longStringExpr, "--[", match => match.Groups[2].Value, true );
            this.AddRegex ( "whitespace", LuaTokenType.Whitespace, /*lang=regex*/@"\s+", null, ws => ws.Value, true );

            if ( luaOptions.AcceptCCommentSyntax )
            {
                this.AddRegex ( "comment", LuaTokenType.Comment, /*lang=regex*/@"\/\/([^\r\n]*)", "//", match => match.Groups[1].Value, true );
                this.AddRegex ( "long-comment", LuaTokenType.LongComment, /*lang=regex*/@"\/\*([\S\s]*?)\*\/", "/*", match => match.Groups[1].Value, true );
            }

            #endregion Others
        }

        /// <summary>
        /// Creates a lexer from the provided code string and diagnostic reporter.
        /// </summary>
        /// <param name="code">The code to lex.</param>
        /// <param name="diagnosticReporter">The diagnostic reporter.</param>
        /// <returns>The lexer.</returns>
        public ILexer<LuaTokenType> CreateLexer ( String code, IProgress<Diagnostic> diagnosticReporter ) =>
            this.BuildLexer ( code, diagnosticReporter );
    }
}