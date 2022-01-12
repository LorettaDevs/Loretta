using System.Collections.Generic;
using System.Text;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal class AbstractLexer
    {
        protected readonly SourceText _text;
        protected readonly SourceTextReader _reader;
        private readonly StringTable _strings;

        private List<SyntaxDiagnosticInfo>? _errors;
        protected int _start;

        public AbstractLexer(SourceText text)
        {
            LorettaDebug.Assert(text is not null);
            _text = text;
            // TODO: Either make an SourceTextCodeReader or reimplement the
            // Lexer without the ICodeReader.
            _reader = new SourceTextReader(text);
            _strings = new StringTable();
        }

        public int Length => _reader.Length;

        public int Position => _reader.Position;

        public SourceText Text => _text;

        protected int LexemeStart => _start;

        protected int LexemeLength => Position - LexemeStart;

        public void Restore(int position) => _reader.Position = position;

        protected void Start()
        {
            _errors = null;
            _start = _reader.Position;
        }

        #region Error Handling

        protected bool HasErrors => _errors is not null;

        protected SyntaxDiagnosticInfo[]? GetErrors(int leadingTriviaWidth)
        {
            if (_errors == null)
            {
                return null;
            }

            if (leadingTriviaWidth > 0)
            {
                var array = new SyntaxDiagnosticInfo[_errors.Count];
                for (var i = 0; i < _errors.Count; i++)
                {
                    // fixup error positioning to account for leading trivia
                    array[i] = _errors[i].WithOffset(_errors[i].Offset + leadingTriviaWidth);
                }

                return array;
            }
            else
            {
                return _errors.ToArray();
            }
        }

        #region AddError

        protected void AddError(int position, int width, ErrorCode code) => AddError(MakeError(position, width, code));

        protected void AddError(int position, int width, ErrorCode code, params object[] args) => AddError(MakeError(position, width, code, args));

        protected void AddError(ErrorCode code) => AddError(MakeError(0, LexemeLength, code));

        protected void AddError(ErrorCode code, params object[] args) => AddError(MakeError(0, LexemeLength, code, args));

        protected void AddError(SyntaxDiagnosticInfo error)
        {
            if (error != null)
            {
                if (_errors == null)
                {
                    _errors = new List<SyntaxDiagnosticInfo>(8);
                }

                _errors.Add(error);
            }
        }

        protected static SyntaxDiagnosticInfo MakeError(ErrorCode code) => new SyntaxDiagnosticInfo(code);

        protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args) => new SyntaxDiagnosticInfo(code, args);

        protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code)
        {
            var offset = GetOffsetFromPosition(position);
            return new SyntaxDiagnosticInfo(offset, width, code);
        }

        protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code, params object[] args)
        {
            var offset = GetOffsetFromPosition(position);
            return new SyntaxDiagnosticInfo(offset, width, code, args);
        }

        private int GetOffsetFromPosition(int position) => position > _start ? position - _start : position;

        #endregion AddError

        #endregion Error Handling

        #region GetText

        protected string Intern(int start, int length) => _strings.Add(_text.ToString(new TextSpan(start, length)));

        protected string Intern(StringBuilder builder) => _strings.Add(builder);

        protected string GetText(bool intern) => GetText(LexemeStart, LexemeLength, intern);

        protected string GetText(int start, int length, bool intern)
        {
            // PERF: Whether interning or not, there are some frequently occurring
            // easy cases we can pick off easily.
            switch (length)
            {
                case 0: return string.Empty;
                case 1:
                    switch (_text[start])
                    {
                        case ' ': return " ";
                        case '\n': return "\n";
                        // k, v; _, v; x, y; x; y; i; j and k loops are common.
                        case 'k': return "k";
                        case 'v': return "v";
                        case '_': return "_";
                        case 'x': return "x";
                        case 'y': return "y";
                        case 'i': return "i";
                        case 'j': return "j";
                    }
                    break;
                case 2:
                    switch (_text[start], _text[start + 1])
                    {
                        case ('\r', '\n'): return "\r\n";
                        case ('-', '-'): return "--";
                        case ('/', '/'): return "//";
                    }
                    break;
                case 3:
                    switch (_text[start], _text[start + 1], _text[start + 2])
                    {
                        case ('-', '-', ' '): return "-- ";
                        case ('/', '/', ' '): return "// ";
                        case ('p', 'l', 'y'): return "ply";
                        case ('v', 'a', 'l'): return "val";
                        case ('i', 'd', 'x'): return "idx";
                        case ('t', 'b', 'l'): return "tbl";
                    }
                    break;
            }

            return intern ? Intern(start, length) : _text.ToString(new TextSpan(start, length));
        }

        #endregion GetText
    }
}
