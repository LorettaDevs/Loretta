using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GParse.Collections;
using GParse.Lexing;
using GUtils.CLI.Commands;
using GUtils.CLI.Commands.Errors;
using GUtils.IO;
using GUtils.Timing;
using Loretta.Lexing;
using Loretta.Parsing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;

namespace Loretta.CLI
{
    public static class Program
    {
        private static Boolean ShouldRun;
        private static ConsoleTimingLogger Logger;

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
                    Logger.LogError ( "Error while executing '{0}': {1}\n{2}", ex.Command, ex.Message, ex.StackTrace );
                }
            }
        }

        [Command ( "q" ), Command ( "quit" ), Command ( "exit" )]
        public static void Quit ( ) => ShouldRun = false;

        #region Current Directory Management

        [Command ( "cd" )]
        public static void ChangeDirectory ( String relativePath ) =>
            Environment.CurrentDirectory = Path.GetFullPath ( Path.Combine ( Environment.CurrentDirectory, relativePath ) );

        [Command ( "ls" )]
        public static void ListSymbols ( )
        {
            var di = new DirectoryInfo ( Environment.CurrentDirectory );
            foreach ( DirectoryInfo directoryInfo in di.EnumerateDirectories ( ) )
                Logger.WriteLine ( $"./{directoryInfo.Name}/" );

            foreach ( FileInfo fileInfo in di.EnumerateFiles ( ) )
                Logger.WriteLine ( $"./{fileInfo.Name}" );
        }

        #endregion Current Directory Management

        [Command ( "p" ), Command ( "parse" )]
        public static void Parse ( String path )
        {
            if ( !File.Exists ( path ) )
            {
                Logger.LogError ( "Provided path does not exist." );
                return;
            }

            var sw = Stopwatch.StartNew ( );
            var dl = new DiagnosticList ( );
            var lb = new LuaLexerBuilder ( );
            var pb = new LuaParserBuilder ( );
            var fw = new FormattedLuaCodeSerializer ( "    " );
            ILexer<LuaTokenType> l = lb.CreateLexer ( File.ReadAllText ( path ), dl );
            LuaParser p = pb.CreateParser ( new TokenReader<LuaTokenType> ( l ), dl );
            StatementList t = p.Parse ( );
            fw.VisitStatementList ( t );
            sw.Stop ( );
            var time = Duration.Format ( sw.ElapsedTicks );
            Logger.WriteLine ( fw.ToString ( ) );
            Logger.WriteLine ( $"Parsed and compiled back to code in {time}." );
        }

        #region Memory Usage

        private static readonly Process CurrentProc = Process.GetCurrentProcess ( );
        private static readonly Stack<(Int64 gcMemory, Int64 processMemory)> MemoryStack = new Stack<(Int64 gcMemory, Int64 processMemory)> ( );

        [Command ( "m" ), Command ( "mem" )]
        public static void PrintMemoryUsage ( )
        {
            var gcmem = GC.GetTotalMemory ( false );
            var procmem = CurrentProc.PrivateMemorySize64;
            Logger.WriteLine ( $"Memory usage according to GC:       {FileSize.Format ( gcmem )}" );
            Logger.WriteLine ( $"Memory usage according to Process:  {FileSize.Format ( procmem )}" );
        }

        [Command ( "mpush" ), Command ( "memory-push" )]
        public static void PushMemoryUsage ( )
        {
            var gcmem = GC.GetTotalMemory ( false );
            var procmem = CurrentProc.PrivateMemorySize64;
            Logger.WriteLine ( $"Memory usage according to GC:       {FileSize.Format ( gcmem )}" );
            Logger.WriteLine ( $"Memory usage according to Process:  {FileSize.Format ( procmem )}" );
            MemoryStack.Push ( (gcmem, procmem) );
            Logger.WriteLine ( "Memory usage pushed to stack." );
        }

        [Command ( "mcomp" ), Command ( "memory-compare" )]
        public static void CompareMemoryUsage ( )
        {
            var currgcmem = GC.GetTotalMemory ( false );
            var currprocmem = CurrentProc.PrivateMemorySize64;
            Logger.WriteLine ( $"Memory usage according to GC:       {FileSize.Format ( currgcmem )}" );
            Logger.WriteLine ( $"Memory usage according to Process:  {FileSize.Format ( currprocmem )}" );

            if ( MemoryStack.Count == 0 )
            {
                Logger.LogError ( "Nothing on memory stack to compare to." );
                return;
            }

            (var oldgcmem, var oldprocmem) = MemoryStack.Peek ( );
            (var Δgcmem, var Δprocmem) = (currgcmem - oldgcmem, currprocmem - oldprocmem);
            Logger.WriteLine ( $"ΔMemory usage according to GC:      {( Δgcmem < 0 ? $"-{FileSize.Format ( -Δgcmem )}" : FileSize.Format ( Δgcmem ) )}" );
            Logger.WriteLine ( $"ΔMemory usage according to Process: {( Δprocmem < 0 ? $"-{FileSize.Format ( -Δprocmem )}" : FileSize.Format ( Δgcmem ) )}" );
        }

        [Command ( "mpop" ), Command ( "memory-pop" )]
        public static void PopMemoryUsage ( )
        {
            if ( MemoryStack.Count == 0 )
            {
                Logger.LogError ( "Nothing on memory stack to pop." );
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