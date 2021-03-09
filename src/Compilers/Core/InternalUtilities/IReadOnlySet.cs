// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.Utilities
{
    internal interface IReadOnlySet<T>
    {
        int Count { get; }
        bool Contains(T item);
    }
}
