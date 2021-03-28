// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Test.Utilities
{
    public interface ISyntaxNodeKindProvider
    {
        string Kind(object node);
    }
}
