﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// A class that represents no location at all. Useful for errors in command line options, for example.
    /// </summary>
    /// <remarks></remarks>
    internal sealed class NoLocation : Location
    {
        public static readonly Location Singleton = new NoLocation();

        private NoLocation()
        {
        }

        public override LocationKind Kind => LocationKind.None;

        public override bool Equals(object? obj) => (object) this == obj;

        public override int GetHashCode() =>
            // arbitrary number, since all NoLocation's are equal
            0x16487756;
    }
}
