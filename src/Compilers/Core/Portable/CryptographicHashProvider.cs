// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Cryptography;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    internal static class CryptographicHashProvider
    {
        internal static int GetHashSize(SourceHashAlgorithm algorithmId)
        {
            return algorithmId switch
            {
                SourceHashAlgorithm.Sha1 => 160 / 8,
                SourceHashAlgorithm.Sha256 => 256 / 8,
                _ => throw ExceptionUtilities.UnexpectedValue(algorithmId),
            };
        }

        internal static HashAlgorithm? TryGetAlgorithm(SourceHashAlgorithm algorithmId)
        {
            return algorithmId switch
            {
                SourceHashAlgorithm.Sha1 => SHA1.Create(),
                SourceHashAlgorithm.Sha256 => SHA256.Create(),
                _ => null,
            };
        }
    }
}
