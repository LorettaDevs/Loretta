namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class Lexer
    {
        private bool ScanIdentifierOrKeyword(ref TokenInfo info)
        {
            if (ScanIdentifier(ref info))
            {
                if (!_cache.TryGetKeywordKind(info.Text!, out info.Kind))
                {
                    info.ContextualKind = info.Kind = SyntaxKind.IdentifierToken;
                }
                else if (SyntaxFacts.IsContextualKeyword(info.Kind, _options.SyntaxOptions))
                {
                    info.ContextualKind = info.Kind;
                    info.Kind = SyntaxKind.IdentifierToken;
                }

                if (info.Kind is SyntaxKind.None)
                    info.Kind = SyntaxKind.IdentifierToken;

                return true;
            }

            return false;
        }

        private bool ScanIdentifier(ref TokenInfo info)
        {
            var startPosition = TextWindow.Position;
            var hasUnicode = false;

            while (true)
            {
                char ch;
                switch (ch = TextWindow.PeekChar())
                {
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case '_':
                        // All of these characters are valid inside an identifier.
                        // consume it and keep processing.
                        TextWindow.AdvanceChar();
                        continue;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (TextWindow.Position == startPosition)
                        {
                            return false;
                        }
                        else
                        {
                            goto case 'A';
                        }

                    case '\0':
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                    case '!':
                    case '%':
                    case '(':
                    case ')':
                    case '*':
                    case '+':
                    case ',':
                    case '-':
                    case '.':
                    case '/':
                    case ':':
                    case ';':
                    case '<':
                    case '=':
                    case '>':
                    case '?':
                    case '[':
                    case ']':
                    case '^':
                    case '{':
                    case '|':
                    case '}':
                    case '~':
                    case '"':
                    case '\'':
                        // All of the following characters are not valid in an 
                        // identifier.  If we see any of them, then we know we're
                        // done.
                        if (TextWindow.Position == startPosition)
                        {
                            TextWindow.Reset(startPosition);
                            return false;
                        }

                        info.Text = info.StringValue = TextWindow.GetText(true);
                        if (hasUnicode && !_options.SyntaxOptions.UseLuaJitIdentifierRules)
                            AddError(ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion);
                        return true;

                    default:
                        if (ch >= 0x7F && !IsAtEnd(ch))
                        {
                            hasUnicode = true;
                            goto case 'A';
                        }
                        else
                        {
                            // Any other characters are not valid in an identifier.
                            goto case '\0';
                        }
                }
            }
        }
    }
}
