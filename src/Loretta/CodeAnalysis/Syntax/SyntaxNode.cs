using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The base class for all syntax nodes.
    /// </summary>
    public abstract class SyntaxNode
    {
        private protected SyntaxNode ( )
        {
        }

        /// <summary>
        /// This node's kind.
        /// </summary>
        public abstract SyntaxKind Kind { get; }

        /// <summary>
        /// The span of this node.
        /// </summary>
        public virtual TextSpan Span
        {
            get
            {
                TextSpan first = this.GetChildren ( ).First ( ).Span;
                TextSpan last = this.GetChildren ( ).Last ( ).Span;
                return TextSpan.FromBounds ( first.Start, last.End );
            }
        }

        /// <summary>
        /// The span of this node including trivia.
        /// </summary>
        public virtual TextSpan FullSpan
        {
            get
            {
                TextSpan first = this.GetChildren ( ).First ( ).FullSpan;
                TextSpan last = this.GetChildren ( ).Last ( ).FullSpan;
                return TextSpan.FromBounds ( first.Start, last.End );
            }
        }

        /// <summary>
        /// Retrieves all immediate children from this node.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<SyntaxNode> GetChildren ( );

        /// <summary>
        /// Returns the last token of the tree rooted by this node.
        /// </summary>
        /// <returns></returns>
        public virtual SyntaxToken GetLastToken ( )
        {
            if ( this is SyntaxToken token )
                return token;

            return this.GetChildren ( ).Last ( ).GetLastToken ( );
        }

        /// <summary>
        /// Pretty prints a tree to the console.
        /// </summary>
        /// <param name="writer"></param>
        public void PrettyPrintTo ( TextWriter writer ) =>
            PrettyPrint ( writer, this );

        private static void PrettyPrint ( TextWriter writer, SyntaxNode node, String indent = "", Boolean isLast = true )
        {
            var isToConsole = writer == Console.Out;
            var token = node as SyntaxToken;

            if ( token != null )
            {
                foreach ( SyntaxTrivia trivia in token.LeadingTrivia )
                {
                    if ( isToConsole )
                        Console.ForegroundColor = ConsoleColor.DarkGray;

                    writer.Write ( indent );
                    writer.Write ( "├──" );

                    if ( isToConsole )
                        Console.ForegroundColor = ConsoleColor.DarkGreen;

                    writer.WriteLine ( $"L: {trivia.Kind}" );
                }
            }

            var hasTrailingTrivia = token != null && token.TrailingTrivia.Any ( );
            var tokenMarker = !hasTrailingTrivia && isLast ? "└──" : "├──";

            if ( isToConsole )
                Console.ForegroundColor = ConsoleColor.DarkGray;

            writer.Write ( indent );
            writer.Write ( tokenMarker );

            if ( isToConsole )
                Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;

            writer.Write ( node.Kind );

            if ( token != null && token.Value.IsSome )
            {
                writer.Write ( " " );
                writer.Write ( token.Value.Value ?? "null" );
            }

            if ( isToConsole )
                Console.ResetColor ( );

            writer.WriteLine ( );

            if ( token != null )
            {
                foreach ( SyntaxTrivia trivia in token.TrailingTrivia )
                {
                    var isLastTrailingTrivia = trivia == token.TrailingTrivia.Last ( );
                    var triviaMarker = isLast && isLastTrailingTrivia ? "└──" : "├──";

                    if ( isToConsole )
                        Console.ForegroundColor = ConsoleColor.DarkGray;

                    writer.Write ( indent );
                    writer.Write ( triviaMarker );

                    if ( isToConsole )
                        Console.ForegroundColor = ConsoleColor.DarkGreen;

                    writer.WriteLine ( $"T: {trivia.Kind}" );
                }
            }

            indent += isLast ? "   " : "│  ";

            SyntaxNode? lastChild = node.GetChildren ( ).LastOrDefault ( );

            foreach ( SyntaxNode? child in node.GetChildren ( ) )
                PrettyPrint ( writer, child, indent, child == lastChild );
        }

        /// <summary>
        /// Pretty-prints a tree of the syntax node.
        /// </summary>
        /// <returns></returns>
        public override String ToString ( )
        {
            using var writer = new StringWriter ( );
            this.PrettyPrintTo ( writer );
            return writer.ToString ( );
        }

        /// <summary>
        /// The accept part of the visitor pattern.
        /// </summary>
        /// <param name="syntaxVisitor"></param>
        public abstract void Accept ( SyntaxVisitor syntaxVisitor );

        /// <summary>
        /// The accept part of the visitor pattern.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="syntaxVisitor"></param>
        /// <returns></returns>
        public abstract TReturn? Accept<TReturn> ( SyntaxVisitor<TReturn> syntaxVisitor );

        /// <summary>
        /// Writes out this node to the provided <paramref name="textWriter"/>.
        /// </summary>
        /// <param name="textWriter"></param>
        public virtual void WriteTo ( TextWriter textWriter )
        {
            if ( this is SyntaxToken token )
            {
                foreach ( SyntaxTrivia? leading in token.LeadingTrivia )
                    textWriter.Write ( leading.Text );
                textWriter.Write ( token.Text );
                foreach ( SyntaxTrivia? trailing in token.TrailingTrivia )
                    textWriter.Write ( trailing.Text );
            }
            else
            {
                foreach ( SyntaxNode? child in this.GetChildren ( ) )
                {
                    child.WriteTo ( textWriter );
                }
            }
        }

        /// <summary>
        /// Obtains the <see cref="SourceText"/> for this node.
        /// </summary>
        /// <returns></returns>
        public SourceText GetText ( )
        {
            var sb = new StringBuilder ( );
            this.WriteTo ( new StringWriter ( sb ) );
            return SourceText.From ( sb.ToString ( ) );
        }
    }
}
