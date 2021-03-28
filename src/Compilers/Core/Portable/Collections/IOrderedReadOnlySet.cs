// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Collections
{
    internal interface IOrderedReadOnlySet<T> : IReadOnlySet<T>, IReadOnlyList<T>
    {
    }
}
