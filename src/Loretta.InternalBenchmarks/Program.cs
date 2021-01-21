using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Loretta.InternalBenchmarks
{
    class Program
    {
        static void Main ( String[] args ) =>
            BenchmarkSwitcher.FromAssembly ( typeof ( Program ).Assembly ).Run ( args );
    }
}
