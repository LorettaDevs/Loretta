// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Loretta.Test.Utilities
{
    public static class EqualityUnit
    {
        public static EqualityUnit<T> Create<T>(T value)
        {
            return new EqualityUnit<T>(value);
        }
    }
}
