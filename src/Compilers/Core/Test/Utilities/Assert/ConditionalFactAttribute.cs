// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.IO;
using System.Runtime.InteropServices;
using Loretta.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Loretta.Test.Utilities
{
    public class ConditionalTheoryAttribute : TheoryAttribute
    {
        /// <summary>
        /// This property exists to prevent users of ConditionalFact from accidentally putting documentation
        /// in the Skip property instead of Reason. Putting it into Skip would cause the test to be unconditionally
        /// skipped vs. conditionally skipped which is the entire point of this attribute.
        /// </summary>
        [Obsolete("ConditionalTheory should use Reason or AlwaysSkip")]
        public new string Skip
        {
            get => base.Skip;
            set => base.Skip = value;
        }

        /// <summary>
        /// Used to unconditionally Skip a test. For the rare occasion when a conditional test needs to be
        /// unconditionally skipped (typically short term for a bug to be fixed).
        /// </summary>
        public string AlwaysSkip
        {
            get => base.Skip;
            set => base.Skip = value;
        }

        public string Reason { get; set; }

        public ConditionalTheoryAttribute(params Type[] skipConditions)
        {
            foreach (var skipCondition in skipConditions)
            {
                var condition = (ExecutionCondition) Activator.CreateInstance(skipCondition);
                if (condition.ShouldSkip)
                {
                    base.Skip = Reason ?? condition.SkipReason;
                    break;
                }
            }
        }
    }

    public abstract class ExecutionCondition
    {
        public abstract bool ShouldSkip { get; }
        public abstract string SkipReason { get; }
    }

    public static class ExecutionConditionUtil
    {
        public static ExecutionArchitecture Architecture => IntPtr.Size switch
        {
            4 => ExecutionArchitecture.x86,
            8 => ExecutionArchitecture.x64,
            _ => throw new InvalidOperationException($"Unrecognized pointer size {IntPtr.Size}")
        };
        public static ExecutionConfiguration Configuration =>
#if DEBUG
            ExecutionConfiguration.Debug;
#elif RELEASE
            ExecutionConfiguration.Release;
#else
#error Unsupported Configuration
#endif

        public static bool IsWindows => Path.DirectorySeparatorChar == '\\';
        public static bool IsUnix => !IsWindows;
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsDesktop => RuntimeUtilities.IsDesktopRuntime;
        public static bool IsWindowsDesktop => IsWindows && IsDesktop;
        public static bool IsMonoDesktop => Type.GetType("Mono.Runtime") != null;
        public static bool IsMono => MonoHelpers.IsRunningOnMono();
        public static bool IsCoreClr => !IsDesktop;
        public static bool IsCoreClrUnix => IsCoreClr && IsUnix;
        public static bool IsMonoOrCoreClr => IsMono || IsCoreClr;
        public static bool RuntimeSupportsCovariantReturnsOfClasses => Type.GetType("System.Runtime.CompilerServices.RuntimeFeature")?.GetField("CovariantReturnsOfClasses") != null;
    }

    public enum ExecutionArchitecture
    {
        x86,
        x64,
    }

    public enum ExecutionConfiguration
    {
        Debug,
        Release,
    }
}
