namespace Loretta.CodeAnalysis.Lua
{
    internal sealed class LuaDiagnosticInfo : DiagnosticInfo
    {
        internal LuaDiagnosticInfo(ErrorCode code)
            : this(code, Array.Empty<object>())
        {
        }

        internal LuaDiagnosticInfo(ErrorCode errorCode, object[] arguments)
            : base(Lua.MessageProvider.Instance, (int) errorCode, arguments)
        {
        }

        internal LuaDiagnosticInfo(bool isWarningAsError, ErrorCode errorCode, object[] arguments)
            : base(Lua.MessageProvider.Instance, isWarningAsError, (int) errorCode, arguments)
        {
        }
    }
}
