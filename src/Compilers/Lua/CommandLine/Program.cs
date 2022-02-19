using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.Text;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Experimental;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Text;
using Tsu.Numerics;
using Tsu.Timing;
using Minifying = Loretta.CodeAnalysis.Lua.Experimental.Minifying;

namespace Loretta.CLI
{
    public static class Program
    {
        private static readonly ConsoleTimingLogger s_logger = new();
        private static bool s_shouldRun, s_printCurrentDir = false, s_printOutputPrefixed = false;
        private static readonly RootCommand s_rootCommand;

        private static TextWriter OutputWriter =>
            s_printOutputPrefixed ? new ConsoleTimingLoggerTextWriter(s_logger) : Console.Out;

        public static void Main()
        {
            var timingConsole = new TimingLoggerConsole(s_logger);

            s_shouldRun = true;
            while (s_shouldRun)
            {
                try
                {
                    if (s_printCurrentDir)
                        s_logger.Write(Environment.CurrentDirectory);
                    s_logger.Write("> ");

                    var line = s_logger.ReadLine() ?? throw new Exception("Unable to read line from input.");
                    var spaceIdx = line.IndexOf(' ');
                    if (spaceIdx != -1)
                    {
                        var verb = line[..spaceIdx];
                        var rest = line[(spaceIdx + 1)..];
                        if (verb is "e" or "expr" or "expression" && rest is not ("-h" or "--help"))
                        {
                            ParseExpression(rest);
                            continue;
                        }
                        else if (verb is "emlua" or "expr-multi-lua" or "exprmultilua" && rest is not ("-h" or "--help"))
                        {
                            MultiLuaExpression(rest);
                            continue;
                        }
                    }
                    s_rootCommand.Invoke(line, timingConsole);
                }
                catch (Exception ex) when (!Debugger.IsAttached)
                {
                    s_logger.LogError("Unexpected exception: {1}\n{2}", ex.Message, ex.StackTrace!);
                }
            }
        }

        #region Settings

        public enum Setting
        {
            PrintCurrentDir,
            PrintOutputPrefixed,
        }

        public static void Set(Setting setting, string value)
        {
            switch (setting)
            {
                case Setting.PrintCurrentDir:
                    s_printCurrentDir = parseBool(value);
                    break;
                case Setting.PrintOutputPrefixed:
                    s_printOutputPrefixed = parseBool(value);
                    break;
            }

            static bool parseBool(string input)
            {
                return input.ToLowerInvariant() switch
                {
                    "yes" or "true" or "on" => true,
                    "no" or "false" or "off" => false,
                    _ => throw new Exception("Invalid boolean value '{0}' accepted values are: yes, true, on, no, false or off"),
                };
            }
        }

        #endregion Settings

        public static void Quit() => s_shouldRun = false;

        #region Current Directory Management

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

        public enum LuaSyntaxOptionsPreset
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
        private static LuaParseOptions PresetEnumToPresetOptions(LuaSyntaxOptionsPreset preset)
        {
            return new LuaParseOptions(preset switch
            {
                LuaSyntaxOptionsPreset.Lua51 => LuaSyntaxOptions.Lua51,
                LuaSyntaxOptionsPreset.Lua52 => LuaSyntaxOptions.Lua52,
                LuaSyntaxOptionsPreset.Lua53 => LuaSyntaxOptions.Lua53,
                LuaSyntaxOptionsPreset.LuaJIT20 => LuaSyntaxOptions.LuaJIT20,
                LuaSyntaxOptionsPreset.LuaJIT21 => LuaSyntaxOptions.LuaJIT21,
                LuaSyntaxOptionsPreset.GMod => LuaSyntaxOptions.GMod,
                LuaSyntaxOptionsPreset.Roblox => LuaSyntaxOptions.Roblox,
                LuaSyntaxOptionsPreset.All => LuaSyntaxOptions.All,
                _ => throw new InvalidOperationException(),
            });
        }

