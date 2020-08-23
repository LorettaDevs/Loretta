using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GParse;
using GParse.Collections;
using GParse.Errors;
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

        public enum ASTVisitor
        {
            ConstantFolder,
            RawStringRewriter,
            UselessAssignmentRemover,
            SimpleInliner,
            AllTillNothingMoreToDo
        }

        [Command ( "p" ), Command ( "parse" )]
        public static void Parse ( String path, params ASTVisitor[] visitors )
        {
            if ( !File.Exists ( path ) )
            {
                Logger.LogError ( "Provided path does not exist." );
                return;
            }

            var stopwatch = Stopwatch.StartNew ( );
            var diagnosticList = new DiagnosticList ( );
            var lexerBuilder = new LuaLexerBuilder ( );
            var parserBuilder = new LuaParserBuilder ( );
            var formattedCodeSerializer = new FormattedLuaCodeSerializer ( "    " );
            var code = File.ReadAllText ( path );
            ILexer<LuaTokenType> lexer = lexerBuilder.CreateLexer ( code, diagnosticList );
            LuaParser parser = parserBuilder.CreateParser ( new TokenReader<LuaTokenType> ( lexer ), diagnosticList );
            StatementList statementList = parser.Parse ( );
            foreach ( ASTVisitor visitor in visitors )
            {
                switch ( visitor )
                {
                    case ASTVisitor.ConstantFolder:
                    {
                        statementList = constantFolder ( statementList );
                        break;
                    }

                    case ASTVisitor.RawStringRewriter:
                    {
                        statementList = rawStringRewriter ( statementList );
                        break;
                    }

                    case ASTVisitor.UselessAssignmentRemover:
                    {
                        statementList = uselessAssignmentRemover ( statementList );
                        break;
                    }

                    case ASTVisitor.SimpleInliner:
                    {
                        statementList = simpleInliner ( statementList );
                        break;
                    }

                    case ASTVisitor.AllTillNothingMoreToDo:
                    {
                        StatementList original;
                        var rounds = 0;
                        do
                        {
                            original = statementList;
                            statementList = simpleInliner ( uselessAssignmentRemover ( rawStringRewriter ( constantFolder ( statementList ) ) ) );
                        }
                        while ( original != statementList && rounds++ < 120 );
                        break;
                    }
                }
            }
            formattedCodeSerializer.VisitNode ( statementList );
            stopwatch.Stop ( );
            var time = Duration.Format ( stopwatch.ElapsedTicks );
            Logger.WriteLine ( formattedCodeSerializer.ToString ( ) );
            Logger.WriteLine ( $"Parsed and compiled back to code in {time}." );
            foreach ( Diagnostic diagnostic in diagnosticList.OrderBy ( d => d.Severity ) )
            {
                Logger.WriteLine ( $@"{diagnostic.Id} {diagnostic.Severity}: {diagnostic.Description}
{LuaDiagnostics.HighlightRange ( code, diagnostic.Range )}" );
            }

            static StatementList constantFolder ( StatementList statementList )
            {
                statementList = new ConstantFolder ( ).VisitNode ( statementList ) as StatementList;
                return statementList;
            }

            static StatementList rawStringRewriter ( StatementList statementList )
            {
                statementList = new RawStringRewriter ( ).VisitNode ( statementList ) as StatementList;
                return statementList;
            }

            static StatementList uselessAssignmentRemover ( StatementList statementList )
            {
                var tracker = new SimpleReadWriteTracker ( );
                tracker.VisitNode ( statementList );
                statementList = new UselessAssignmentRemover ( tracker ).VisitNode ( statementList ) as StatementList;
                return statementList;
            }

            static StatementList simpleInliner ( StatementList statementList )
            {
                var tracker = new SimpleReadWriteTracker ( );
                tracker.VisitNode ( statementList );
                statementList = new SimpleInliner ( tracker ).VisitNode ( statementList ) as StatementList;
                return statementList;
            }
        }

        [Command ( "mp" ), Command ( "mass-parse" )]
        public static void MassParse ( params String[] patterns )
        {
            var files = patterns.SelectMany ( pattern => Directory.EnumerateFiles ( ".", pattern, new EnumerationOptions
            {
                IgnoreInaccessible = true,
                MatchType = MatchType.Simple
            } ) )
                .ToArray ( );

            var lexerBuilder = new LuaLexerBuilder ( );
            var parserBuilder = new LuaParserBuilder ( );
            foreach ( var file in files )
            {
                var stopwatch = Stopwatch.StartNew ( );
                var diagnosticList = new DiagnosticList ( );
                var formattedCodeSerializer = new FormattedLuaCodeSerializer ( "    " );
                var code = File.ReadAllText ( file );
                try
                {
                    ILexer<LuaTokenType> lexer = lexerBuilder.CreateLexer ( code, diagnosticList );
                    LuaParser parser = parserBuilder.CreateParser ( new TokenReader<LuaTokenType> ( lexer ), diagnosticList );
                    StatementList statementList = parser.Parse ( );
                    formattedCodeSerializer.VisitNode ( statementList );
                    stopwatch.Stop ( );
                    var time = Duration.Format ( stopwatch.ElapsedTicks );
                    Logger.WriteLine ( $"{file}: {time}." );
                    foreach ( Diagnostic diagnostic in diagnosticList.OrderBy ( d => d.Severity ) )
                    {
                        Logger.WriteLine ( $@"{diagnostic.Id} {diagnostic.Severity}: {diagnostic.Description}
{LuaDiagnostics.HighlightRange ( code, diagnostic.Range )}" );
                    }
                }
                catch ( FatalParsingException fpex )
                {
                    Logger.WriteLine ( $"{file}:" );
                    Logger.LogError ( $"{typeof ( FatalParsingException ).FullName}: {fpex.Message}" );
                    Logger.LogError ( LuaDiagnostics.HighlightRange ( code, fpex.Range ) );
                    Logger.LogError ( fpex.StackTrace );
                }
                catch ( Exception ex )
                {
                    Logger.WriteLine ( $"{file}:" );
                    Logger.LogError ( ex.ToString ( ) );
                }
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