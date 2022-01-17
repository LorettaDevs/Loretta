// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Reflection;

namespace Loretta.CodeAnalysis.Test.Utilities
{
    /// <summary>
    /// Hide all of the runtime specific implementations of types that we need to use when multi-targeting.
    /// </summary>
    public static partial class RuntimeUtilities
    {
        internal static bool IsDesktopRuntime =>
#if NET472
            true;
#elif NETCOREAPP
            false;
#elif NETSTANDARD2_0
            throw new PlatformNotSupportedException();
#else
#error Unsupported configuration
#endif
        internal static bool IsCoreClrRuntime => !IsDesktopRuntime;

        /// <summary>
        /// Get the location of the assembly that contains this type
        /// </summary>
        internal static string GetAssemblyLocation(Type type) =>
            type.GetTypeInfo().Assembly.Location;
    }
}