        public enum ASTVisitors
        {
        }

        public static void Lex(LuaSyntaxOptionsPreset preset, string path, bool printTokens = false)
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
                OutputWriter.WriteLine(TreeDumper.DumpCompact(rootNode));
            }
        }

        public static void Parse(LuaSyntaxOptionsPreset preset, string path, bool constantFold = false, bool printTree = false)
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
                OutputWriter.WriteLine(TreeDumper.DumpCompact(LuaTreeDumperConverter.Convert(rootNode)));
            }
            else
            {
                rootNode.WriteTo(OutputWriter);
                OutputWriter.WriteLine("");
            }

            var script = new Script(ImmutableArray.Create<SyntaxTree>(syntaxTree));
            var global = script.RootScope;
            s_logger.WriteLine("Global variables:");
            foreach (var variable in global.DeclaredVariables)
                s_logger.WriteLine($"    {variable.Kind} {variable.Name}");
        }

        public static void ParseExpression(string input)
        {
            var options = LuaParseOptions.Default;
            string? code = null;
            if (input.IndexOf(' ') != -1)
            {
                var presetName = input[..input.IndexOf(' ')];
                if (Enum.TryParse<LuaSyntaxOptionsPreset>(presetName, true, out var presetEnum))
                {
                    code = input[(input.IndexOf(' ') + 1)..];
                    options = PresetEnumToPresetOptions(presetEnum);
                }
            }
            code ??= input;

            var text = SourceText.From(code, Console.InputEncoding);

            var expr = SyntaxFactory.ParseExpression(text, options);
            var diagnostics = expr.GetDiagnostics();
            foreach (var diagnostic in diagnostics)
                s_logger.WriteLine(diagnostic.ToString());

            expr = (ExpressionSyntax) expr.ConstantFold();
            expr = expr.NormalizeWhitespace();
            expr.WriteTo(OutputWriter);
            OutputWriter.WriteLine("");
        }

        public static void MassParse(LuaSyntaxOptionsPreset preset, params string[] patterns)
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
                _ => throw new InvalidOperationException("Invalid naming strategy."),
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
                _ => throw new InvalidOperationException("Invalid slot allocator."),
            };
        }

        public static void Minify(
            string path,
            LuaSyntaxOptionsPreset preset = LuaSyntaxOptionsPreset.All,
            NamingStrategy namingStrategy = NamingStrategy.Numerical,
            SlotAllocator slotAllocator = SlotAllocator.Sorted,
            bool format = false)
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
            if (format)
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
            root.WriteTo(OutputWriter);
            OutputWriter.WriteLine("");
        }

        #endregion Loretta

        #region Multi-Lua

        public static void MultiLua(string scriptPath) => RunMultiLua(scriptPath);

        public static void MultiLuaExpression(string expression)
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, expression);
                RunMultiLua(path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static void RunMultiLua(params string[] args)
        {
            const string prefixTemplate = "[00:00:00.000000]";

            var versions = Directory.GetDirectories("binaries");
            Array.Sort(versions);

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
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    },
                    EnableRaisingEvents = true
                };
                foreach (var arg in args)
                    proc.StartInfo.ArgumentList.Add(arg);
