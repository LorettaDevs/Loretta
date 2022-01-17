// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Loretta.CodeAnalysis.Test.Utilities
{
    public static class MonoHelpers
    {
        public static bool IsRunningOnMono() => Loretta.Test.Utilities.ExecutionConditionUtil.IsMonoDesktop;
    }
}
