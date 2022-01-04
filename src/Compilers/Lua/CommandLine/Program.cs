using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Experimental;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Text;
using Tsu.CLI.Commands;
using Tsu.CLI.Commands.Errors;
using Tsu.Numerics;
using Tsu.Timing;
using Minifying = Loretta.CodeAnalysis.Lua.Experimental.Minifying;

namespace Loretta.CLI
{
    public static class Program
    {
        private static readonly ConsoleTimingLogger s_logger = new ConsoleTimingLogger();
        private static bool s_shouldRun, s_printCurrentDir = true;

        public static void Main()
        {
            var commandManager = new ConsoleCommandManager();
            commandManager.LoadCommands(typeof(Program), null);
            commandManager.AddHelpCommand();

            s_shouldRun = true;
            while (s_shouldRun)
            {
                try
                {
                    if (s_printCurrentDir)
                        s_logger.Write(Environment.CurrentDirectory);
                    s_logger.Write("> ");
                    commandManager.Execute(s_logger.ReadLine());
                }
                catch (NonExistentCommandException ex)
                {
                    s_logger.LogError("Unexistent command '{0}'. Use the 'help' command to list all commands.", ex.Command);
                }
                catch (CommandInvocationException ex)
                {
                    s_logger.LogError("Error while executing '{0}': {1}\n{2}", ex.Command, ex.Message, ex.StackTrace!);
                }
            }
        }

        #region Settings

        [Command("@cd")]
        public static void SetPrintCurrentDir(string value)
        {
            switch (value.ToLowerInvariant())
            {
                case "true":
                case "on":
                case "yes":
                    s_printCurrentDir = true;
                    break;

                case "false":
                case "off":
                case "no":
                    s_printCurrentDir = false;
                    break;

                default:
                    s_logger.LogError("Invalid print current dir setting: {0}", value);
                    break;
            }
        }

        [Command("s"), Command("set")]
        public static void Set(string key, string value)
        {
            switch (key.ToLower().Replace("-", ""))
            {
                case "printcurrentdir":
                    SetPrintCurrentDir(value);
                    break;
            }
        }

        #endregion Settings

        [Command("q"), Command("quit"), Command("exit")]
        public static void Quit() => s_shouldRun = false;

        #region Current Directory Management

