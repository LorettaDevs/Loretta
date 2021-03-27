// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Loretta.CodeAnalysis;

namespace Loretta.Test.Utilities
{
    internal static class ModuleInitializer
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new ThrowingTraceListener());
        }
    }
}
