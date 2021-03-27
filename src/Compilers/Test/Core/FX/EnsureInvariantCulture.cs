// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace Loretta.Test.Utilities
{
    public class EnsureInvariantCulture : IDisposable
    {
        private readonly CultureInfo _threadCulture;
        private readonly int _threadId;

        public EnsureInvariantCulture()
        {
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _threadCulture = CultureInfo.CurrentCulture;

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        public void Dispose()
        {
            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);

            if (_threadId == Thread.CurrentThread.ManagedThreadId)
            {
                CultureInfo.CurrentCulture = _threadCulture;
            }
        }
    }
}
