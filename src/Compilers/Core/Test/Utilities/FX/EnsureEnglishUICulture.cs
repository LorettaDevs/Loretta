// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;

namespace Loretta.Test.Utilities
{
    [PublicAPI]
    public class EnsureEnglishUICulture : IDisposable
    {
        public static CultureInfo? PreferredOrNull
        {
            get
            {
                var currentUiCultureName = CultureInfo.CurrentUICulture.Name;
                if (currentUiCultureName.Length == 0 || currentUiCultureName.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return CultureInfo.InvariantCulture;
            }
        }

        private          bool         _needToRestore;
        private readonly CultureInfo? _threadUiCulture;
        private readonly int          _threadId;

        public EnsureEnglishUICulture()
        {
            _threadId = Environment.CurrentManagedThreadId;
            var preferred = PreferredOrNull;

            if (preferred == null) return;
            _threadUiCulture = CultureInfo.CurrentUICulture;
            _needToRestore   = true;

            CultureInfo.CurrentUICulture = preferred;
        }

        public void Dispose()
        {
            Debug.Assert(_threadId == Environment.CurrentManagedThreadId);

            if (_needToRestore && _threadId == Environment.CurrentManagedThreadId)
            {
                _needToRestore = false;
                CultureInfo.CurrentUICulture = _threadUiCulture!;
            }
            GC.SuppressFinalize(this);
        }
    }
}
