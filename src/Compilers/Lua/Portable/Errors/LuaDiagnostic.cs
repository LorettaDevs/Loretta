using System;

namespace Loretta.CodeAnalysis.Lua
{
    internal sealed class LuaDiagnostic : DiagnosticWithInfo
    {
        internal LuaDiagnostic(DiagnosticInfo info, Location location, bool isSuppressed = false)
            : base(info, location, isSuppressed)
        {
        }

        public override string ToString() => LuaDiagnosticFormatter.Instance.Format(this);

        internal override Diagnostic WithLocation(Location location)
        {
            if (location is null)
                throw new ArgumentNullException(nameof(location));
            if (Location != location)
                return new LuaDiagnostic(Info, location, IsSuppressed);
            return this;
        }

        internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
        {
            if (Severity != severity)
                return new LuaDiagnostic(Info.GetInstanceWithSeverity(severity), Location, IsSuppressed);
            return this;
        }

        internal override Diagnostic WithIsSuppressed(bool isSuppressed)
        {
            if (IsSuppressed != isSuppressed)
                return new LuaDiagnostic(Info, Location, isSuppressed);
            return this;
        }
    }
}
