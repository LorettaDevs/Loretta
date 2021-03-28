// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Loretta.CodeAnalysis;

namespace Loretta.Test.Utilities
{
    internal sealed class VirtualizedRelativePathResolver : RelativePathResolver
    {
        private readonly HashSet<string> _existingFullPaths;

        public VirtualizedRelativePathResolver(IEnumerable<string> existingFullPaths, string baseDirectory = null, ImmutableArray<string> searchPaths = default(ImmutableArray<string>))
            : base(searchPaths.NullToEmpty(), baseDirectory)
        {
            _existingFullPaths = new HashSet<string>(existingFullPaths, StringComparer.Ordinal);
        }

        protected override bool FileExists(string fullPath)
        {
            // normalize path to remove '..' and '.'
            return _existingFullPaths.Contains(Path.GetFullPath(fullPath));
        }
    }
}
