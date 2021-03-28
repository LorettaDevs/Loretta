// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Globalization;
using Loretta.CodeAnalysis;

namespace Loretta.Test.Utilities
{
    internal abstract class TestMessageProvider : CommonMessageProvider
    {
        public override Type ErrorCodeType => throw new NotImplementedException();

        public override Diagnostic CreateDiagnostic(int code, Location location, params object[] args) => throw new NotImplementedException();

        public override Diagnostic CreateDiagnostic(DiagnosticInfo info) => throw new NotImplementedException();

        public override string GetMessagePrefix(string id, DiagnosticSeverity severity, bool isWarningAsError, CultureInfo culture) => throw new NotImplementedException();

        public override DiagnosticSeverity GetSeverity(int code) => throw new NotImplementedException();

        public override string LoadMessage(int code, CultureInfo language) => throw new NotImplementedException();

        public override string CodePrefix => throw new NotImplementedException();

        public override int GetWarningLevel(int code) => throw new NotImplementedException();

        public override int ERR_BadDocumentationMode => throw new NotImplementedException();
    }
}
