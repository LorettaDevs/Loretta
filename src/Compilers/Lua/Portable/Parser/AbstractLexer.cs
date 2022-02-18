using System.Text;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal class AbstractLexer : IDisposable
    {
        internal readonly SlidingTextWindow TextWindow;
        private List<SyntaxDiagnosticInfo>? _errors;

        public AbstractLexer(SourceText text)
        {
            LorettaDebug.Assert(text is not null);
            TextWindow = new SlidingTextWindow(text);
        }

        protected void Start()
        {
            TextWindow.Start();
            _errors = null;
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

        protected void AddError(ErrorCode code) => AddError(MakeError(0, TextWindow.Width, code));

        protected void AddError(ErrorCode code, params object[] args) => AddError(MakeError(0, TextWindow.Width, code, args));

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

        protected static SyntaxDiagnosticInfo MakeError(ErrorCode code) => new(code);

        protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args) => new(code, args);

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

        private int GetOffsetFromPosition(int position) =>
            position >= TextWindow.LexemeStartPosition ? position - TextWindow.LexemeStartPosition : position;

        #endregion AddError

        #endregion Error Handling

        public virtual void Dispose() => TextWindow.Dispose();
    }
}
