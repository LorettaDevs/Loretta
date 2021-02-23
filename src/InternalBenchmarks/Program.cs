using System;
using BenchmarkDotNet.Running;

namespace Loretta.InternalBenchmarks
{
    internal class Program
    {
        private static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
