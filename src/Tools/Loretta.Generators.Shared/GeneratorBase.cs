using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators
{
    public abstract class GeneratorBase : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var extraFiles = new List<SourceText>();
            AddFixedFiles(context, extraFiles);
            var options = (context.ParseOptions as CSharpParseOptions)!;
            var compilation = (context.Compilation as CSharpCompilation)!.AddSyntaxTrees(extraFiles.Select(t => CSharpSyntaxTree.ParseText(t, options)));
            GenerateFiles(context, compilation);

        }

        protected virtual void AddFixedFiles(GeneratorExecutionContext context, List<SourceText> extraFiles)
        {
        }

        protected abstract void GenerateFiles(GeneratorExecutionContext context, CSharpCompilation compilation);

        protected static void CompletePartialType(
            StringWriter writer,
            INamedTypeSymbol namedTypeSymbol,
            Action<IndentedTextWriter> bodyAction,
            Action<IndentedTextWriter>? prefixAction = null)
        {
            TypeDeclarationSyntax? declaration = null;
            foreach (var declarationCandidate in namedTypeSymbol.DeclaringSyntaxReferences)
            {
                var syntax = declarationCandidate.GetSyntax();
                if (syntax is TypeDeclarationSyntax typeDeclaration)
                {
                    foreach (var modifer in typeDeclaration.Modifiers)
                    {
                        if (modifer.ValueText == "partial")
                            declaration = typeDeclaration;
                    }
                }
            }

            if (declaration is null)
                throw new InvalidOperationException("Cannot write a partial definition of a type that is not partial.");

            using var textWriter = new IndentedTextWriter(writer, "    ");
            prefixAction?.Invoke(textWriter);
            var stack = new Stack<CurlyIndenter>();
            foreach (var ancestor in declaration.AncestorsAndSelf().Reverse())
            {
                switch (ancestor.Kind())
                {
                    case SyntaxKind.NamespaceDeclaration:
                    {
                        var ancestorNamespace = (NamespaceDeclarationSyntax) ancestor;
                        stack.Push(new CurlyIndenter(
                            textWriter,
                            $"namespace {ancestorNamespace.Name}"));
                    }
                    break;

                    case SyntaxKind.RecordDeclaration:
                    case SyntaxKind.ClassDeclaration:
                    case SyntaxKind.StructDeclaration:
                    {
                        var ancestorDeclaration = (TypeDeclarationSyntax) ancestor;
                        if (!ancestorDeclaration.Modifiers.Any(mod => mod.ValueText == "partial"))
                            throw new InvalidOperationException("Type is contained within a non-partial struct.");
                        var nameBuilder = new StringBuilder();
                        nameBuilder.Append(string.Join(" ", ancestorDeclaration.Modifiers));
                        nameBuilder.Append(' ');
                        nameBuilder.Append(ancestorDeclaration.Keyword);
                        nameBuilder.Append(' ');
                        nameBuilder.Append(ancestorDeclaration.Identifier);
                        if (ancestorDeclaration.TypeParameterList?.Parameters.Count > 0)
                        {
                            nameBuilder.Append('<');
                            var isFirst = true;
                            foreach (var parameter in ancestorDeclaration.TypeParameterList.Parameters)
                            {
                                if (!isFirst)
                                    nameBuilder.Append(", ");
                                isFirst = false;
                                nameBuilder.Append(parameter.Identifier);
                            }
                            nameBuilder.Append('>');
                        }

                        stack.Push(new CurlyIndenter(
                            textWriter,
                             nameBuilder.ToString()));
                    }
                    break;
                }
            }

            bodyAction(textWriter);

            while (stack.Count > 0)
                stack.Pop().Dispose();

            textWriter.Flush();
            writer.Flush();
        }
    }
}
