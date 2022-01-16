using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Loretta.UnusedStuffFinder
{
    internal class ReferenceFinder
    {
        private const int MaxTries = 20;

        public static async Task<IEnumerable<ReferencedSymbol>> FindReferencesAsync(
            Solution solution,
            ISymbol symbol,
            CancellationToken cancellationToken = default)
        {
            var tries = 0;
        start:
            try
            {
                return await SymbolFinder.FindReferencesAsync(
                    symbol,
                    solution,
                    cancellationToken);
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch when (++tries <= MaxTries) { goto start; }
        }

        private static SyntaxNode? GetDeclaration(SyntaxNode node)
        {
            return node.AncestorsAndSelf(false)
                       .FirstOrDefault(isDeclaration);

            static bool isDeclaration(SyntaxNode node)
            {
                return node.Kind() is SyntaxKind.AddAccessorDeclaration or SyntaxKind.ClassDeclaration or SyntaxKind.ConstructorDeclaration
                                   or SyntaxKind.ConversionOperatorDeclaration or SyntaxKind.DelegateDeclaration or SyntaxKind.DestructorDeclaration
                                   or SyntaxKind.EnumDeclaration or SyntaxKind.EnumMemberDeclaration or SyntaxKind.EventDeclaration
                                   or SyntaxKind.GetAccessorDeclaration or SyntaxKind.IndexerDeclaration or SyntaxKind.InitAccessorDeclaration
                                   or SyntaxKind.InterfaceDeclaration or SyntaxKind.MethodDeclaration or SyntaxKind.OperatorDeclaration
                                   or SyntaxKind.PropertyDeclaration or SyntaxKind.RecordDeclaration or SyntaxKind.RecordStructDeclaration
                                   or SyntaxKind.RemoveAccessorDeclaration or SyntaxKind.SetAccessorDeclaration or SyntaxKind.StructDeclaration
                                   or SyntaxKind.FieldDeclaration or SyntaxKind.EventFieldDeclaration or SyntaxKind.UsingDirective
                       // Field declarations can have multiple field symbols declared in them
                       || (node.IsKind(SyntaxKind.VariableDeclarator) && node.Parent?.Parent?.Kind() is SyntaxKind.FieldDeclaration or SyntaxKind.EventFieldDeclaration)
                       // A property can have an arrow body so we have to differentiate between a normal one and a short one
                       || (node.IsKind(SyntaxKind.ArrowExpressionClause) && node.Parent!.IsKind(SyntaxKind.PropertyDeclaration));
            }
        }

        public static async Task<IEnumerable<ISymbol>?> GetReferenceSymbolsAsync(
            ReferenceLocation referenceLocation,
            CancellationToken cancellationToken = default)
        {
            var model = (await referenceLocation.Document.GetSemanticModelAsync(cancellationToken))!;
            var location = referenceLocation.Location;
            var root = await location.SourceTree!.GetRootAsync(cancellationToken);
            var node = root.FindNode(location.SourceSpan);
            var decl = GetDeclaration(node);
            if (decl is null)
                return null;

            // Check if the declaration is a field
            if (decl.Kind() is SyntaxKind.FieldDeclaration or SyntaxKind.EventFieldDeclaration)
            {
                // If it is, and not part of the value (as a VariableDeclarator), then it means that it's the type or part of the type for that field,
                // which means that all fields there depend on it.
                var fieldDecl = (BaseFieldDeclarationSyntax) decl;
                var symbols = new List<ISymbol>();
                foreach (var variableDecl in fieldDecl.Declaration.Variables)
                {
                    var symbol = model.GetDeclaredSymbol(variableDecl, cancellationToken);
                    if (symbol is null)
                        continue;
                    symbols.Add(symbol);
                }
                return symbols;
            }
            // Else check if it's an arrow expression
            else if (decl.IsKind(SyntaxKind.ArrowExpressionClause))
            {
                // If it is, then this is a property in arrow form and the symbol is being referenced in the getter body, so we have to return the getter.
                var propertyDecl = (PropertyDeclarationSyntax) decl.Parent!;
                var propertySymbol = model.GetDeclaredSymbol(propertyDecl, cancellationToken)!;
                return new[] { propertySymbol.GetMethod! };
            }
            else
            {
                // Otherwise return the symbol the declaration is referring to.
                var symbol = model.GetDeclaredSymbol(decl, cancellationToken);
                if (symbol is null)
                    return null;
                return new[] { symbol };
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1024:Compare symbols correctly", Justification = "<Pending>")]
        public static async Task<ImmutableArray<ISymbol>> FindReferencingSymbolsAsync(
            Solution solution,
            ISymbol symbol,
            CancellationToken cancellationToken = default)
        {
            var results = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            var references = await FindReferencesAsync(solution, symbol, cancellationToken);
            foreach (var reference in references)
            {
                foreach (var location in reference.Locations)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var containingSymbols = await GetReferenceSymbolsAsync(location, cancellationToken);
                    if (containingSymbols is null)
                    {
                        continue;
                    }
                    results.UnionWith(containingSymbols);
                }
            }
            return results.ToImmutableArray();
        }
    }
}
