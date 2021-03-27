using System;
using System.Globalization;
using System.Threading;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    internal sealed class MessageProvider : CommonMessageProvider, IObjectWritable
    {
        public static readonly MessageProvider Instance = new MessageProvider();

        static MessageProvider()
        {
            ObjectBinder.RegisterTypeReader(typeof(MessageProvider), r => Instance);
        }

        private MessageProvider()
        {
        }

        bool IObjectWritable.ShouldReuseInSerialization => true;

        void IObjectWritable.WriteTo(ObjectWriter writer)
        {
            // write nothing, always read/deserialized as global Instance
        }

        public override DiagnosticSeverity GetSeverity(int code) => ErrorFacts.GetSeverity((ErrorCode) code);

        public override string LoadMessage(int code, CultureInfo? language)
            => ErrorFacts.GetMessage((ErrorCode) code, language);

        public override LocalizableString GetMessageFormat(int code) => ErrorFacts.GetMessageFormat((ErrorCode) code);

        public override LocalizableString GetDescription(int code) => ErrorFacts.GetDescription((ErrorCode) code);

        public override LocalizableString GetTitle(int code) => ErrorFacts.GetTitle((ErrorCode) code);

        public override string GetHelpLink(int code) => "";

        public override string GetCategory(int code) => ErrorFacts.GetCategory((ErrorCode) code);

        public override string CodePrefix => "LUA";

        public override string GetMessagePrefix(
            string id,
            DiagnosticSeverity severity,
            bool isWarningAsError,
            CultureInfo? culture)
        {
            return string.Format(
                culture,
                "{0} {1}",
                severity is DiagnosticSeverity.Error || isWarningAsError ? "error" : "warning",
                id);
        }

        public override int GetWarningLevel(int code) => 1;

        public override Type ErrorCodeType => typeof(ErrorCode);

        public override Diagnostic CreateDiagnostic(int code, Location location, params object[] args)
        {
            var info = new LuaDiagnosticInfo((ErrorCode) code, args);
            return new LuaDiagnostic(info, location);
        }

        public override Diagnostic CreateDiagnostic(DiagnosticInfo info)
            => new LuaDiagnostic(info, Location.None);

        public override ReportDiagnostic GetDiagnosticReport(DiagnosticInfo diagnosticInfo, CompilationOptions options) =>
            LuaDiagnosticFilter.GetDiagnosticReport(
                diagnosticInfo.Severity,
                true,
                diagnosticInfo.MessageIdentifier,
                diagnosticInfo.WarningLevel,
                Location.None,
                options.WarningLevel,
                options.GeneralDiagnosticOption,
                options.SpecificDiagnosticOptions,
                options.SyntaxTreeOptionsProvider,
                CancellationToken.None,
                out _);

        public override int ERR_BadDocumentationMode => (int) ErrorCode.ERR_BadDocumentationMode;
    }
}
