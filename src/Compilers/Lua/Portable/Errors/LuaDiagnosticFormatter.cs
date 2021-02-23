namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The Lua diagnostic formatter.
    /// </summary>
    public class LuaDiagnosticFormatter : DiagnosticFormatter
    {
        internal LuaDiagnosticFormatter() { }

        /// <summary>
        /// The diagnostic formatter instance.
        /// </summary>
        public static new LuaDiagnosticFormatter Instance { get; } = new LuaDiagnosticFormatter();
    }
}
