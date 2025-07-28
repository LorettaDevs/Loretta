using Loretta.CodeAnalysis.Syntax.InternalSyntax;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class LanguageParser
    {
        private partial TypeBindingSyntax? TryParseReturnTypeBinding()
        {
            if (CurrentToken.Kind is SyntaxKind.ColonToken)
            {
                var colonToken = EatTokenWithPrejudice(SyntaxKind.ColonToken);
                var type = ParseReturnType();
                var typeBinding = SyntaxFactory.TypeBinding(colonToken, type);
                if (!Options.SyntaxOptions.AcceptTypedLua)
                    typeBinding = AddError(typeBinding, ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion);
                return typeBinding;
            }
            return null;
        }

        private partial TypeSyntax ParseReturnType()
        {
            // If the user is returning a type pack, then parse that. 
            if (CurrentToken.Kind is SyntaxKind.OpenParenthesisToken)
                return ParseTypeStartingWithParenthesis(TypePackType.Some);

            // If we don't have a variadic type to parse, then just move on.
            if (CurrentToken.Kind != SyntaxKind.DotDotDotToken) return ParseType();

            // Otherwise, we have a variadic return type, so go ahead with that.
            var dotDotDotToken = EatToken(SyntaxKind.DotDotDotToken);
            var type           = ParseType();
            return SyntaxFactory.VariadicTypePack(dotDotDotToken, type);
        }

        private partial TypeBindingSyntax? TryParseTypeBinding()
        {
            if (CurrentToken.Kind is SyntaxKind.ColonToken)
            {
                var colonToken = EatTokenWithPrejudice(SyntaxKind.ColonToken);
                var type = ParseType();
                var typeBinding = SyntaxFactory.TypeBinding(colonToken, type);
                if (!Options.SyntaxOptions.AcceptTypedLua)
                    typeBinding = AddError(typeBinding, ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion);
                return typeBinding;
            }
            return null;
        }

        internal partial TypeSyntax ParseType()
        {
            var isUnion = false;
            var isIntersection = false;
            
            TypeSyntax type;
            var isNilable = false;
            
            switch (CurrentToken.Kind)
            {
                case SyntaxKind.PipeToken:
                {
                    var pipeToken = EatTokenWithPrejudice(SyntaxKind.PipeToken);
                    var right = ParsePossiblyNullableType();
                    isNilable |= right.Kind is SyntaxKind.NilableType;
                    type = SyntaxFactory.UnionType(
                        null,
                        pipeToken,
                        right);
                    isUnion = true;
                    break;
                }
                case SyntaxKind.AmpersandToken:
                {
                    var ampersandToken = EatTokenWithPrejudice(SyntaxKind.AmpersandToken);
                    var right = ParsePossiblyNullableType();
                    isNilable |= right.Kind is SyntaxKind.NilableType;
                    type = SyntaxFactory.IntersectionType(
                        null,
                        ampersandToken,
                        right);
                    isIntersection = true;
                    break;
                }
                default:
                    type = ParsePossiblyNullableType();
                    isNilable |= type.Kind is SyntaxKind.NilableType;
                    break;
            }

            while (CurrentToken.Kind is SyntaxKind.PipeToken or SyntaxKind.AmpersandToken)
            {
                if (CurrentToken.Kind == SyntaxKind.PipeToken)
                {
                    var pipeToken = EatTokenWithPrejudice(SyntaxKind.PipeToken);
                    var right = ParsePossiblyNullableType();
                    isNilable |= right.Kind is SyntaxKind.NilableType;
                    type = SyntaxFactory.UnionType(
                        type,
                        pipeToken,
                        right);
                    isUnion = true;
                }
                else if (CurrentToken.Kind == SyntaxKind.AmpersandToken)
                {
                    var ampersandToken = EatTokenWithPrejudice(SyntaxKind.AmpersandToken);
                    var right = ParsePossiblyNullableType();
                    isNilable |= right.Kind is SyntaxKind.NilableType;
                    type = SyntaxFactory.IntersectionType(
                        type,
                        ampersandToken,
                        right);
                    isIntersection = true;
                }
            }

            if (isNilable && isIntersection)
            {
                type = AddError(type, ErrorCode.ERR_MixingNilableAndIntersectionNotAllowed);
            }
            if (isUnion && isIntersection)
            {
                type = AddError(type, ErrorCode.ERR_MixingUnionsAndIntersectionsNotAllowed);
            }

            return type;
        }

        private TypeSyntax ParsePossiblyNullableType()
        {
            var type = ParseSimpleType();
            while (CurrentToken.Kind is SyntaxKind.QuestionToken)
            {
                type = SyntaxFactory.NilableType(
                    type,
                    EatTokenWithPrejudice(SyntaxKind.QuestionToken));
            }
            return type;
        }

        private TypeSyntax ParseSimpleType()
        {
            return CurrentToken.Kind switch
            {
                // function or parenthesized
                SyntaxKind.OpenParenthesisToken
                or SyntaxKind.LessThanToken =>
                    ParseTypeStartingWithParenthesis(TypePackType.None),
                // table
                SyntaxKind.OpenBraceToken =>
                    ParseTableBasedType(),
                // typeof type
                SyntaxKind.IdentifierToken when CurrentToken.ContextualKind is SyntaxKind.TypeofKeyword =>
                    ParseTypeofType(),
                // literal types
                SyntaxKind.StringLiteralToken =>
                    SyntaxFactory.LiteralType(
                        SyntaxKind.StringType,
                        EatTokenWithPrejudice(SyntaxKind.StringLiteralToken)),
                SyntaxKind.NilKeyword =>
                    SyntaxFactory.LiteralType(
                        SyntaxKind.NilType,
                        EatTokenWithPrejudice(SyntaxKind.NilKeyword)),
                SyntaxKind.TrueKeyword =>
                    SyntaxFactory.LiteralType(
                        SyntaxKind.TrueType,
                        EatTokenWithPrejudice(SyntaxKind.TrueKeyword)),
                SyntaxKind.FalseKeyword =>
                    SyntaxFactory.LiteralType(
                        SyntaxKind.FalseType,
                        EatTokenWithPrejudice(SyntaxKind.FalseKeyword)),

                // If everything else fails, try to parse an identifier-based name.
                _ => ParseTypeName(),
            };
        }

        private partial TypeParameterListSyntax? TryParseTypeParameterList(bool acceptDefaults)
        {
            if (CurrentToken.Kind is SyntaxKind.LessThanToken)
            {
                return ParseTypeParameterList(acceptDefaults);
            }

            return null;
        }

        private TypeParameterListSyntax ParseTypeParameterList(bool acceptDefaults)
        {
            var seenPackParameter = false;
            var lessThanToken = EatTokenWithPrejudice(SyntaxKind.LessThanToken);
            var parametersBuilder = _pool.AllocateSeparated<TypeParameterSyntax>();

            var parameter = ParseTypeParameter(acceptDefaults);
            seenPackParameter |= parameter.DotDotDotToken is not null;
            parametersBuilder.Add(parameter);

            while (CurrentToken.Kind is SyntaxKind.CommaToken)
            {
                var separator = EatToken(SyntaxKind.CommaToken);
                parametersBuilder.AddSeparator(separator);

                parameter = ParseTypeParameter(acceptDefaults);
                if (seenPackParameter && parameter.DotDotDotToken is null)
                {
                    parameter = AddError(
                        parameter,
                        ErrorCode.ERR_NormalTypeParametersComeBeforePacks);
                }
                seenPackParameter |= parameter.DotDotDotToken is not null;
                parametersBuilder.Add(parameter);
            }

            var greaterThanToken = EatToken(SyntaxKind.GreaterThanToken);

            var parameters = _pool.ToListAndFree(parametersBuilder);
            var typeParameterList = SyntaxFactory.TypeParameterList(
                lessThanToken,
                parameters,
                greaterThanToken);
            if (!Options.SyntaxOptions.AcceptTypedLua)
            {
                typeParameterList = AddError(
                    typeParameterList,
                    ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion);
            }
            return typeParameterList;
        }

        private TypeParameterSyntax ParseTypeParameter(bool acceptDefaults)
        {
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            SyntaxToken? dotDotDotToken = null;
            if (CurrentToken.Kind is SyntaxKind.DotDotDotToken)
                dotDotDotToken = EatToken(SyntaxKind.DotDotDotToken);

            EqualsTypeSyntax? equalsType = null;
            if (acceptDefaults && CurrentToken.Kind is SyntaxKind.EqualsToken)
            {
                var equalsToken = EatToken(SyntaxKind.EqualsToken);
                var type = dotDotDotToken is not null
                    ? ParseVariadicTypeParameterValue()
                    : ParseType();

                equalsType = SyntaxFactory.EqualsType(
                    equalsToken,
                    type);
            }

            return SyntaxFactory.TypeParameter(
                identifier,
                dotDotDotToken,
                equalsType);
        }

        private TypeArgumentListSyntax ParseTypeArgumentList()
        {
            var lessThanToken = EatTokenWithPrejudice(SyntaxKind.LessThanToken);
            var typesBuilder = _pool.AllocateSeparated<TypeSyntax>();

            while (CurrentToken.Kind is not (SyntaxKind.GreaterThanToken or SyntaxKind.EndOfFileToken))
            {
                var type = ParseTypeArgument();
                typesBuilder.Add(type);

                if (CurrentToken.Kind is SyntaxKind.CommaToken)
                {
                    var separator = EatToken(SyntaxKind.CommaToken);
                    typesBuilder.AddSeparator(separator);
                }
                else
                {
                    break;
                }
            }

            var greaterThanToken = EatToken(SyntaxKind.GreaterThanToken);

            var types = _pool.ToListAndFree(typesBuilder);
            var typeArgumentList = SyntaxFactory.TypeArgumentList(
                lessThanToken,
                types,
                greaterThanToken);
            return typeArgumentList;
        }

        private TypeSyntax ParseTypeArgument()
        {
            if (CurrentToken.Kind is SyntaxKind.DotDotDotToken)
            {
                var dotDotDotToken = EatTokenWithPrejudice(SyntaxKind.DotDotDotToken);
                var type = ParseType();
                return SyntaxFactory.VariadicTypePack(
                    dotDotDotToken,
                    type);
            }
            else if (CurrentToken.Kind is SyntaxKind.IdentifierToken
                && PeekToken(1).Kind is SyntaxKind.DotDotDotToken)
            {
                var identifier = EatTokenWithPrejudice(SyntaxKind.IdentifierToken);
                var dotDotDotToken = EatTokenWithPrejudice(SyntaxKind.DotDotDotToken);
                return SyntaxFactory.GenericTypePack(
                    identifier,
                    dotDotDotToken);
            }
            else
            {
                return ParseType();
            }
        }

        private TypeSyntax ParseVariadicTypeParameterValue()
        {
            switch (CurrentToken.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    return ParseTypeStartingWithParenthesis(TypePackType.Only);

                case SyntaxKind.DotDotDotToken:
                {
                    var dotDotDotToken = EatToken(SyntaxKind.DotDotDotToken);
                    var type = ParseType();
                    return SyntaxFactory.VariadicTypePack(dotDotDotToken, type);
                }

                default:
                {
                    var identifier = EatToken(SyntaxKind.IdentifierToken);
                    var dotDotDotToken = EatToken(SyntaxKind.DotDotDotToken);
                    return SyntaxFactory.GenericTypePack(identifier, dotDotDotToken);
                }
            }
        }

        private enum TypePackType
        {
            None,
            Some,
            Only
        }

        private TypeSyntax ParseTypeStartingWithParenthesis(TypePackType typePackType)
        {
            var typeParameterList = TryParseTypeParameterList(true);

            var openParenthesisToken = EatToken(SyntaxKind.OpenParenthesisToken);
            // We use LuaSyntaxNode here because FunctionTypeParameterSyntax does not inherit from TypeSyntax, so we have
            // to pick the next common parent, which is LuaSyntaxToken.
            // We ensure that everything has the correct types through a combination of the hasParameter flag and
            // typePackType, to ensure we only add FunctionTypeParameterSyntaxes whenever we will be able to accept them.
            var parenthesizedElements = _pool.AllocateSeparated<LuaSyntaxNode>();
            var hasParameters         = false;

            while (CurrentToken.Kind is not (SyntaxKind.CloseParenthesisToken or SyntaxKind.EndOfFileToken))
            {
                // Check for the `identifier ':'` pattern for function type parameters.
                if (typePackType != TypePackType.Only
                    && CurrentToken.Kind is SyntaxKind.IdentifierToken
                    && PeekToken(1).Kind is SyntaxKind.ColonToken)
                {
                    hasParameters = true;
                    var name      = EatToken(SyntaxKind.IdentifierToken);
                    var colon     = EatToken(SyntaxKind.ColonToken);
                    var type      = ParseType();
                    var parameter = SyntaxFactory.FunctionTypeParameter(name, colon, type);

                    parenthesizedElements.Add(parameter);
                }
                else if (CurrentToken.Kind is SyntaxKind.DotDotDotToken)
                {
                    var dotDotDotToken = EatToken(SyntaxKind.DotDotDotToken);
                    var type           = ParseType();
                    var variadicType   = SyntaxFactory.VariadicTypePack(dotDotDotToken, type);

                    parenthesizedElements.Add(variadicType);
                    break; // Leave the loop because nothing is allowed past variadic types.
                }
                else
                {
                    var type = ParseType();
                    parenthesizedElements.Add(type);
                }

                if (CurrentToken.Kind is SyntaxKind.CommaToken)
                {
                    var separator = EatToken(SyntaxKind.CommaToken);
                    parenthesizedElements.Add(separator);
                }
                else
                {
                    break;
                }
            }

            var closeParenthesisToken = EatToken(SyntaxKind.CloseParenthesisToken);

            if (typePackType == TypePackType.Only) goto pack;

            if (typeParameterList is not null
                // If we have any parameters, there's no option other than a function type.
                || hasParameters
                || CurrentToken.Kind == SyntaxKind.MinusGreaterThanToken
                // If there's more than one type in the list, and we are not allowed to accept packs, then assume the
                // user intended to type a function type.
                || (parenthesizedElements.Count != 1 && typePackType == TypePackType.None))
            {
                var slimArrow  = EatToken(SyntaxKind.MinusGreaterThanToken);
                var returnType = ParseReturnType();

                // Convert parameters to a FunctionTypeParameter if they aren't one already.
                for (var idx = 0; idx < parenthesizedElements.Count; idx += 2)
                {
                    var elem = parenthesizedElements[idx];
                    if (elem is not FunctionTypeParameterSyntax)
                    {
                        parenthesizedElements[idx] = SyntaxFactory.FunctionTypeParameter(
                            identifier: null,
                            colonToken: null,
                            (TypeSyntax) elem!);
                    }
                }

                var parameters = _pool.ToListAndFree(
                    new SeparatedSyntaxListBuilder<FunctionTypeParameterSyntax>(
                        parenthesizedElements.UnderlyingBuilder));

                return SyntaxFactory.FunctionType(
                    typeParameterList,
                    openParenthesisToken,
                    parameters,
                    closeParenthesisToken,
                    slimArrow,
                    returnType);
            }
            
            if (parenthesizedElements.Count == 1)
            {
                // We won't arrive here if there are any parameters, so the only option is a TypeSyntax.
                var type = (TypeSyntax) parenthesizedElements[0]!;
                return SyntaxFactory.ParenthesizedType(openParenthesisToken, type, closeParenthesisToken);
            }

        pack:
            // We also only arrive here without parameters.
            var types = _pool.ToListAndFree(
                new SeparatedSyntaxListBuilder<TypeSyntax>(parenthesizedElements.UnderlyingBuilder));
            return SyntaxFactory.TypePack(openParenthesisToken, types, closeParenthesisToken);
        }

        private TableBasedTypeSyntax ParseTableBasedType()
        {
            var hasIndexer = false;
            var openBrace = EatTokenWithPrejudice(SyntaxKind.OpenBraceToken);

            if (CurrentToken.Kind is SyntaxKind.OpenBracketToken or SyntaxKind.CloseBraceToken
                || PeekToken(1).Kind == SyntaxKind.ColonToken)
            {
                var elementsBuilder = _pool.AllocateSeparated<TableTypeElementSyntax>();
                while (CurrentToken.Kind is not (SyntaxKind.CloseBraceToken or SyntaxKind.EndOfFileToken))
                {
                    var element = ParseTableTypeElement();
                    if (element.Kind is SyntaxKind.TableTypeIndexer)
                    {
                        if (hasIndexer)
                        {
                            element = AddError(
                                element,
                                ErrorCode.ERR_OnlyOneTableTypeIndexerIsAllowed);
                        }
                        hasIndexer = true;
                    }
                    elementsBuilder.Add(element);

                    // If there's a separator, then there might be a field next
                    if (CurrentToken.Kind is SyntaxKind.CommaToken or SyntaxKind.SemicolonToken)
                    {
                        elementsBuilder.AddSeparator(EatToken());
                    }
                    else
                    {
                        // Otherwise we're done.
                        break;
                    }
                }

                var elements = _pool.ToListAndFree(elementsBuilder);
                var closeBrace = EatToken(SyntaxKind.CloseBraceToken);

                return SyntaxFactory.TableType(
                    openBrace,
                    elements,
                    closeBrace);
            }
            else
            {
                var type = ParseType();
                var closeBrace = EatToken(SyntaxKind.CloseBraceToken);

                return SyntaxFactory.ArrayType(
                    openBrace,
                    type,
                    closeBrace);
            }
        }

        private TableTypeElementSyntax ParseTableTypeElement()
        {
            if (CurrentToken.Kind is SyntaxKind.OpenBracketToken)
            {
                var openBracket = EatTokenWithPrejudice(SyntaxKind.OpenBracketToken);
                var indexType = ParseType();
                var closeBracket = EatToken(SyntaxKind.CloseBracketToken);
                var colon = EatToken(SyntaxKind.ColonToken);
                var valueType = ParseType();

                return SyntaxFactory.TableTypeIndexer(
                    openBracket,
                    indexType,
                    closeBracket,
                    colon,
                    valueType);
            }
            else
            {
                var identifier = EatTokenWithPrejudice(SyntaxKind.IdentifierToken);
                var colonToken = EatToken(SyntaxKind.ColonToken);
                var valueType = ParseType();

                return SyntaxFactory.TableTypeProperty(
                    identifier,
                    colonToken,
                    valueType);
            }
        }

        private TypeofTypeSyntax ParseTypeofType()
        {
            var typeofKeyword = EatContextualToken(SyntaxKind.TypeofKeyword);
            var openParenthesis = EatToken(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpressionCore();
            var closeParenthesis = EatToken(SyntaxKind.CloseParenthesisToken);

            return SyntaxFactory.TypeofType(typeofKeyword, openParenthesis, expression, closeParenthesis);
        }

        private TypeNameSyntax ParseTypeName()
        {
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            TypeNameSyntax currentName = SyntaxFactory.SimpleTypeName(identifier, null);

            while (CurrentToken.Kind is SyntaxKind.DotToken)
            {
                var dotToken = EatToken(SyntaxKind.DotToken);
                var memberName = EatToken(SyntaxKind.IdentifierToken);
                currentName = SyntaxFactory.CompositeTypeName(currentName, dotToken, memberName, null);
            }

            if (CurrentToken.Kind is SyntaxKind.LessThanToken)
            {
                var typeArgumentList = ParseTypeArgumentList();
                if (currentName is SimpleTypeNameSyntax simpleTypeName)
                {
                    currentName = simpleTypeName.Update(simpleTypeName.IdentifierToken, typeArgumentList);
                }
                else
                {
                    var compositeTypeName = (CompositeTypeNameSyntax) currentName;
                    currentName = compositeTypeName.Update(
                        compositeTypeName.Base,
                        compositeTypeName.DotToken,
                        compositeTypeName.identifierToken,
                        typeArgumentList);
                }
            }

            return currentName;
        }
    }
}
