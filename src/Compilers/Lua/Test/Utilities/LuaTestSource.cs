// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Diagnostics;

namespace Loretta.CodeAnalysis.Lua.Test.Utilities
{
    /// <summary>
    /// Represents the source code used for a Lua test. Allows us to have single helpers that enable all the different ways
    /// we typically provide source in testing.
    /// </summary>
    public readonly struct LuaTestSource
    {
        public static LuaTestSource None => new(null);

        public object Value { get; }

        private LuaTestSource(object value)
        {
            Value = value;
        }

        public SyntaxTree[] GetSyntaxTrees(LuaParseOptions parseOptions, string sourceFileName = "")
        {
            switch (Value)
            {
                case string source:
                    return new[] { LuaTestBase.Parse(source, filename: sourceFileName, parseOptions) };
                case string[] sources:
                    Debug.Assert(string.IsNullOrEmpty(sourceFileName));
                    return LuaTestBase.Parse(parseOptions, sources);
                case SyntaxTree tree:
                    Debug.Assert(parseOptions == null);
                    Debug.Assert(string.IsNullOrEmpty(sourceFileName));
                    return new[] { tree };
                case SyntaxTree[] trees:
                    Debug.Assert(parseOptions == null);
                    Debug.Assert(string.IsNullOrEmpty(sourceFileName));
                    return trees;
                case LuaTestSource[] testSources:
                    return testSources.SelectMany(s => s.GetSyntaxTrees(parseOptions, sourceFileName)).ToArray();
                case null:
                    return Array.Empty<SyntaxTree>();
                default:
                    throw new Exception($"Unexpected value: {Value}");
            }
        }

        public static implicit operator LuaTestSource(string source) => new(source);
        public static implicit operator LuaTestSource(string[] source) => new(source);
        public static implicit operator LuaTestSource(SyntaxTree source) => new(source);
        public static implicit operator LuaTestSource(SyntaxTree[] source) => new(source);
        public static implicit operator LuaTestSource(List<SyntaxTree> source) => new(source.ToArray());
        public static implicit operator LuaTestSource(ImmutableArray<SyntaxTree> source) => new(source.ToArray());
        public static implicit operator LuaTestSource(LuaTestSource[] source) => new(source);
    }
}
