using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    internal static partial class ErrorFacts
    {
        private const string TitleSuffix = "_Title";
        private const string DescriptionSuffix = "_Description";
        private static readonly Lazy<ImmutableDictionary<ErrorCode, string>> s_categoriesMap = new(CreateCategoriesMap);

        public static string GetId(ErrorCode errorCode) =>
            MessageProvider.Instance.GetIdForErrorCode((int) errorCode);

        private static ImmutableDictionary<ErrorCode, string> CreateCategoriesMap()
        {
            var map = new Dictionary<ErrorCode, string>
            {
            };
            return map.ToImmutableDictionary();
        }

        public static partial bool IsWarning(ErrorCode code);
        public static partial bool IsFatal(ErrorCode code);
        public static partial bool IsInfo(ErrorCode code);
        public static partial bool IsHidden(ErrorCode code);

        public static DiagnosticSeverity GetSeverity(ErrorCode code)
        {
            if (code == ErrorCode.Void)
            {
                return InternalDiagnosticSeverity.Void;
            }
            else if (code == ErrorCode.Unknown)
            {
                return InternalDiagnosticSeverity.Unknown;
            }
            else if (IsWarning(code))
            {
                return DiagnosticSeverity.Warning;
            }
            else if (IsInfo(code))
            {
                return DiagnosticSeverity.Info;
            }
            else if (IsHidden(code))
            {
                return DiagnosticSeverity.Hidden;
            }
            else
            {
                return DiagnosticSeverity.Error;
            }
        }

        public static string GetMessage(ErrorCode code, CultureInfo? culture)
        {
            var message = ResourceManager.GetString(code.ToString(), culture);
            LorettaDebug.Assert(!string.IsNullOrEmpty(message), code.ToString());
            return message;
        }

        public static LocalizableResourceString GetMessageFormat(ErrorCode code)
            => new(code.ToString(), ResourceManager, typeof(ErrorFacts));

        public static LocalizableResourceString GetTitle(ErrorCode code)
            => new(code.ToString() + TitleSuffix, ResourceManager, typeof(ErrorFacts));

        public static LocalizableResourceString GetDescription(ErrorCode code)
            => new(code.ToString() + DescriptionSuffix, ResourceManager, typeof(ErrorFacts));

        public static string GetCategory(ErrorCode code)
            => s_categoriesMap.Value.TryGetValue(code, out var category)
            ? category
            : Diagnostic.CompilerDiagnosticCategory;

        private static System.Resources.ResourceManager? s_resourceManager;
        private static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (s_resourceManager == null)
                {
                    Interlocked.CompareExchange(
                        ref s_resourceManager,
                        LuaResources.ResourceManager,
                        null);
                }
                return s_resourceManager;
            }
        }
    }
}
