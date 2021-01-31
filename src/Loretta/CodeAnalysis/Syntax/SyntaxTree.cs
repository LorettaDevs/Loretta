using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The container for all data related to a syntax node tree.
    /// </summary>
    public sealed class SyntaxTree
    {
        #region Factory Methods

        #region Parse

        private static void Parse ( SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics )
        {
            var parser = new Parser ( syntaxTree );
            root = parser.ParseCompilationUnit ( );
            diagnostics = parser.Diagnostics.ToImmutableArray ( );
        }

        /// <summary>
        /// Parses the provided text.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static SyntaxTree Parse ( LuaOptions options, SourceText text ) =>
            new ( options, text, Parse );

        /// <summary>
        /// Parses the provided text.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static SyntaxTree Parse ( LuaOptions options, String text )
        {
            var sourceText = SourceText.From ( text );
            return Parse ( options, sourceText );
        }

        #endregion Parse

        /// <summary>
        /// Loads and parses a syntax tree from the provided file path.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static SyntaxTree Load ( LuaOptions options, String filePath )
        {
            var text = File.ReadAllText ( filePath );
            var sourceText = SourceText.From ( text, filePath );
            return Parse ( options, sourceText );
        }

        #region ParseExpression

        /// <summary>
        /// Parses an expression from the provided text.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        public static ExpressionSyntax ParseExpression ( LuaOptions options, SourceText text, out ImmutableArray<Diagnostic> diagnostics )
        {
            ExpressionSyntax? expression = null;
            void ParseExpression ( SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics )
            {
                var parser = new Parser ( syntaxTree );

                expression = parser.ParseExpression ( );

                SyntaxToken? eofToken = parser.Match ( SyntaxKind.EndOfFileToken );
                root = new CompilationUnitSyntax ( ImmutableArray<StatementSyntax>.Empty, eofToken );
                diagnostics = parser.Diagnostics.ToImmutableArray ( );
            }


            var syntaxTree = new SyntaxTree ( options, text, ParseExpression );
            diagnostics = syntaxTree.Diagnostics;
            return expression!;
        }

        /// <summary>
        /// Parses an expression from the provided text.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ExpressionSyntax ParseExpression ( LuaOptions options, SourceText text ) =>
            ParseExpression ( options, text, out _ );

        /// <summary>
        /// Parses an expression from the provided text.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        public static ExpressionSyntax ParseExpression ( LuaOptions options, String text, out ImmutableArray<Diagnostic> diagnostics )
        {
            var sourceText = SourceText.From ( text );
            return ParseExpression ( options, sourceText, out diagnostics );
        }

        /// <summary>
        /// Parses an expression from the provided text.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ExpressionSyntax ParseExpression ( LuaOptions options, String text )
        {
            var sourceText = SourceText.From ( text );
            return ParseExpression ( options, sourceText );
        }

        #endregion ParseExpression

        #region ParseTokens

        /// <summary>
        /// Parses all tokens from the input.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <param name="diagnostics"></param>
        /// <param name="includeEndOfFile"></param>
        /// <returns></returns>
        public static ImmutableArray<SyntaxToken> ParseTokens ( LuaOptions options, SourceText text, out ImmutableArray<Diagnostic> diagnostics, Boolean includeEndOfFile = false )
        {
            ImmutableArray<SyntaxToken>.Builder tokens = ImmutableArray.CreateBuilder<SyntaxToken> ( );

            void ParseTokens ( SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics )
            {
                var lexer = new Lexer ( syntaxTree );

                while ( true )
                {
                    SyntaxToken token = lexer.Lex ( );

                    if ( token.Kind != SyntaxKind.EndOfFileToken || includeEndOfFile )
                        tokens.Add ( token );

                    if ( token.Kind == SyntaxKind.EndOfFileToken )
                    {
                        root = new CompilationUnitSyntax ( ImmutableArray<StatementSyntax>.Empty, token );
                        break;
                    }
                }

                diagnostics = lexer.Diagnostics.ToImmutableArray ( );
            }

            var syntaxTree = new SyntaxTree ( options, text, ParseTokens );
            diagnostics = syntaxTree.Diagnostics;
            return tokens.ToImmutable ( );
        }

        /// <summary>
        /// Parses all tokens from the input.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <param name="includeEndOfFile"></param>
        /// <returns></returns>
        public static ImmutableArray<SyntaxToken> ParseTokens ( LuaOptions options, SourceText text, Boolean includeEndOfFile = false ) =>
            ParseTokens ( options, text, out _, includeEndOfFile );

        /// <summary>
        /// Parses all tokens from the input.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <param name="diagnostics"></param>
        /// <param name="includeEndOfFile"></param>
        /// <returns></returns>
        public static ImmutableArray<SyntaxToken> ParseTokens ( LuaOptions options, String text, out ImmutableArray<Diagnostic> diagnostics, Boolean includeEndOfFile = false )
        {
            var sourceText = SourceText.From ( text );
            return ParseTokens ( options, sourceText, out diagnostics, includeEndOfFile );
        }

        /// <summary>
        /// Parses all tokens from the input.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <param name="includeEndOfFile"></param>
        /// <returns></returns>
        public static ImmutableArray<SyntaxToken> ParseTokens ( LuaOptions options, String text, Boolean includeEndOfFile = false )
        {
            var sourceText = SourceText.From ( text );
            return ParseTokens ( options, sourceText, includeEndOfFile );
        }

        #endregion ParseTokens

        #region LoadTokens

        /// <summary>
        /// Loads and parses a token list from the provided file path.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="filePath"></param>
        /// <param name="includeEndOfFile"></param>
        /// <returns></returns>
        public static ImmutableArray<SyntaxToken> LoadTokens ( LuaOptions options, String filePath, Boolean includeEndOfFile = false )
        {
            var text = File.ReadAllText ( filePath );
            var sourceText = SourceText.From ( text, filePath );
            return ParseTokens ( options, sourceText, includeEndOfFile );
        }

        /// <summary>
        /// Loads and parses a token list from the provided file path.
        /// </summary>
        /// <param name="options">The options to use when lexing.</param>
        /// <param name="filePath">The path of the file to lex.</param>
        /// <param name="diagnostics">The diagnostics generated when lexing the file.</param>
        /// <param name="includeEndOfFile">Whether to include the End-of-File token.</param>
        /// <returns></returns>
        public static ImmutableArray<SyntaxToken> LoadTokens ( LuaOptions options, String filePath, out ImmutableArray<Diagnostic> diagnostics, Boolean includeEndOfFile = false )
        {
            var text = File.ReadAllText ( filePath );
            var sourceText = SourceText.From ( text, filePath );
            return ParseTokens ( options, sourceText, out diagnostics, includeEndOfFile );
        }

        #endregion LoadTokens

        #endregion Factory Methods

        private SourceText? _text;

        private delegate void ParseHandler (
            SyntaxTree syntaxTree,
            out CompilationUnitSyntax root,
            out ImmutableArray<Diagnostic> diagnostics );

        private SyntaxTree ( LuaOptions options, SourceText text, ParseHandler handler )
        {
            this.Options = options;
            this._text = text;

            handler ( this, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics );

            this.Diagnostics = diagnostics;
            this.Root = root;
        }

        /// <summary>
        /// The options used to parse this tree.
        /// </summary>
        public LuaOptions Options { get; }

        /// <summary>
        /// The text that originated this tree.
        /// </summary>
        public SourceText Text
        {
            get
            {
                if ( this._text is null )
                {
                    Interlocked.CompareExchange ( ref this._text, this.Root.GetText ( ), null );
                }

                return this._text!;
            }
        }

        /// <summary>
        /// The diagnostics generated by lexing and parsing the input.
        /// </summary>
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// The root node.
        /// </summary>
        public SyntaxNode Root { get; }
    }
}
