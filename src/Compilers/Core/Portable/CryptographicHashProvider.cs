// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    internal abstract class CryptographicHashProvider
    {
        private ImmutableArray<byte> _lazySHA1Hash;
        private ImmutableArray<byte> _lazySHA256Hash;
        private ImmutableArray<byte> _lazySHA384Hash;
        private ImmutableArray<byte> _lazySHA512Hash;
        private ImmutableArray<byte> _lazyMD5Hash;

        internal abstract ImmutableArray<byte> ComputeHash(HashAlgorithm algorithm);

        internal ImmutableArray<byte> GetHash(AssemblyHashAlgorithm algorithmId)
        {
            using var algorithm = TryGetAlgorithm(algorithmId);
            // ERR_CryptoHashFailed has already been reported:
            if (algorithm == null)
            {
                return ImmutableArray.Create<byte>();
            }

            return algorithmId switch
            {
                AssemblyHashAlgorithm.None
                or AssemblyHashAlgorithm.Sha1 => GetHash(ref _lazySHA1Hash, algorithm),
                AssemblyHashAlgorithm.Sha256 => GetHash(ref _lazySHA256Hash, algorithm),
                AssemblyHashAlgorithm.Sha384 => GetHash(ref _lazySHA384Hash, algorithm),
                AssemblyHashAlgorithm.Sha512 => GetHash(ref _lazySHA512Hash, algorithm),
                AssemblyHashAlgorithm.MD5 => GetHash(ref _lazyMD5Hash, algorithm),
                _ => throw ExceptionUtilities.UnexpectedValue(algorithmId),
            };
        }

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

        internal static HashAlgorithmName GetAlgorithmName(SourceHashAlgorithm algorithmId)
        {
            return algorithmId switch
            {
                SourceHashAlgorithm.Sha1 => HashAlgorithmName.SHA1,
                SourceHashAlgorithm.Sha256 => HashAlgorithmName.SHA256,
                _ => throw ExceptionUtilities.UnexpectedValue(algorithmId),
            };
        }

        internal static HashAlgorithm? TryGetAlgorithm(AssemblyHashAlgorithm algorithmId)
        {
            return algorithmId switch
            {
                AssemblyHashAlgorithm.None or AssemblyHashAlgorithm.Sha1 => SHA1.Create(),
                AssemblyHashAlgorithm.Sha256 => SHA256.Create(),
                AssemblyHashAlgorithm.Sha384 => SHA384.Create(),
                AssemblyHashAlgorithm.Sha512 => SHA512.Create(),
                AssemblyHashAlgorithm.MD5 => MD5.Create(),
                _ => null,
            };
        }

        internal static bool IsSupportedAlgorithm(AssemblyHashAlgorithm algorithmId)
        {
            switch (algorithmId)
            {
                case AssemblyHashAlgorithm.None:
                case AssemblyHashAlgorithm.Sha1:
                case AssemblyHashAlgorithm.Sha256:
                case AssemblyHashAlgorithm.Sha384:
                case AssemblyHashAlgorithm.Sha512:
                case AssemblyHashAlgorithm.MD5:
                    return true;

                default:
                    return false;
            }
        }

        private ImmutableArray<byte> GetHash(ref ImmutableArray<byte> lazyHash, HashAlgorithm algorithm)
        {
            if (lazyHash.IsDefault)
            {
                ImmutableInterlocked.InterlockedCompareExchange(ref lazyHash, ComputeHash(algorithm), default);
            }

            return lazyHash;
        }

        internal const int Sha1HashSize = 20;

        internal static ImmutableArray<byte> ComputeSha1(Stream stream)
        {
            if (stream != null)
            {
                stream.Seek(0, SeekOrigin.Begin);
                using var hashProvider = SHA1.Create();
                return ImmutableArray.Create(hashProvider.ComputeHash(stream));
            }

            return ImmutableArray<byte>.Empty;
        }

        internal static ImmutableArray<byte> ComputeSha1(ImmutableArray<byte> bytes) => ComputeSha1(bytes.ToArray());

        internal static ImmutableArray<byte> ComputeSha1(byte[] bytes)
        {
            using (var hashProvider = SHA1.Create())
            {
                return ImmutableArray.Create(hashProvider.ComputeHash(bytes));
            }
        }

        internal static ImmutableArray<byte> ComputeHash(HashAlgorithmName algorithmName, IEnumerable<Blob> bytes)
        {
            using (var incrementalHash = IncrementalHash.CreateHash(algorithmName))
            {
                incrementalHash.AppendData(bytes);
                return ImmutableArray.Create(incrementalHash.GetHashAndReset());
            }
        }

        internal static ImmutableArray<byte> ComputeHash(HashAlgorithmName algorithmName, IEnumerable<ArraySegment<byte>> bytes)
        {
            using (var incrementalHash = IncrementalHash.CreateHash(algorithmName))
            {
                incrementalHash.AppendData(bytes);
                return ImmutableArray.Create(incrementalHash.GetHashAndReset());
            }
        }

        internal static ImmutableArray<byte> ComputeSourceHash(ImmutableArray<byte> bytes, SourceHashAlgorithm hashAlgorithm = SourceHashAlgorithmUtils.DefaultContentHashAlgorithm)
        {
            var algorithmName = GetAlgorithmName(hashAlgorithm);
            using (var incrementalHash = IncrementalHash.CreateHash(algorithmName))
            {
                incrementalHash.AppendData(bytes.ToArray());
                return ImmutableArray.Create(incrementalHash.GetHashAndReset());
            }
        }

        internal static ImmutableArray<byte> ComputeSourceHash(IEnumerable<Blob> bytes, SourceHashAlgorithm hashAlgorithm = SourceHashAlgorithmUtils.DefaultContentHashAlgorithm) => ComputeHash(GetAlgorithmName(hashAlgorithm), bytes);
    }
}