        [Command("cd")]
        public static void ChangeDirectory(string relativePath)
        {
            try
            {
                Environment.CurrentDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, relativePath));
            }
            catch (Exception ex)
            {
                s_logger.LogError("Error while changing directory: {0}", ex);
            }
        }

        [Command("ls")]
        public static void ListSymbols()
        {
            var di = new DirectoryInfo(Environment.CurrentDirectory);
            foreach (var directoryInfo in di.EnumerateDirectories())
                s_logger.WriteLine($"./{directoryInfo.Name}/");

            foreach (var fileInfo in di.EnumerateFiles())
                s_logger.WriteLine($"./{fileInfo.Name}");
        }

        #endregion Current Directory Management

        #region Loretta

        public enum LuaOptionsPreset
        {
            Lua51,
            Lua52,
            Lua53,
            LuaJIT20,
            LuaJIT21,
            GMod,
            Roblox,
            All,
        }

        private static LuaParseOptions PresetEnumToPresetOptions(LuaOptionsPreset preset)
        {
            return new LuaParseOptions(preset switch
            {
                LuaOptionsPreset.Lua51 => LuaSyntaxOptions.Lua51,
                LuaOptionsPreset.Lua52 => LuaSyntaxOptions.Lua52,
                LuaOptionsPreset.Lua53 => LuaSyntaxOptions.Lua53,
                LuaOptionsPreset.LuaJIT20 => LuaSyntaxOptions.LuaJIT20,
                LuaOptionsPreset.LuaJIT21 => LuaSyntaxOptions.LuaJIT21,
                LuaOptionsPreset.GMod => LuaSyntaxOptions.GMod,
                LuaOptionsPreset.Roblox => LuaSyntaxOptions.Roblox,
                LuaOptionsPreset.All => LuaSyntaxOptions.All,
                _ => throw new InvalidOperationException(),
            });
        }

        public enum ASTVisitors
        {
        }

        [Command("l"), Command("lex")]
        public static void Lex(LuaOptionsPreset preset, string path, bool printTokens = false)
        {
            if (!File.Exists(path))
            {
                s_logger.LogError("Provided path does not exist.");
                return;
            }

            var options = PresetEnumToPresetOptions(preset);
            SourceText sourceText;
            using (var stream = File.OpenRead(path))
                sourceText = SourceText.From(stream, Encoding.UTF8);

            ImmutableArray<SyntaxToken> tokens;
            using (s_logger.BeginOperation("Lexing"))
                tokens = SyntaxFactory.ParseTokens(sourceText, options: options).ToImmutableArray();

            s_logger.LogInformation($"{tokens.Length} tokens lexed.");
            if (printTokens)
            {
                var tokenNodes = tokens.Select(t => LuaTreeDumperConverter.Convert(t, true));
                var rootNode = new TreeDumperNode("Root", null, tokenNodes);
                s_logger.WriteLine(TreeDumper.DumpCompact(rootNode));
            }
        }

        [Command("p"), Command("parse")]
        public static void Parse(LuaOptionsPreset preset, string path, bool constantFold = false, bool printTree = false)
        {
            if (!File.Exists(path))
            {
                s_logger.LogError("Provided path does not exist.");
                return;
            }

            var options = PresetEnumToPresetOptions(preset);
            SourceText sourceText;
            using (var stream = File.OpenRead(path))
                sourceText = SourceText.From(stream, Encoding.UTF8);

            LuaSyntaxTree syntaxTree;
            using (s_logger.BeginOperation("Parsing"))
                syntaxTree = (LuaSyntaxTree) LuaSyntaxTree.ParseText(sourceText, options: options, path: path);

            SyntaxNode rootNode = syntaxTree.GetRoot();
            if (constantFold)
            {
                using (s_logger.BeginOperation("Constant Folding"))
                    rootNode = rootNode.ConstantFold();
            }

            using (s_logger.BeginOperation("Format"))
                rootNode = rootNode.NormalizeWhitespace();

            var diagnostics = syntaxTree.GetDiagnostics();
            foreach (var diagnostic in diagnostics)
                s_logger.WriteLine(diagnostic.ToString());
            s_logger.Write("Press any key to continue...");
            Console.ReadKey(true);
            s_logger.WriteLine("");

            if (printTree)
            {
                s_logger.WriteLine(TreeDumper.DumpCompact(LuaTreeDumperConverter.Convert(rootNode)));
            }
            else
            {
                rootNode.WriteTo(new ConsoleTimingLoggerTextWriter(s_logger));
                s_logger.WriteLine("");
            }

            var script = new Script(ImmutableArray.Create<SyntaxTree>(syntaxTree));
            var global = script.RootScope;
            s_logger.WriteLine("Global variables:");
            foreach (var variable in global.DeclaredVariables)
                s_logger.WriteLine($"    {variable.Kind} {variable.Name}");
        }

        [Command("e"), Command("expr"), Command("expression")]
        [RawInput]
        public static void ParseExpression(string input)
        {
            var options = LuaParseOptions.Default;
            string code;
            var presetName = input.Substring(0, input.IndexOf(' '));
            if (Enum.TryParse<LuaOptionsPreset>(presetName, true, out var presetEnum))
            {
                code = input[(input.IndexOf(' ') + 1)..];
                options = PresetEnumToPresetOptions(Enum.Parse<LuaOptionsPreset>(presetName, true));
            }
            else
            {
                code = input;
            }
            var text = SourceText.From(code, Console.InputEncoding);

            var expr = SyntaxFactory.ParseExpression(text, options);
            var diagnostics = expr.GetDiagnostics();
            foreach (var diagnostic in diagnostics)
                s_logger.WriteLine(diagnostic.ToString());

            expr = (ExpressionSyntax) expr.ConstantFold();
            expr = expr.NormalizeWhitespace();
            expr.WriteTo(new ConsoleTimingLoggerTextWriter(s_logger));
            s_logger.WriteLine("");
        }

        [Command("mp"), Command("mass-parse")]
        public static void MassParse(LuaOptionsPreset preset, params string[] patterns)
        {
            var files = patterns.SelectMany(pattern => Directory.EnumerateFiles(".", pattern, new EnumerationOptions
            {
                IgnoreInaccessible = true,
                MatchType = MatchType.Simple
            }))
                .ToArray();

            var options = PresetEnumToPresetOptions(preset);
            foreach (var file in files)
            {
                SourceText sourceText;
                using (var stream = File.OpenRead(file))
                    sourceText = SourceText.From(stream, Encoding.UTF8);

                var stopwatch = Stopwatch.StartNew();
                var tree = LuaSyntaxTree.ParseText(sourceText, options, file);
                stopwatch.Stop();
                s_logger.WriteLine($"{file}: {Duration.Format(stopwatch.ElapsedTicks)}");
                if (!tree.GetRoot().ContainsDiagnostics)
                    s_logger.LogError("Diagnostics were emitted.");
            }
        }

        public enum NamingStrategy
        {
            Alphabetical,
            Numerical,
            ZeroWidth
        }

        private static Minifying.NamingStrategy GetNamingStrategy(NamingStrategy namingStrategy)
        {
            return namingStrategy switch
            {
                NamingStrategy.Alphabetical => Minifying.NamingStrategies.Alphabetical,
                NamingStrategy.Numerical => Minifying.NamingStrategies.Numerical,
                NamingStrategy.ZeroWidth => Minifying.NamingStrategies.ZeroWidth,
            };
        }

        public enum SlotAllocator
        {
            Sequential,
            Sorted
        }

        private static Minifying.ISlotAllocator GetSlotAllocator(SlotAllocator slotAllocator)
        {
            return slotAllocator switch
            {
                SlotAllocator.Sequential => new Minifying.SequentialSlotAllocator(),
                SlotAllocator.Sorted => new Minifying.SortedSlotAllocator(),
            };
        }

        [Command("min"), Command("minify")]
        public static void Minify(
            string path,
            LuaOptionsPreset preset = LuaOptionsPreset.All,
            NamingStrategy namingStrategy = NamingStrategy.Numerical,
            SlotAllocator slotAllocator = SlotAllocator.Sorted,
            bool reformat = false)
        {
            if (!File.Exists(path))
            {
                s_logger.LogError("Provided path does not exist.");
                return;
            }

            var options = PresetEnumToPresetOptions(preset);
            SourceText sourceText;
            using (var stream = File.OpenRead(path))
                sourceText = SourceText.From(stream, Encoding.UTF8);

            LuaSyntaxTree syntaxTree;
            using (s_logger.BeginOperation("Parsing"))
                syntaxTree = (LuaSyntaxTree) LuaSyntaxTree.ParseText(sourceText, options: options, path: path);
            using (s_logger.BeginOperation("Minifying"))
                syntaxTree = (LuaSyntaxTree) syntaxTree.Minify(GetNamingStrategy(namingStrategy), GetSlotAllocator(slotAllocator));
            var root = syntaxTree.GetRoot();
            if (reformat)
            {
                using (s_logger.BeginOperation("Formatting"))
                    root = root.NormalizeWhitespace();
            }

            var diagnostics = syntaxTree.GetDiagnostics();
            foreach (var diagnostic in diagnostics)
                s_logger.WriteLine(diagnostic.ToString());

            s_logger.Write("Press any key to continue...");
            Console.ReadKey(true);
            s_logger.WriteLine("");
            root.WriteTo(new ConsoleTimingLoggerTextWriter(s_logger));
            s_logger.WriteLine("");
        }

        #endregion Loretta

        [Command("mlua"), Command("multi-lua"), Command("multilua")]
        public static void MultiLua(string scriptPath)
        {
            const string prefixTemplate = "[00:00:00.000000]";

            var versions = Directory.GetDirectories("binaries", "lua*");

            foreach (var version in versions)
            {
                var name = getFormattedLuaName(new DirectoryInfo(version).Name);
                var executable = Path.Combine(version, "lua.exe");

                var title = $"===== {name} ".PadRight(Console.WindowWidth - prefixTemplate.Length, '=');
                s_logger.WriteLine(title);
                var proc = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = executable,
                        ArgumentList = { scriptPath },
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    },
                    EnableRaisingEvents = true
                };
