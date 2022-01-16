using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Loretta.CodeAnalysis.Collections;
using Loretta.CodeAnalysis.Lua.StatisticsCollector.Mathematics;
using Loretta.CodeAnalysis.Text;
using Tsu.Numerics;

namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    internal class Program
    {
        private static readonly object s_reportLock = new object();
        private static GlobalStatistics.Builder? s_globalStatisticsBuilder = new GlobalStatistics.Builder();
        private static readonly ConcurrentBag<FileStatistics> s_fileStatistics = new ConcurrentBag<FileStatistics>();
        private static long s_parsedFiles, s_totalFiles;
        private static bool s_includeAllocations, s_includeTokenStats, s_noReportStatistics, s_gcInReport, s_noPrompts;
        private static long s_executionStart, s_nextReport = -1, s_reportInterval = TimeSpan.FromSeconds(5).Ticks;
        private static int s_warmupCount = 1, s_threadsCount = Environment.ProcessorCount / 4;

        private static ImmutableArray<string> ParseArguments(string[] args)
        {
            var patterns = ImmutableArray.CreateBuilder<string>(args.Length);
            for (var idx = 0; idx < args.Length; idx++)
            {
                switch (args[idx])
                {
                    case "--warmup-count":
                        s_warmupCount = int.Parse(args[idx + 1].Replace("_", ""));
                        idx += 1;
                        break;

                    case "--report-interval":
                        s_reportInterval = TimeSpan.Parse(args[idx + 1]).Ticks;
                        idx += 1;
                        break;

                    case "--threads":
                        s_threadsCount = int.Parse(args[idx + 1]);
                        idx += 1;
                        break;

                    case "--include-allocation":
                        s_includeAllocations = true;
                        break;

                    case "--include-token-stats":
                        s_includeTokenStats = true;
                        break;

                    case "--no-report-stats":
                        s_noReportStatistics = true;
                        break;

                    case "--gc-in-report":
                        s_gcInReport = true;
                        break;

                    case "-y":
                    case "--yes":
                        s_noPrompts = true;
                        break;

                    default:
                        patterns.Add(args[idx]);
                        break;
                }
            }
            return patterns.ToImmutable();
        }

        private static void Main(string[] args)
        {
            var files = ParseArguments(args)
                .SelectMany(file => Directory.GetFiles(".", file))
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.Length)
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount * 2)
                .Select(file =>
                {
                    using var stream = file.OpenRead();
                    return new SourceFile(file.Name, SourceText.From(stream, Encoding.UTF8));
                })
                .ToImmutableArray();
            s_totalFiles = files.Length;
            s_nextReport = Stopwatch.GetTimestamp() + s_reportInterval;

            // Warmup
            Warmup(files);
            PressKeyToContinue();

            InvokeGC();
            if (s_includeAllocations)
            {
                foreach (var file in files)
                {
                    ProcessFile(file);
                }
            }
            else
            {
                Parallel.ForEach(files, new ParallelOptions
                {
                    MaxDegreeOfParallelism = s_threadsCount
                }, ProcessFile);
            }

            ReportStatus("", s_parsedFiles);

            var globalStatistics = s_globalStatisticsBuilder!.Summarize();
            s_globalStatisticsBuilder = null;
            files = default;
            InvokeGC(); // Clear up as much as we can before generating the report.

            ReportWriter.WriteReport(
                "Report.xlsx",
                globalStatistics,
                s_fileStatistics.ToImmutableArray());
            PressKeyToContinue();
        }

        private static void PressKeyToContinue()
        {
            if (s_noPrompts)
                return;
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
            Console.WriteLine();
        }

        private static void Warmup(ImmutableArray<SourceFile> files)
        {

            Console.WriteLine("Warming up...");
            var addLock = new object();
            var reportLock = new object();
            var warmups = new SegmentedList<double>(1 << (int) Math.Ceiling(Math.Log2(files.Length * s_warmupCount)));
            var nextReport = -1L;
            var parsed = 0; var total = files.Length * s_warmupCount;
            var start = Stopwatch.GetTimestamp();
            Parallel.ForEach(files, new ParallelOptions
            {
                MaxDegreeOfParallelism = s_threadsCount
            }, (file, _s, idx) =>
            {
                for (var round = 0; round < s_warmupCount; round++)
                {
                    var tStart = Stopwatch.GetTimestamp();
                    _ = LuaSyntaxTree.ParseText(file.Text, path: file.FileName);
                    var tEnd = Stopwatch.GetTimestamp();
                    lock (addLock)
                        warmups.Add(tEnd - tStart);
                    var ourParsed = Interlocked.Increment(ref parsed);

                    if (nextReport <= Stopwatch.GetTimestamp() && Monitor.TryEnter(reportLock))
                    {
                        try
                        {
                            var delta = Stopwatch.GetTimestamp() - start;
                            var timePerItem = (double) delta / ourParsed;
                            var remaining = total - ourParsed;
                            var eta = (long) Math.Ceiling(remaining * timePerItem);
                            Console.WriteLine($"Warmup progress: {ourParsed}/{total}. ETA: {TimeSpan.FromTicks(eta)}");
                            nextReport = Stopwatch.GetTimestamp() + s_reportInterval;
                        }
                        finally
                        {
                            Monitor.Exit(reportLock);
                        }
                    }
                }
            });

            var statistics = new Statistics(warmups.Take(files.Length * s_warmupCount));
            using var writer = new IndentedTextWriter(Console.Out);
            writer.WriteLine($"Warmup results:");
            PrintStatistics(writer, statistics, static d => Duration.Format(ceil(d)));

            static long ceil(double val) => (long) Math.Ceiling(val);
        }

        private static void ProcessFile(SourceFile file)
        {
            Interlocked.CompareExchange(ref s_executionStart, Stopwatch.GetTimestamp(), 0);

            SyntaxTree tree;

            if (s_includeAllocations)
                InvokeGC();
            long mStart = 0, mEnd = 0;
            if (s_includeAllocations)
                mStart = GC.GetTotalMemory(true);
            var tStart = Stopwatch.GetTimestamp();
            tree = LuaSyntaxTree.ParseText(file.Text, path: file.FileName);
            var tEnd = Stopwatch.GetTimestamp();
            if (s_includeAllocations)
            {
                mEnd = GC.GetTotalMemory(false);
            }

            TokenStatistics.Builder? tokenStatisticsBuilder = null;
            FileFeatureStatistics? featureStatistics = null;
            if (s_includeTokenStats)
            {
                tokenStatisticsBuilder = new TokenStatistics.Builder();
                var walker = new LuaStatisticsSyntaxWalker(s_globalStatisticsBuilder!, tokenStatisticsBuilder);

                walker.Visit((Syntax.InternalSyntax.LuaSyntaxNode) tree.GetRoot().Green);
                featureStatistics = walker.FeatureStatistics;
            }

            var fileStatistics = new FileStatistics(
                            file.FileName,
                            new ParseStatistics(tEnd - tStart, mEnd - mStart),
                            tokenStatisticsBuilder?.SummarizeAndFree(),
                            featureStatistics,
                            DiagnosticStatistics.Collect(tree.GetDiagnostics()));

            s_globalStatisticsBuilder!.AddParse(fileStatistics);
            s_fileStatistics.Add(fileStatistics);
            var parsed = Interlocked.Increment(ref s_parsedFiles);

            if (s_nextReport <= Stopwatch.GetTimestamp() && Monitor.TryEnter(s_reportLock))
            {
                try
                {
                    if (s_gcInReport)
                        InvokeGC();
                    ReportStatus(file.FileName, parsed);
                    s_nextReport = Stopwatch.GetTimestamp() + s_reportInterval;
                }
                finally
                {
                    Monitor.Exit(s_reportLock);
                }
            }
        }

        private static void ReportStatus(string file, long parsed)
        {
            if (!s_noReportStatistics)
                Console.Clear();

            using var writer = new IndentedTextWriter(Console.Out, "    ");
            var delta = Stopwatch.GetTimestamp() - s_executionStart;
            var timePerItem = (double) delta / parsed;
            var remaining = s_totalFiles - parsed;
            var eta = (long) Math.Ceiling(remaining * timePerItem);
            writer.WriteLine($"{parsed}/{s_totalFiles} processed. ETA: {TimeSpan.FromTicks(eta)} Current file: {file}");

            if (s_noReportStatistics)
                return;

            var statistics = s_globalStatisticsBuilder!.Summarize();
            writer.WriteLine("Current Global Statistics:");
            writer.Indent++;
            {
                writer.WriteLine("Parse Time:");
                PrintStatistics(writer, statistics.ParseTimeStatistics, static d => Duration.Format(ceil(d)));

                if (s_includeAllocations)
                {
                    writer.WriteLine("Allocations:");
                    PrintStatistics(writer, statistics.AllocationStatistics!, FileSize.Format);
                }

                if (s_includeTokenStats)
                {
                    writer.WriteLine("Token Statistics:");
                    writer.Indent++;
                    {
                        writer.WriteLine("Token Count:");
                        PrintStatistics(writer, statistics.TokenCountStatistics!);
                        writer.WriteLine("Token Lengths:");
                        PrintStatistics(writer, statistics.TokenLengthStatistics!);
                    }
                    writer.Indent--;

                    writer.WriteLine("Total Diagnostics:");
                    writer.Indent++;
                    {
                        writer.WriteLine($"Errors:   {statistics.DiagnosticStatistics.ErrorCount}");
                        writer.WriteLine($"Warnings: {statistics.DiagnosticStatistics.WarningCount}");
                        writer.WriteLine($"Infos:    {statistics.DiagnosticStatistics.InformationCount}");
                        writer.WriteLine($"Total:    {statistics.DiagnosticStatistics.TotalCount}");
                    }
                    writer.Indent--;
                }
            }
            writer.Indent--;

            static long ceil(double val) => (long) Math.Ceiling(val);
        }

        private static void PrintStatistics(IndentedTextWriter writer, Statistics statistics, Func<double, string>? converter = null)
        {
            converter ??= static n => n.ToString();

            writer.Indent++;
            {
                writer.WriteLine($"Min:         {converter(statistics.Min)}");
                writer.WriteLine($"Lower Fence: {converter(statistics.LowerFence)}");
                writer.WriteLine($"Q1:          {converter(statistics.Q1)}");
                writer.WriteLine($"Median:      {converter(statistics.Median)}");
                writer.WriteLine($"Mean:        {converter(statistics.Mean)}");
                writer.WriteLine($"Q3:          {converter(statistics.Q3)}");
                writer.WriteLine($"UpperFence:  {converter(statistics.UpperFence)}");
                writer.WriteLine($"Max:         {converter(statistics.Max)}");
                writer.WriteLine($"Percentiles:");
                writer.Indent++;
                {
                    writer.WriteLine($"P0:      {converter(statistics.P0)}");
                    writer.WriteLine($"P25:     {converter(statistics.P25)}");
                    writer.WriteLine($"P50:     {converter(statistics.P50)}");
                    writer.WriteLine($"P67:     {converter(statistics.P67)}");
                    writer.WriteLine($"P80:     {converter(statistics.P80)}");
                    writer.WriteLine($"P85:     {converter(statistics.P85)}");
                    writer.WriteLine($"P90:     {converter(statistics.P90)}");
                    writer.WriteLine($"P95:     {converter(statistics.P95)}");
                    writer.WriteLine($"P99:     {converter(statistics.P99)}");
                    writer.WriteLine($"P100:    {converter(statistics.P100)}");
                }
                writer.Indent--;
            }
            writer.Indent--;
        }

        private static void InvokeGC()
        {
            for (var idx = 0; idx < 25; idx++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
