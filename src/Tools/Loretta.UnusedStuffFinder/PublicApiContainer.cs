using Microsoft.CodeAnalysis;

namespace Loretta.UnusedStuffFinder
{
    internal class PublicApiContainer
    {
        private const int IncludeNonNullableReferenceTypeModifier = 1 << 8;

        private static readonly SymbolDisplayFormat s_publicApiFormat =
            new(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                propertyStyle: SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions:
                    SymbolDisplayMemberOptions.IncludeParameters |
                    SymbolDisplayMemberOptions.IncludeContainingType |
                    SymbolDisplayMemberOptions.IncludeExplicitInterface |
                    SymbolDisplayMemberOptions.IncludeModifiers |
                    SymbolDisplayMemberOptions.IncludeConstantValue,
                parameterOptions:
                    SymbolDisplayParameterOptions.IncludeExtensionThis |
                    SymbolDisplayParameterOptions.IncludeParamsRefOut |
                    SymbolDisplayParameterOptions.IncludeType |
                    SymbolDisplayParameterOptions.IncludeName |
                    SymbolDisplayParameterOptions.IncludeDefaultValue,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                    SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
                    (SymbolDisplayMiscellaneousOptions) IncludeNonNullableReferenceTypeModifier);

        public static PublicApiContainer LoadAll(IEnumerable<string> files)
        {
            var publicApis = new PublicApiContainer();
            foreach (var file in files)
            {
                publicApis.Load(file);
            }
            return publicApis;
        }

        private IImmutableSet<string> _publicApiSymbols;

        public PublicApiContainer()
        {
            _publicApiSymbols = ImmutableHashSet.Create<string>(StringComparer.Ordinal);
        }

        public void Load(string path)
        {
            IEnumerable<string> lines = File.ReadAllLines(path);
            if (lines.First().StartsWith("#nullable", StringComparison.Ordinal))
                lines = lines.Skip(1);
            ImmutableInterlocked.Update(ref _publicApiSymbols, static (symbols, lines) => symbols.Union(lines), lines);
        }

        public bool IsPartOfPublicApi(ISymbol symbol) => _publicApiSymbols.Contains(GetPublicApiName(symbol));

        public static string GetPublicApiName(ISymbol symbol)
        {
            string publicApiName = symbol.ToDisplayString(s_publicApiFormat);

            ITypeSymbol? memberType = null;
            if (symbol is IMethodSymbol method)
            {
                memberType = method.ReturnType;
            }
            else if (symbol is IPropertySymbol property)
            {
                memberType = property.Type;
            }
            else if (symbol is IEventSymbol @event)
            {
                memberType = @event.Type;
            }
            else if (symbol is IFieldSymbol field)
            {
                memberType = field.Type;
            }

            if (memberType != null)
            {
                publicApiName = publicApiName + " -> " + memberType.ToDisplayString(s_publicApiFormat);
            }

            if (((symbol as INamespaceSymbol)?.IsGlobalNamespace).GetValueOrDefault())
            {
                return string.Empty;
            }

            return publicApiName;
        }
    }
}