#pragma warning disable CA1416 // Validate platform compatibility
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    proc.StartInfo.LoadUserProfile = true;
#pragma warning restore CA1416 // Validate platform compatibility
                proc.OutputDataReceived += (_, args) =>
                {
                    if (args.Data is not null)
                        s_logger.WriteLine(args.Data);
                };
                proc.ErrorDataReceived += (_, args) =>
                {
                    if (args.Data is not null)
                        s_logger.LogError(args.Data);
                };
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                if (!proc.WaitForExit(2000))
                {
                    s_logger.LogError("Process has timed out, killing...");
                    proc.Kill(true);
                    s_logger.LogError("Killed.");
                }
                proc.WaitForExit();
            }

            static string getFormattedLuaName(string name)
            {
                if (name.StartsWith("luajit"))
                    return "LuaJIT " + name["luajit".Length..];
                else
                    return "Lua " + name["lua".Length..];
            }
        } // /MultiLua

        [RawInput]
        [Command("emlua"), Command("expr-multi-lua"), Command("exprmultilua")]
        public static void MultiLuaExpression(string expression)
        {
            const string prefixTemplate = "[00:00:00.000000]";

            var versions = Directory.GetDirectories("binaries", "lua*");

            foreach (var version in versions)
            {
                var name = getFormattedLuaName(new DirectoryInfo(version).Name);
                var executable = Path.Combine(version, "lua.exe");

                var title = $"===== {name} ".PadRight(Console.WindowWidth - prefixTemplate.Length, '=');
                s_logger.WriteLine(title);
                var proc = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = executable,
                        ArgumentList = { "-e", expression },
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    },
                    EnableRaisingEvents = true
                };
