// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Represents the possible compilation stages for which it is possible to get diagnostics
    /// (errors).
    /// </summary>
    internal enum CompilationStage
    {
        Parse,
        Declare,
        Compile,
    }
}