#pragma warning disable CA1416 // Validate platform compatibility
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    proc.StartInfo.LoadUserProfile = true;
#pragma warning restore CA1416 // Validate platform compatibility
                proc.OutputDataReceived += (_, e) =>
                {
                    if (e.Data is not null)
                        s_logger.WriteLine(e.Data);
                };
                proc.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data is not null)
                        s_logger.LogError(e.Data);
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

            static string getFormattedLuaName(string name) =>
                name.Replace("_", " ");
        }

        #endregion Multi-Lua

        public static void Clear() =>
            Console.Clear();

        #region Memory Usage

        private static readonly Process s_currentProc = Process.GetCurrentProcess();
        private static readonly Stack<(long gcMemory, long processMemory)> s_memoryStack = new();

        public static void PrintMemoryUsage()
        {
            var gcmem = GC.GetTotalMemory(false);
            var procmem = s_currentProc.PrivateMemorySize64;
            s_logger.WriteLine($"Memory usage according to GC:       {FileSize.Format(gcmem)}");
            s_logger.WriteLine($"Memory usage according to Process:  {FileSize.Format(procmem)}");
        }

        public static void PushMemoryUsage()
        {
            var gcmem = GC.GetTotalMemory(false);
            var procmem = s_currentProc.PrivateMemorySize64;
            s_logger.WriteLine($"Memory usage according to GC:       {FileSize.Format(gcmem)}");
            s_logger.WriteLine($"Memory usage according to Process:  {FileSize.Format(procmem)}");
            s_memoryStack.Push((gcmem, procmem));
            s_logger.WriteLine("Memory usage pushed to stack.");
        }

        // [Command("mcomp"), Command("memory-compare")]
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

        // [Command("gc")]
        // [HelpDescription("Invokes the garbage collector")]
        public static void InvokeGC(int amount = 1000)
        {
            for (var idx = 0; idx < amount; idx++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                GC.WaitForPendingFinalizers();
            }
        }

        #endregion Memory Usage

        static Program()
        {
            var lsopOption = new Option<LuaSyntaxOptionsPreset>(
                new[] { "-p", "--preset" },
                () => LuaSyntaxOptionsPreset.All,
                "The preset to use when processing the provided input.");

            var setPrintCurrentDirCommand = new Command("@cd", "Enable or disable printing the current directory.")
            {
                new Argument<string>("value", "'on' or 'off'")
            };

            var setCommand = new Command("s", "Set a setting.")
            {
                new Argument<Setting>("setting", "The setting to set"),
                new Argument<string>("value", "The value to set the setting to")
            };
            setCommand.Handler = CommandHandler.Create(Set);
            setCommand.AddAlias("set");

            var quitCommand = new Command("q", "Quit the program.")
            {
                Handler = CommandHandler.Create(Quit)
            };
            quitCommand.AddAlias("quit");
            quitCommand.AddAlias("exit");

            var changeDirectoryCommand = new Command("cd", "Changes the current directory.")
            {
                new Argument<string>("relativePath", "The path relatie to the current one to move to.")
            };
            changeDirectoryCommand.Handler = CommandHandler.Create(ChangeDirectory);

            var listSymbolsCommand = new Command("ls", "List the current directory's symbols.")
            {
                Handler = CommandHandler.Create(ListSymbols)
            };

            var lexCommand = new Command("l", "Lexes the provided file.")
            {
                lsopOption,
                new Argument<string>("path", "The path of the file to lex."),
                new Option<bool>(
                    new[] { "-p", "--print-tokens" },
                    () => false,
                    "Whether to print the lexed tokens.")
            };
            lexCommand.Handler = CommandHandler.Create(Lex);
            lexCommand.AddAlias("lex");

            var parseCommand = new Command("p", "Parses the provided file.")
            {
                lsopOption,
                new Argument<string>("path", "The path of the file to parse."),
                new Option<bool>(
                    new[] { "-c", "--constant-fold" },
                    () => false,
                    "Whether to constant-fold the parsed tree."),
                new Option<bool>(
                    new[] { "-t", "--print-tree" },
                    () => false,
                    "Whether to print the parsed tree as a tree instead of back as code.")
            };
            parseCommand.Handler = CommandHandler.Create(Parse);
            parseCommand.AddAlias("parse");

            var parseExpressionCommand = new Command("e", "Parses the provided expression.")
            {
                new Argument<LuaSyntaxOptionsPreset>(
                    "preset",
                    () => LuaSyntaxOptionsPreset.All,
                    "The preset to use when parsing."),
                new Argument<string>("input", "The provided expression to parse.")
            };
            parseExpressionCommand.Handler = CommandHandler.Create(ParseExpression);
            parseExpressionCommand.AddAlias("expr");
            parseExpressionCommand.AddAlias("expression");

            var massParseCommand = new Command("mp", "Parses files en masse by finding them with the provided patterns.")
            {
                lsopOption,
                new Argument<string[]>("patterns", "The patterns to use when searching for files.")
            };
            massParseCommand.Handler = CommandHandler.Create(MassParse);
            massParseCommand.AddAlias("mass-parse");

            var minifyCommand = new Command("min", "Minifies the provided file.")
            {
                new Argument<string>("path", "The path of the file to minify."),
                lsopOption,
                new Option<NamingStrategy>(
                    new[] { "-n", "--naming", "--naming-strategy" },
                    () => NamingStrategy.Numerical,
                    "The naming strategy to use when renaming variables."),
                new Option<SlotAllocator>(
                    new[] { "-a", "--allocator", "--slot-allocator" },
                    () => SlotAllocator.Sorted,
                    "The slot allocator to use when allocating slots for variables."),
                new Option<bool>(
                    new[] { "-f", "--format" },
                    () => false,
                    "Whether to format the output."),
            };
            minifyCommand.Handler = CommandHandler.Create(Minify);
            minifyCommand.AddAlias("minify");

            var multiLuaCommand = new Command("mlua", "Executes a file in multiple lua distributions.")
            {
                new Argument<string>("scriptPath", "The path of the script to execute.")
            };
            multiLuaCommand.Handler = CommandHandler.Create(MultiLua);
            multiLuaCommand.AddAlias("multi-lua");
            multiLuaCommand.AddAlias("multilua");

            var multiLuaExpressionCommand = new Command("emlua", "Executes an expression in multiple lua distributions.")
            {
                new Argument<string>("expression", "The expression to execute.")
            };
            multiLuaExpressionCommand.Handler = CommandHandler.Create(MultiLuaExpression);
            multiLuaExpressionCommand.AddAlias("expr-multi-lua");
            multiLuaExpressionCommand.AddAlias("exprmultilua");

            var clearCommand = new Command("clear", "Clears the console screen.")
            {
                Handler = CommandHandler.Create(Clear)
            };
            clearCommand.AddAlias("cls");

            var pushMemoryUsageCommand = new Command("push", "Pushes the current memory usage to the stack.")
            {
                Handler = CommandHandler.Create(PushMemoryUsage)
            };

            var popMemoryUsageCommand = new Command("pop", "Pops and compares the memory usage from the stack.")
            {
                Handler = CommandHandler.Create(PopMemoryUsage)
            };

            var compareMemoryUsage = new Command("comp", "Compares the current memory usage to the most recent one in the stack.")
            {
                Handler = CommandHandler.Create(CompareMemoryUsage)
            };
            compareMemoryUsage.AddAlias("compare");

            var printMemoryUsageCommand = new Command("m", "Prints the current memory usage.")
            {
                pushMemoryUsageCommand,
                popMemoryUsageCommand,
                compareMemoryUsage,
            };
            printMemoryUsageCommand.Handler = CommandHandler.Create(PrintMemoryUsage);
            printMemoryUsageCommand.AddAlias("mem");

            var invokeGcCommand = new Command("gc", "Aggressively invokes the GC.")
            {
                new Argument<int>("amount", () => 1000, "The amount of times to invoke the GC.")
            };
            invokeGcCommand.Handler = CommandHandler.Create(InvokeGC);

            s_rootCommand = new RootCommand()
            {
                setPrintCurrentDirCommand,
                setCommand,
                quitCommand,
                changeDirectoryCommand,
                listSymbolsCommand,
                lexCommand,
                parseCommand,
                parseExpressionCommand,
                massParseCommand,
                minifyCommand,
                multiLuaCommand,
                multiLuaExpressionCommand,
                clearCommand,
                printMemoryUsageCommand,
                invokeGcCommand,
            };
        }
    }
}
