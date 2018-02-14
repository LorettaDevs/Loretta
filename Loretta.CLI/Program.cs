using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GParse.Lexing;
using GParse.Lexing.Errors;
using Loretta.CLI.Commands;
using Loretta.Lexing;
using Loretta.Utils;

namespace Loretta.CLI
{
    internal class Program
    {
        private static readonly CommandManager commandManager = new CommandManager ( );

        public static Boolean ShouldRun { get; private set; } = true;

        private static void Main ( )
        {
            commandManager.LoadCommands ( typeof ( Program ), instance: null );
            while ( ShouldRun )
            {
                var path = Environment.CurrentDirectory.Replace ( '\\', '/' );
                path = path.Substring ( path.IndexOf ( "Loretta" ) );
                Console.Write ( $"{path}>" );
                var line = Console.ReadLine ( ).Trim ( );
                if ( line == "exit" )
                    break;

                try
                {
                    commandManager.Execute ( line );
                }
                catch ( LexException e )
                {
                    Error ( $"{e.Location} {e.Message}" );
                }
                catch ( Exception e )
                {
                    Error ( e.ToString ( ) );
                }
            }
        }

        [Command ( "cwd" )]
        public static void CurrentWorkingDir ( )
        {
            Console.WriteLine ( Environment.CurrentDirectory );
        }

        [Command ( "cd" )]
        public static void ChangeDirectory ( [CommandArgumentRest] String path )
        {
            if ( !Directory.Exists ( path ) )
                Error ( "Directory doesn't exists." );
            Environment.CurrentDirectory = path;
        }

        [Command ( "ls" )]
        public static void ListStuff ( )
        {
            var cwd = new DirectoryInfo ( "." );
            foreach ( DirectoryInfo di in cwd.EnumerateDirectories ( ) )
                Console.WriteLine ( $"{di.Name}/" );
            foreach ( FileInfo fi in cwd.EnumerateFiles ( ) )
                Console.WriteLine ( fi.Name );
        }

        [Command ( "head" )]
        public static void Head ( String filePath, Int32 bytes )
        {
            if ( !File.Exists ( filePath ) )
                throw new Exception ( "File doesn't exists." );

            using ( FileStream handle = File.OpenRead ( filePath ) )
            {
                String data = CInterop.ReadToEndAsASCII ( handle );
                Console.WriteLine ( String.Join ( " ", data.Take ( bytes ).Select ( ch => ch + $"({( ( Int32 ) ch ).ToString ( "X2" )})" ) ) );
            }
        }

        [Command ( "lex" )]
        public static void LexFile ( String filePath, String outputPath = "stdin" )
        {
            if ( !File.Exists ( filePath ) )
                throw new Exception ( "File doesn't exists." );

            StreamWriter outputWriter;
            using ( outputWriter = outputPath == "stdin"
                ? new StreamWriter ( Console.OpenStandardOutput ( ), Encoding.UTF8, 4096 )
                : new StreamWriter ( outputPath, false, Encoding.UTF8, 4096 ) )
            using ( FileStream handle = File.OpenRead ( filePath ) )
            {
                var max = new SourceLocation ( Int32.MaxValue, Int32.MaxValue, Int32.MaxValue );
                var last = new SourceRange ( max, max );

                var sw = Stopwatch.StartNew ( );
                var lexer = new GLuaLexer ( handle );
                foreach ( LToken token in lexer.Lex ( ) )
                {
                    outputWriter.WriteLine ( $"LToken ( '{token.ID}', '{token.Raw}', '{token.Value}', {token.Type}, {token.Range} )" );
                    foreach ( Token flair in token.LeadingFlair )
                        outputWriter.WriteLine ( $"\tLTokenFlair ( '{flair.ID}', '{flair.Raw}', '{flair.Value}', {flair.Type}, {flair.Range} )" );

                    if ( last == token.Range )
                        throw new Exception ( "Possible infinite loop. Token has same range as last." );
                    last = token.Range;
                }
                sw.Stop ( );
                outputWriter.WriteLine ( $"Time elapsed on lexing: {HumanTime ( sw )}" );
            }
        }

        private const Double TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000D;

        public static String HumanTime ( Stopwatch sw )
        {
            return sw.ElapsedTicks > TimeSpan.TicksPerMinute
                ? $"{sw.Elapsed.TotalMinutes:#00.00}m"
                : sw.ElapsedTicks > TimeSpan.TicksPerSecond
                    ? $"{sw.Elapsed.TotalSeconds:#00.00}s"
                    : sw.ElapsedTicks > TimeSpan.TicksPerMillisecond
                        ? $"{sw.Elapsed.TotalMilliseconds:#00.00}ms"
                        : $"{sw.ElapsedTicks / TicksPerMicrosecond:#00.00}μs";
        }

        private static void Error ( String v )
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine ( v );
            Console.ResetColor ( );
        }
    }
}
