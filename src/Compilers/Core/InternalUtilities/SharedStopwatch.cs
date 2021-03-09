// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Diagnostics;

namespace Loretta.Utilities
{
    internal readonly struct SharedStopwatch
    {
        private static readonly Stopwatch s_stopwatch = Stopwatch.StartNew();

        private readonly TimeSpan _started;

        private SharedStopwatch(TimeSpan started)
        {
            _started = started;
        }

        public TimeSpan Elapsed => s_stopwatch.Elapsed - _started;

        public static SharedStopwatch StartNew()
            => new SharedStopwatch(s_stopwatch.Elapsed);
    }
}
