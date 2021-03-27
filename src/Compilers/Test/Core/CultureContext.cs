// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Globalization;
using System.Threading;

namespace Loretta.Test.Utilities
{
    public class CultureContext : IDisposable
    {
        private readonly CultureInfo _threadCulture;

        public CultureContext(CultureInfo cultureInfo)
        {
            _threadCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = cultureInfo;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _threadCulture;
        }
    }
}