#pragma warning disable CA1416 // Validate platform compatibility
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    proc.StartInfo.LoadUserProfile = true;
#pragma warning restore CA1416 // Validate platform compatibility
                proc.OutputDataReceived += (_, args) =>
                {
                    if (args.Data is not null)
                        s_logger.WriteLine(args.Data);
                };
                proc.ErrorDataReceived += (_, args) =>
                {
                    if (args.Data is not null)
                        s_logger.LogError(args.Data);
                };
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                if (!proc.WaitForExit(2000))
                {
                    s_logger.LogError("Process has timed out, killing...");
                    proc.Kill(true);
                    s_logger.LogError("Killed.");
                }
                proc.WaitForExit();
            }

            static string getFormattedLuaName(string name)
            {
                if (name.StartsWith("luajit"))
                    return "LuaJIT " + name["luajit".Length..];
                else
                    return "Lua " + name["lua".Length..];
            }
        } // /MultiLuaExpression

        [Command("cls"), Command("clear")]
        public static void Clear() =>
            Console.Clear();

        #region Memory Usage

        private static readonly Process s_currentProc = Process.GetCurrentProcess();
        private static readonly Stack<(long gcMemory, long processMemory)> s_memoryStack = new Stack<(long gcMemory, long processMemory)>();

        [Command("m"), Command("mem")]
        public static void PrintMemoryUsage()
        {
            var gcmem = GC.GetTotalMemory(false);
            var procmem = s_currentProc.PrivateMemorySize64;
            s_logger.WriteLine($"Memory usage according to GC:       {FileSize.Format(gcmem)}");
            s_logger.WriteLine($"Memory usage according to Process:  {FileSize.Format(procmem)}");
        }

        [Command("mpush"), Command("memory-push")]
        public static void PushMemoryUsage()
        {
            var gcmem = GC.GetTotalMemory(false);
            var procmem = s_currentProc.PrivateMemorySize64;
            s_logger.WriteLine($"Memory usage according to GC:       {FileSize.Format(gcmem)}");
            s_logger.WriteLine($"Memory usage according to Process:  {FileSize.Format(procmem)}");
            s_memoryStack.Push((gcmem, procmem));
            s_logger.WriteLine("Memory usage pushed to stack.");
        }

        [Command("mcomp"), Command("memory-compare")]
        public static void CompareMemoryUsage()
        {
            var currgcmem = GC.GetTotalMemory(false);
            var currprocmem = s_currentProc.PrivateMemorySize64;
            s_logger.WriteLine($"Memory usage according to GC:       {FileSize.Format(currgcmem)}");
            s_logger.WriteLine($"Memory usage according to Process:  {FileSize.Format(currprocmem)}");

            if (s_memoryStack.Count == 0)
            {
                s_logger.LogError("Nothing on memory stack to compare to.");
                return;
            }

            (var oldgcmem, var oldprocmem) = s_memoryStack.Peek();
            (var Δgcmem, var Δprocmem) = (currgcmem - oldgcmem, currprocmem - oldprocmem);
            s_logger.WriteLine($"ΔMemory usage according to GC:      {(Δgcmem < 0 ? $"-{FileSize.Format(-Δgcmem)}" : FileSize.Format(Δgcmem))}");
            s_logger.WriteLine($"ΔMemory usage according to Process: {(Δprocmem < 0 ? $"-{FileSize.Format(-Δprocmem)}" : FileSize.Format(Δprocmem))}");
        }

        [Command("mpop"), Command("memory-pop")]
        public static void PopMemoryUsage()
        {
            if (s_memoryStack.Count == 0)
            {
                s_logger.LogError("Nothing on memory stack to pop.");
                return;
            }

            CompareMemoryUsage();
            s_memoryStack.Pop();
        }

        [Command("gc")]
        [HelpDescription("Invokes the garbage collector")]
        public static void InvokeGC()
        {
            for (var idx = 0; idx < 1000; idx++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                GC.WaitForPendingFinalizers();
            }
        }

        #endregion Memory Usage
    }
}