using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Syntax;
using Loretta.CodeAnalysis.Text;
using Loretta.IO;
using Loretta.Utilities;
using Tsu.CLI.Commands;
using Tsu.CLI.Commands.Errors;
using Tsu.Numerics;
using Tsu.Timing;

namespace Loretta.CLI
{
    public static class Program
    {
        private static Boolean ShouldRun;
        private static ConsoleTimingLogger? Logger;

        public static void Main ( )
        {
            var commandManager = new ConsoleCommandManager ( );
            commandManager.LoadCommands ( typeof ( Program ), null );
            commandManager.AddHelpCommand ( );

            Logger = new ConsoleTimingLogger ( );

            ShouldRun = true;
            while ( ShouldRun )
            {
                try
                {
                    Logger.Write ( $"{Environment.CurrentDirectory}> " );
                    commandManager.Execute ( Logger.ReadLine ( ) );
                }
                catch ( NonExistentCommandException ex )
                {
                    Logger.LogError ( "Unexistent command '{0}'. Use the 'help' command to list all commands.", ex.Command );
                }
                catch ( CommandInvocationException ex )
                {
                    Logger.LogError ( "Error while executing '{0}': {1}\n{2}", ex.Command, ex.Message, ex.StackTrace! );
                }
            }
        }

        [Command ( "q" ), Command ( "quit" ), Command ( "exit" )]
        public static void Quit ( ) => ShouldRun = false;

        #region Current Directory Management

        [Command ( "cd" )]
        public static void ChangeDirectory ( String relativePath )
        {
            try
            {
                Environment.CurrentDirectory = Path.GetFullPath ( Path.Combine ( Environment.CurrentDirectory, relativePath ) );
            }
            catch ( Exception ex )
            {
                Logger!.LogError ( "Error while changing directory: {0}", ex );
            }
        }

        [Command ( "ls" )]
        public static void ListSymbols ( )
        {
            var di = new DirectoryInfo ( Environment.CurrentDirectory );
            foreach ( DirectoryInfo directoryInfo in di.EnumerateDirectories ( ) )
                Logger!.WriteLine ( $"./{directoryInfo.Name}/" );

            foreach ( FileInfo fileInfo in di.EnumerateFiles ( ) )
                Logger!.WriteLine ( $"./{fileInfo.Name}" );
        }

        #endregion Current Directory Management

        public enum LuaOptionsPreset
        {
            Lua51,
            Lua52,
            LuaJIT,
            GMod,
            Roblox,
            All,
        }

        private static LuaOptions PresetEnumToPresetOptions ( LuaOptionsPreset preset )
        {
            return preset switch
            {
                LuaOptionsPreset.Lua51 => LuaOptions.Lua51,
                LuaOptionsPreset.Lua52 => LuaOptions.Lua52,
                LuaOptionsPreset.LuaJIT => LuaOptions.LuaJIT,
                LuaOptionsPreset.GMod => LuaOptions.GMod,
                LuaOptionsPreset.Roblox => LuaOptions.Roblox,
                LuaOptionsPreset.All => LuaOptions.All,
                _ => throw new InvalidOperationException ( ),
            };
        }

        public enum ASTVisitors
        {
            ConstantFolder,
            RawStringRewriter,
            UselessAssignmentRemover,
            SimpleInliner,
            AllTillNothingMoreToDo
        }

        [Command ( "l" ), Command ( "lex" )]
        public static void Lex ( LuaOptionsPreset preset, String path, Boolean printTokens = false )
        {
            if ( !File.Exists ( path ) )
            {
                Logger!.LogError ( "Provided path does not exist." );
                return;
            }

            LuaOptions options = PresetEnumToPresetOptions ( preset );
            var text = File.ReadAllText ( path );
            var sourceText = SourceText.From ( text, path );
            ImmutableArray<SyntaxToken> tokens;
            ImmutableArray<Diagnostic> diagnostics;
            using ( Logger!.BeginOperation ( "Lexing" ) )
                tokens = SyntaxTree.ParseTokens ( options, sourceText, out diagnostics, includeEndOfFile: true );

            Logger!.LogInformation ( $"{tokens.Length} tokens lexed." );
            var tw = new ConsoleTimingLoggerTextWriter ( Logger );
            if ( printTokens )
            {
                foreach ( SyntaxToken token in tokens )
                {
                    token.PrettyPrintTo ( tw );
                }
            }

            tw.WriteDiagnostics ( diagnostics );
        }

