using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Scoping
{
    public class ScriptTestsBase
    {
        protected static (SyntaxTree, Script) ParseScript(string code)
        {
            var tree = LuaSyntaxTree.ParseText(code);
            Assert.Empty(tree.GetDiagnostics());
            var script = new Script(ImmutableArray.Create(tree));
            return (tree, script);
        }

        protected static Script ParseScript(params string[] codes)
        {
            var idx = 0;
            var trees = Array.ConvertAll(codes, (code) => LuaSyntaxTree.ParseText(code, path: $"codes_{idx++}"));
            if (trees.Any(t => t.GetDiagnostics().Any()))
            {
                foreach (var tree in trees)
                {
                    if (tree.GetDiagnostics().Any())
                    {
                        Console.WriteLine(tree.FilePath + ":");
                        foreach (var diagnostic in tree.GetDiagnostics())
                        {
                            Console.WriteLine(diagnostic);
                        }
                    }
                }
            }
            Assert.Empty(trees.SelectMany(tree => tree.GetDiagnostics()));
            var script = new Script(ImmutableArray.CreateRange(trees));
            return script;
        }
    }
}