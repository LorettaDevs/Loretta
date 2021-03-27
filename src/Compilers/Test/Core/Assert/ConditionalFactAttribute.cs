// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Loretta.CodeAnalysis.Test.Utilities;
using Loretta.Utilities;
using Xunit;

namespace Loretta.Test.Utilities
{
    /// <summary>
    /// Container for common skip reasons. Secondary benefit allows us to use find all ref to
    /// discover the set of tests affected by a particular scenario.
    /// </summary>
    public static class ConditionalSkipReason
    {
        public const string NoPiaNeedsDesktop = "NoPia is only supported on desktop";
        public const string NetModulesNeedDesktop = "Net Modules are only supported on desktop";
        public const string RestrictedTypesNeedDesktop = "Restricted types are only supported on desktop";
        public const string NativePdbRequiresDesktop = "Native PDB tests can only execute on windows desktop";
        public const string TestExecutionHasNewLineDependency = "Test execution depends on OS specific new lines";

        /// <summary>
        /// There are certain types which only appear in the desktop runtime and tests which depend on them
        /// can't be run on CoreClr.
        /// </summary>
        public const string TestExecutionNeedsDesktopTypes = "Test execution depends on desktop types";

        /// <summary>
        /// There are certain types, like PermissionSet, which are only available by default in runtimes that exist
        /// on Windows. These types can be added using extra assemblies but that is not done in our unit tests.
        /// </summary>
        public const string TestExecutionNeedsWindowsTypes = "Test execution depends on windows only types";

        public const string TestExecutionHasCOMInterop = "Test execution depends on COM Interop";
        public const string TestHasWindowsPaths = "Test depends on Windows style paths";
        public const string TestExecutionNeedsFusion = "Test depends on desktop fusion loader API";

        public const string WinRTNeedsWindowsDesktop = "WinRT is only supported on Windows desktop";

        /// <summary>
        /// Mono issues around Default Interface Methods
        /// </summary>
        public const string MonoDefaultInterfaceMethods = "Mono can't execute this default interface method test yet";
    }

    public class ConditionalFactAttribute : FactAttribute
    {
        /// <summary>
        /// This property exists to prevent users of ConditionalFact from accidentally putting documentation
        /// in the Skip property instead of Reason. Putting it into Skip would cause the test to be unconditionally
        /// skipped vs. conditionally skipped which is the entire point of this attribute.
        /// </summary>
        [Obsolete("ConditionalFact should use Reason or AlwaysSkip", error: true)]
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

        public ConditionalFactAttribute(params Type[] skipConditions)
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
        public static ExecutionArchitecture Architecture => (IntPtr.Size) switch
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

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Intentional.")]
    public class x86 : ExecutionCondition
    {
        public override bool ShouldSkip => ExecutionConditionUtil.Architecture != ExecutionArchitecture.x86;

        public override string SkipReason => "Target platform is not x86";
    }

    public class HasShiftJisDefaultEncoding : ExecutionCondition
    {
        public override bool ShouldSkip => Encoding.GetEncoding(0)?.CodePage != 932;

        public override string SkipReason => "OS default codepage is not Shift-JIS (932).";
    }

    public class HasEnglishDefaultEncoding : ExecutionCondition
    {
        public override bool ShouldSkip
        {
            get
            {
                try
                {
                    return Encoding.GetEncoding(0)?.CodePage != 1252;
                }
                catch (EntryPointNotFoundException)
                {
                    // Mono is throwing this exception on recent runs. Need to just assume false in this case while the
                    // bug is tracked down.
                    // https://github.com/mono/mono/issues/12603
                    return false;
                }
            }
        }

        public override string SkipReason => "OS default codepage is not Windows-1252.";
    }

    public class IsEnglishLocal : ExecutionCondition
    {
        public override bool ShouldSkip =>
            !CultureInfo.CurrentUICulture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
            !CultureInfo.CurrentCulture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase);

        public override string SkipReason => "Current culture is not en";
    }

    public class IsRelease : ExecutionCondition
    {
#if DEBUG
        public override bool ShouldSkip => true;
#else
        public override bool ShouldSkip => false;
#endif

        public override string SkipReason => "Test not supported in DEBUG";
    }

    public class IsDebug : ExecutionCondition
    {
#if DEBUG
        public override bool ShouldSkip => false;
#else
        public override bool ShouldSkip => true;
#endif

        public override string SkipReason => "Test not supported in RELEASE";
    }

    public class WindowsOnly : ExecutionCondition
    {
        public override bool ShouldSkip => !ExecutionConditionUtil.IsWindows;
        public override string SkipReason => "Test not supported on Mac and Linux";
    }

    public class WindowsDesktopOnly : ExecutionCondition
    {
        public override bool ShouldSkip => !ExecutionConditionUtil.IsWindowsDesktop;
        public override string SkipReason => "Test only supported on Windows desktop";
    }

    public class CovariantReturnRuntimeOnly : ExecutionCondition
    {
        public override bool ShouldSkip => !ExecutionConditionUtil.RuntimeSupportsCovariantReturnsOfClasses;
        public override string SkipReason => "Test only supported on runtimes that support covariant returns";
    }

    public class UnixLikeOnly : ExecutionCondition
    {
        public override bool ShouldSkip => !PathUtilities.IsUnixLikePlatform;
        public override string SkipReason => "Test not supported on Windows";
    }

    public class WindowsOrLinuxOnly : ExecutionCondition
    {
        public override bool ShouldSkip => ExecutionConditionUtil.IsMacOS;
        public override string SkipReason => "Test not supported on macOS";
    }
}