        [Command ( "p" ), Command ( "parse" )]
        public static void Parse ( LuaOptionsPreset preset, String path, params ASTVisitors[] visitors )
        {
            if ( !File.Exists ( path ) )
            {
                Logger!.LogError ( "Provided path does not exist." );
                return;
            }

            LuaOptions options = PresetEnumToPresetOptions ( preset );
            var text = File.ReadAllText ( path );
            var sourceText = SourceText.From ( text, path );
            SyntaxTree syntaxTree;
            using ( Logger!.BeginOperation ( "Parsing" ) )
                syntaxTree = SyntaxTree.Parse ( options, sourceText );

            var tw = new ConsoleTimingLoggerTextWriter ( Logger );
            tw.WriteDiagnostics ( syntaxTree.Diagnostics );
            Logger.Write ( "Press any key to continue..." );
            Console.ReadKey ( true );
            Logger.WriteLine ( "" );
            syntaxTree.Root.PrettyPrintTo ( tw );
        }

        [Command ( "mp" ), Command ( "mass-parse" )]
        public static void MassParse ( LuaOptionsPreset preset, params String[] patterns )
        {
            var files = patterns.SelectMany ( pattern => Directory.EnumerateFiles ( ".", pattern, new EnumerationOptions
            {
                IgnoreInaccessible = true,
                MatchType = MatchType.Simple
            } ) )
                .ToArray ( );

            LuaOptions options = PresetEnumToPresetOptions ( preset );
            foreach ( var file in files )
            {

            }
        }

        #region Memory Usage

        private static readonly Process CurrentProc = Process.GetCurrentProcess ( );
        private static readonly Stack<(Int64 gcMemory, Int64 processMemory)> MemoryStack = new Stack<(Int64 gcMemory, Int64 processMemory)> ( );

        [Command ( "m" ), Command ( "mem" )]
        public static void PrintMemoryUsage ( )
        {
            var gcmem = GC.GetTotalMemory ( false );
            var procmem = CurrentProc.PrivateMemorySize64;
            Logger!.WriteLine ( $"Memory usage according to GC:       {FileSize.Format ( gcmem )}" );
            Logger.WriteLine ( $"Memory usage according to Process:  {FileSize.Format ( procmem )}" );
        }

        [Command ( "mpush" ), Command ( "memory-push" )]
        public static void PushMemoryUsage ( )
        {
            var gcmem = GC.GetTotalMemory ( false );
            var procmem = CurrentProc.PrivateMemorySize64;
            Logger!.WriteLine ( $"Memory usage according to GC:       {FileSize.Format ( gcmem )}" );
            Logger.WriteLine ( $"Memory usage according to Process:  {FileSize.Format ( procmem )}" );
            MemoryStack.Push ( (gcmem, procmem) );
            Logger.WriteLine ( "Memory usage pushed to stack." );
        }

        [Command ( "mcomp" ), Command ( "memory-compare" )]
        public static void CompareMemoryUsage ( )
        {
            var currgcmem = GC.GetTotalMemory ( false );
            var currprocmem = CurrentProc.PrivateMemorySize64;
            Logger!.WriteLine ( $"Memory usage according to GC:       {FileSize.Format ( currgcmem )}" );
            Logger.WriteLine ( $"Memory usage according to Process:  {FileSize.Format ( currprocmem )}" );

            if ( MemoryStack.Count == 0 )
            {
                Logger.LogError ( "Nothing on memory stack to compare to." );
                return;
            }

            (var oldgcmem, var oldprocmem) = MemoryStack.Peek ( );
            (var Δgcmem, var Δprocmem) = (currgcmem - oldgcmem, currprocmem - oldprocmem);
            Logger.WriteLine ( $"ΔMemory usage according to GC:      {( Δgcmem < 0 ? $"-{FileSize.Format ( -Δgcmem )}" : FileSize.Format ( Δgcmem ) )}" );
            Logger.WriteLine ( $"ΔMemory usage according to Process: {( Δprocmem < 0 ? $"-{FileSize.Format ( -Δprocmem )}" : FileSize.Format ( Δprocmem ) )}" );
        }

        [Command ( "mpop" ), Command ( "memory-pop" )]
        public static void PopMemoryUsage ( )
        {
            if ( MemoryStack.Count == 0 )
            {
                Logger!.LogError ( "Nothing on memory stack to pop." );
                return;
            }

            CompareMemoryUsage ( );
            MemoryStack.Pop ( );
        }

        [Command ( "gc" )]
        [HelpDescription ( "Invokes the garbage collector" )]
        public static void InvokeGC ( ) => GC.Collect ( );

        #endregion Memory Usage
    }
}