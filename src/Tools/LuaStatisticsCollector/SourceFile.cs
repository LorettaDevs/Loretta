using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    internal record SourceFile(string FileName, SourceText Text);
}
