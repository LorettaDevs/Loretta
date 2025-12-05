
namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing;

public sealed class TypeParsingTests : ParsingTestsBase
{
    private const string TypeArgumentListString = "<Type, Type..., ...Type, Type.Member>";

    private async Task CheckTypeArgumentListAsync()
    {
        await N(SyntaxKind.TypeArgumentList);
        {
            await N(SyntaxKind.LessThanToken);
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "Type");
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.GenericTypePack);
            {
                await N(SyntaxKind.IdentifierToken, "Type");
                await N(SyntaxKind.DotDotDotToken);
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.VariadicTypePack);
            {
                await N(SyntaxKind.DotDotDotToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "Type");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.CompositeTypeName);
            {
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "Type");
                }
                await N(SyntaxKind.DotToken);
                await N(SyntaxKind.IdentifierToken, "Member");
            }
            await N(SyntaxKind.GreaterThanToken);
        }
    }

    [Test]
    public async Task Parser_ParsesSimpleTypeName()
    {
        await UsingTypeAsync("Type");

        await N(SyntaxKind.SimpleTypeName);
        {
            await N(SyntaxKind.IdentifierToken, "Type");
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesSimpleTypeName_WithTypeArgumentList()
    {
        await UsingTypeAsync($"Type{TypeArgumentListString}");

        await N(SyntaxKind.SimpleTypeName);
        {
            await N(SyntaxKind.IdentifierToken, "Type");
            await CheckTypeArgumentListAsync();
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesCompositeTypeName()
    {
        await UsingTypeAsync("Type.Member");

        await N(SyntaxKind.CompositeTypeName);
        {
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "Type");
            }
            await N(SyntaxKind.DotToken);
            await N(SyntaxKind.IdentifierToken, "Member");
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesCompositeTypeName_WithTypeArgumentList()
    {
        await UsingTypeAsync($"Type.Member{TypeArgumentListString}");

        await N(SyntaxKind.CompositeTypeName);
        {
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "Type");
            }
            await N(SyntaxKind.DotToken);
            await N(SyntaxKind.IdentifierToken, "Member");
            await CheckTypeArgumentListAsync();
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeofType_WithStrings()
    {
        await UsingTypeAsync("typeof('hi')");

        await N(SyntaxKind.TypeofType);
        {
            await N(SyntaxKind.TypeofKeyword);
            await N(SyntaxKind.OpenParenthesisToken);
            {
                await N(SyntaxKind.StringLiteralExpression);
                {
                    await N(SyntaxKind.StringLiteralToken, "'hi'");
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeofType_WithNumbers()
    {
        await UsingTypeAsync("typeof(1)");

        await N(SyntaxKind.TypeofType);
        {
            await N(SyntaxKind.TypeofKeyword);
            await N(SyntaxKind.OpenParenthesisToken);
            {
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "1");
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);
        }
    }

    [Test]
    public async Task Parser_ParsesTypeofType_WithTables()
    {
        await UsingTypeAsync("typeof({ 1 })");

        await N(SyntaxKind.TypeofType);
        {
            await N(SyntaxKind.TypeofKeyword);
            await N(SyntaxKind.OpenParenthesisToken);
            {
                await N(SyntaxKind.TableConstructorExpression);
                await N(SyntaxKind.OpenBraceToken);
                {
                    await N(SyntaxKind.UnkeyedTableField);
                    {
                        await N(SyntaxKind.NumericalLiteralExpression);
                        {
                            await N(SyntaxKind.NumericLiteralToken, "1");
                        }
                    }
                }
                await N(SyntaxKind.CloseBraceToken);
            }
            await N(SyntaxKind.CloseParenthesisToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeofType_WithComplexExpression()
    {
        await UsingTypeAsync("typeof(tbl[1].member:method { 'hi' })");

        await N(SyntaxKind.TypeofType);
        {
            await N(SyntaxKind.TypeofKeyword);
            await N(SyntaxKind.OpenParenthesisToken);
            {
                await N(SyntaxKind.MethodCallExpression);
                {
                    await N(SyntaxKind.MemberAccessExpression);
                    {
                        await N(SyntaxKind.ElementAccessExpression);
                        {
                            await N(SyntaxKind.IdentifierName);
                            {
                                await N(SyntaxKind.IdentifierToken, "tbl");
                            }
                            await N(SyntaxKind.OpenBracketToken);
                            await N(SyntaxKind.NumericalLiteralExpression);
                            {
                                await N(SyntaxKind.NumericLiteralToken, "1");
                            }
                            await N(SyntaxKind.CloseBracketToken);
                        }
                        await N(SyntaxKind.DotToken);
                        await N(SyntaxKind.IdentifierToken, "member");
                    }
                    await N(SyntaxKind.ColonToken);
                    await N(SyntaxKind.IdentifierToken, "method");
                    await N(SyntaxKind.TableConstructorFunctionArgument);
                    {
                        await N(SyntaxKind.TableConstructorExpression);
                        {
                            await N(SyntaxKind.OpenBraceToken);
                            await N(SyntaxKind.UnkeyedTableField);
                            {
                                await N(SyntaxKind.StringLiteralExpression);
                                {
                                    await N(SyntaxKind.StringLiteralToken, "'hi'");
                                }
                            }
                            await N(SyntaxKind.CloseBraceToken);
                        }
                    }
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesArrayType_WithSimpleTypeNameElement()
    {
        await UsingTypeAsync("{Type}");

        await N(SyntaxKind.ArrayType);
        {
            await N(SyntaxKind.OpenBraceToken);
            {
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "Type");
                }
            }
            await N(SyntaxKind.CloseBraceToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesArrayType_WithCompositeTypeNameElement()
    {
        await UsingTypeAsync("{Type.Member}");

        await N(SyntaxKind.ArrayType);
        {
            await N(SyntaxKind.OpenBraceToken);
            {
                await N(SyntaxKind.CompositeTypeName);
                {
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "Type");
                    }
                    await N(SyntaxKind.DotToken);
                    await N(SyntaxKind.IdentifierToken, "Member");
                }
            }
            await N(SyntaxKind.CloseBraceToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesArrayType_WithArrayTypeElement()
    {
        await UsingTypeAsync("{{Type}}");

        await N(SyntaxKind.ArrayType);
        {
            await N(SyntaxKind.OpenBraceToken);
            {
                await N(SyntaxKind.ArrayType);
                {
                    await N(SyntaxKind.OpenBraceToken);
                    {
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "Type");
                        }
                    }
                    await N(SyntaxKind.CloseBraceToken);
                }
            }
            await N(SyntaxKind.CloseBraceToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTableType_WithIndexer()
    {
        await UsingTypeAsync("{[Type]: Type}");

        await N(SyntaxKind.TableType);
        {
            await N(SyntaxKind.OpenBraceToken);
            {
                await N(SyntaxKind.TableTypeIndexer);
                {
                    await N(SyntaxKind.OpenBracketToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "Type");
                    }
                    await N(SyntaxKind.CloseBracketToken);
                    await N(SyntaxKind.ColonToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "Type");
                    }
                }
            }
            await N(SyntaxKind.CloseBraceToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeTable_WithProperty()
    {
        await UsingTypeAsync("{prop1: Type1, prop2: Type2, prop3: Type3}");

        await N(SyntaxKind.TableType);
        {
            await N(SyntaxKind.OpenBraceToken);
            await N(SyntaxKind.TableTypeProperty);
            {
                await N(SyntaxKind.IdentifierToken, "prop1");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "Type1");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.TableTypeProperty);
            {
                await N(SyntaxKind.IdentifierToken, "prop2");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "Type2");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.TableTypeProperty);
            {
                await N(SyntaxKind.IdentifierToken, "prop3");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "Type3");
                }
            }
            await N(SyntaxKind.CloseBraceToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFunctionTypes_WithoutTypeParameters_AndNoTrailingVariadicPack_AndTypeReturn()
    {
        await UsingTypeAsync("(T) -> T");

        await N(SyntaxKind.FunctionType);
        {
            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);
            await N(SyntaxKind.MinusGreaterThanToken);
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFunctionTypes_WithoutTypeParameters_AndTrailingVariadicPack_AndTypePackReturn()
    {
        await UsingTypeAsync("(T, ...T) -> (T, ...T)");

        await N(SyntaxKind.FunctionType);
        {
            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.VariadicTypePack);
                {
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);

            await N(SyntaxKind.MinusGreaterThanToken);

            await N(SyntaxKind.TypePack);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.VariadicTypePack);
                {
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFunctionTypes_WithoutTypeParameters_AndTrailingVariadicPack_AndTypePackReturn_AndParameterNameOnFirstParameter()
    {
        await UsingTypeAsync("(p1: T, ...T) -> (T, ...T)");

        await N(SyntaxKind.FunctionType);
        {
            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.IdentifierToken, "p1");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.VariadicTypePack);
                {
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);

            await N(SyntaxKind.MinusGreaterThanToken);

            await N(SyntaxKind.TypePack);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.VariadicTypePack);
                {
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFunctionTypes_WithoutTypeParameters_AndTypePackReturn_AndParameterNameOnFirstParameter()
    {
        await UsingTypeAsync("(p1: T, T) -> (T, ...T)");

        await N(SyntaxKind.FunctionType);
        {
            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.IdentifierToken, "p1");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);

            await N(SyntaxKind.MinusGreaterThanToken);

            await N(SyntaxKind.TypePack);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.VariadicTypePack);
                {
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFunctionTypes_WithoutTypeParameters_AndTypePackReturn_AndParameterNameOnSecondParameter()
    {
        await UsingTypeAsync("(T, p2: T) -> (T, ...T)");

        await N(SyntaxKind.FunctionType);
        {
            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.IdentifierToken, "p2");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);

            await N(SyntaxKind.MinusGreaterThanToken);

            await N(SyntaxKind.TypePack);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.VariadicTypePack);
                {
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFunctionTypes_WithoutTypeParameters_AndTypePackReturn_AndParameterNameOnBothParameters()
    {
        await UsingTypeAsync("(p1: T, p2: T) -> (T, ...T)");

        await N(SyntaxKind.FunctionType);
        {
            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.IdentifierToken, "p1");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.IdentifierToken, "p2");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);

            await N(SyntaxKind.MinusGreaterThanToken);

            await N(SyntaxKind.TypePack);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.VariadicTypePack);
                {
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFunctionType_WithTypeParameters_AndTrailingVariadicPack_AndTypeReturn()
    {
        await UsingTypeAsync("<T, T = T, T... = ...T, T... = T...> (T, ...T) -> T");

        await N(SyntaxKind.FunctionType);
        {
            await N(SyntaxKind.TypeParameterList);
            {
                await N(SyntaxKind.LessThanToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.VariadicTypePack);
                        {
                            await N(SyntaxKind.DotDotDotToken);
                            await N(SyntaxKind.SimpleTypeName);
                            {
                                await N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.GenericTypePack);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                            await N(SyntaxKind.DotDotDotToken);
                        }
                    }
                }
                await N(SyntaxKind.GreaterThanToken);
            }
            // /TypeParameterList

            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                // No name
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
                
            await N(SyntaxKind.FunctionTypeParameter);
            {
                // No name
                await N(SyntaxKind.VariadicTypePack);
                {
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);

            await N(SyntaxKind.MinusGreaterThanToken);

            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFunctionType_WithTypeParameters_AndTrailingVariadicPack_AndTypeReturn_AndParameterNameOnFirstParameter()
    {
        await UsingTypeAsync("<T, T = T, T... = ...T, T... = T...> (p1: T, ...T) -> T");

        await N(SyntaxKind.FunctionType);
        {
            await N(SyntaxKind.TypeParameterList);
            {
                await N(SyntaxKind.LessThanToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.VariadicTypePack);
                        {
                            await N(SyntaxKind.DotDotDotToken);
                            await N(SyntaxKind.SimpleTypeName);
                            {
                                await N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.GenericTypePack);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                            await N(SyntaxKind.DotDotDotToken);
                        }
                    }
                }
                await N(SyntaxKind.GreaterThanToken);
            }
            // /TypeParameterList

            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.IdentifierToken, "p1");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
                
            await N(SyntaxKind.FunctionTypeParameter);
            {
                // No name
                await N(SyntaxKind.VariadicTypePack);
                {
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);

            await N(SyntaxKind.MinusGreaterThanToken);

            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFunctionType_WithTypeParameters_AndTypeReturn_AndParameterNameOnSecondParameter()
    {
        await UsingTypeAsync("<T, T = T, T... = ...T, T... = T...> (T, p2: T) -> T");

        await N(SyntaxKind.FunctionType);
        {
            await N(SyntaxKind.TypeParameterList);
            {
                await N(SyntaxKind.LessThanToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.VariadicTypePack);
                        {
                            await N(SyntaxKind.DotDotDotToken);
                            await N(SyntaxKind.SimpleTypeName);
                            {
                                await N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.GenericTypePack);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                            await N(SyntaxKind.DotDotDotToken);
                        }
                    }
                }
                await N(SyntaxKind.GreaterThanToken);
            }
            // /TypeParameterList

            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
                
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.IdentifierToken, "p2");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);

            await N(SyntaxKind.MinusGreaterThanToken);

            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFunctionType_WithTypeParameters_AndTypeReturn_AndParameterNameOnBothParameters()
    {
        await UsingTypeAsync("<T, T = T, T... = ...T, T... = T...> (p1: T, p2: T) -> T");

        await N(SyntaxKind.FunctionType);
        {
            await N(SyntaxKind.TypeParameterList);
            {
                await N(SyntaxKind.LessThanToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.VariadicTypePack);
                        {
                            await N(SyntaxKind.DotDotDotToken);
                            await N(SyntaxKind.SimpleTypeName);
                            {
                                await N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.EqualsType);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.GenericTypePack);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                            await N(SyntaxKind.DotDotDotToken);
                        }
                    }
                }
                await N(SyntaxKind.GreaterThanToken);
            }
            // /TypeParameterList

            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.IdentifierToken, "p1");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
                
            await N(SyntaxKind.FunctionTypeParameter);
            {
                await N(SyntaxKind.IdentifierToken, "p2");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CloseParenthesisToken);

            await N(SyntaxKind.MinusGreaterThanToken);

            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesStringType()
    {
        await UsingTypeAsync("'value'");

        await N(SyntaxKind.StringType);
        {
            await N(SyntaxKind.StringLiteralToken, "'value'");
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTrueType()
    {
        await UsingTypeAsync("true");

        await N(SyntaxKind.TrueType);
        {
            await N(SyntaxKind.TrueKeyword, "true");
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesFalseType()
    {
        await UsingTypeAsync("false");

        await N(SyntaxKind.FalseType);
        {
            await N(SyntaxKind.FalseKeyword, "false");
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesNilType()
    {
        await UsingTypeAsync("nil");

        await N(SyntaxKind.NilType);
        {
            await N(SyntaxKind.NilKeyword, "nil");
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesParenthesizedTypes()
    {
        await UsingTypeAsync("(T)");

        await N(SyntaxKind.ParenthesizedType);
        {
            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
            await N(SyntaxKind.CloseParenthesisToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesNilableTypes()
    {
        await UsingTypeAsync("{T}?");

        await N(SyntaxKind.NilableType);
        {
            await N(SyntaxKind.ArrayType);
            {
                await N(SyntaxKind.OpenBraceToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CloseBraceToken);
            }
            await N(SyntaxKind.QuestionToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesIntersectionType()
    {
        await UsingTypeAsync("T & T");

        await N(SyntaxKind.IntersectionType);
        {
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
            await N(SyntaxKind.AmpersandToken);
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesUnionType()
    {
        await UsingTypeAsync("T | T");

        await N(SyntaxKind.UnionType);
        {
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
            await N(SyntaxKind.PipeToken);
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesLocalVariableWithTypeBinding()
    {
        await UsingStatementAsync("local Var: T = true");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "Var");
                }
            }
            await N(SyntaxKind.TypeBinding);
            {
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }

            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.TrueLiteralExpression);
                {
                    await N(SyntaxKind.TrueKeyword);
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesNumericForLoop()
    {
        await UsingStatementAsync("for i:T = 1, 5 do end");

        await N(SyntaxKind.NumericForStatement);
        {
            await N(SyntaxKind.ForKeyword);
            await N(SyntaxKind.TypedIdentifierName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "i");
                }
                await N(SyntaxKind.TypeBinding);
                {
                    await N(SyntaxKind.ColonToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
            }
            await N(SyntaxKind.EqualsToken);
            await N(SyntaxKind.NumericalLiteralExpression);
            {
                await N(SyntaxKind.NumericLiteralToken, "1");
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.NumericalLiteralExpression);
            {
                await N(SyntaxKind.NumericLiteralToken, "5");
            }
            await N(SyntaxKind.DoKeyword);
            await M(SyntaxKind.StatementList);
            { }
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesGenericForLoop()
    {
        await UsingStatementAsync("for i:T in iter() do end");

        await N(SyntaxKind.GenericForStatement);
        {
            await N(SyntaxKind.ForKeyword);
            await N(SyntaxKind.TypedIdentifierName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "i");
                }
                await N(SyntaxKind.TypeBinding);
                {
                    await N(SyntaxKind.ColonToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
            }
            await N(SyntaxKind.InKeyword);
            await N(SyntaxKind.FunctionCallExpression);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "iter");
                }
                await N(SyntaxKind.ExpressionListFunctionArgument);
                {
                    await N(SyntaxKind.OpenParenthesisToken);
                    await N(SyntaxKind.CloseParenthesisToken);
                }
            }
            await N(SyntaxKind.DoKeyword);
            await M(SyntaxKind.StatementList);
            { }
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesGenericForLoopWithOccasionalTyping()
    {
        await UsingStatementAsync("for i: T, v in iter() do end");

        await N(SyntaxKind.GenericForStatement);
        {
            await N(SyntaxKind.ForKeyword);
            await N(SyntaxKind.TypedIdentifierName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "i");
                }
                await N(SyntaxKind.TypeBinding);
                {
                    await N(SyntaxKind.ColonToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.TypedIdentifierName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "v");
                }
            }
            await N(SyntaxKind.InKeyword);
            await N(SyntaxKind.FunctionCallExpression);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "iter");
                }
                await N(SyntaxKind.ExpressionListFunctionArgument);
                {
                    await N(SyntaxKind.OpenParenthesisToken);
                    await N(SyntaxKind.CloseParenthesisToken);
                }
            }
            await N(SyntaxKind.DoKeyword);
            await M(SyntaxKind.StatementList);
            { }
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypedNamedParameters()
    {
        await UsingStatementAsync("function a(b:T, c:A) end");

        await N(SyntaxKind.FunctionDeclarationStatement);
        {
            await N(SyntaxKind.FunctionKeyword);
            await N(SyntaxKind.SimpleFunctionName);
            {
                await N(SyntaxKind.IdentifierToken, "a");
            }
            await N(SyntaxKind.ParameterList);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.NamedParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "b");
                    await N(SyntaxKind.TypeBinding);
                    {
                        await N(SyntaxKind.ColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.NamedParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "c");
                    await N(SyntaxKind.TypeBinding);
                    {
                        await N(SyntaxKind.ColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "A");
                        }
                    }
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
            await M(SyntaxKind.StatementList);
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesOccasionallyTypedNamedParameters()
    {
        await UsingStatementAsync("function a(b, c:A) end");

        await N(SyntaxKind.FunctionDeclarationStatement);
        {
            await N(SyntaxKind.FunctionKeyword);
            await N(SyntaxKind.SimpleFunctionName);
            {
                await N(SyntaxKind.IdentifierToken, "a");
            }
            await N(SyntaxKind.ParameterList);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.NamedParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "b");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.NamedParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "c");
                    await N(SyntaxKind.TypeBinding);
                    {
                        await N(SyntaxKind.ColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "A");
                        }
                    }
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
            await M(SyntaxKind.StatementList);
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesNamedParameterAndVararg()
    {
        await UsingStatementAsync("function a(b:T, ...:A) end");

        await N(SyntaxKind.FunctionDeclarationStatement);
        {
            await N(SyntaxKind.FunctionKeyword);
            await N(SyntaxKind.SimpleFunctionName);
            {
                await N(SyntaxKind.IdentifierToken, "a");
            }
            await N(SyntaxKind.ParameterList);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.NamedParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "b");
                    await N(SyntaxKind.TypeBinding);
                    {
                        await N(SyntaxKind.ColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.VarArgParameter);
                {
                    await N(SyntaxKind.DotDotDotToken);
                    await N(SyntaxKind.TypeBinding);
                    {
                        await N(SyntaxKind.ColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "A");
                        }
                    }
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
            await M(SyntaxKind.StatementList);
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesAnonymousFunctionParameters()
    {
        await UsingStatementAsync("local a = function(b:T, c:T) end");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
            }
            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.AnonymousFunctionExpression);
                {
                    await N(SyntaxKind.FunctionKeyword);
                    await N(SyntaxKind.ParameterList);
                    {
                        await N(SyntaxKind.OpenParenthesisToken);
                        await N(SyntaxKind.NamedParameter);
                        {
                            await N(SyntaxKind.IdentifierToken, "b");
                            await N(SyntaxKind.TypeBinding);
                            {
                                await N(SyntaxKind.ColonToken);
                                await N(SyntaxKind.SimpleTypeName);
                                {
                                    await N(SyntaxKind.IdentifierToken, "T");
                                }
                            }
                        }
                        await N(SyntaxKind.CommaToken);
                        await N(SyntaxKind.NamedParameter);
                        {
                            await N(SyntaxKind.IdentifierToken, "c");
                            await N(SyntaxKind.TypeBinding);
                            {
                                await N(SyntaxKind.ColonToken);
                                await N(SyntaxKind.SimpleTypeName);
                                {
                                    await N(SyntaxKind.IdentifierToken, "T");
                                }
                            }
                        }
                        await N(SyntaxKind.CloseParenthesisToken);
                    }
                    await M(SyntaxKind.StatementList);
                    await N(SyntaxKind.EndKeyword);
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesNamedFunctionReturnType()
    {
        await UsingStatementAsync("function a() : T end");

        await N(SyntaxKind.FunctionDeclarationStatement);
        {
            await N(SyntaxKind.FunctionKeyword);
            await N(SyntaxKind.SimpleFunctionName);
            {
                await N(SyntaxKind.IdentifierToken, "a");
            }
            await N(SyntaxKind.ParameterList);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.CloseParenthesisToken);
            }
            await N(SyntaxKind.TypeBinding);
            {
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await M(SyntaxKind.StatementList);
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesAnonymousFunctionReturnType()
    {
        await UsingStatementAsync("local a = function() : T end");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
            }
            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.AnonymousFunctionExpression);
                {
                    await N(SyntaxKind.FunctionKeyword);
                    await N(SyntaxKind.ParameterList);
                    {
                        await N(SyntaxKind.OpenParenthesisToken);
                        await N(SyntaxKind.CloseParenthesisToken);
                    }
                    await N(SyntaxKind.TypeBinding);
                    {
                        await N(SyntaxKind.ColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                    await M(SyntaxKind.StatementList);
                    await N(SyntaxKind.EndKeyword);
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeDeclarationStatement()
    {
        await UsingStatementAsync("type a = T");

        await N(SyntaxKind.TypeDeclarationStatement);
        {
            await N(SyntaxKind.TypeKeyword);
            await N(SyntaxKind.IdentifierToken, "a");
            await N(SyntaxKind.EqualsToken);
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesExportedTypeDeclarationStatement()
    {
        await UsingStatementAsync("export type a = T");

        await N(SyntaxKind.TypeDeclarationStatement);
        {
            await N(SyntaxKind.ExportKeyword);
            await N(SyntaxKind.TypeKeyword);
            await N(SyntaxKind.IdentifierToken, "a");
            await N(SyntaxKind.EqualsToken);
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeCastExpression()
    {
        await UsingStatementAsync("local a = b :: T");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
            }
            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.TypeCastExpression);
                {
                    await N(SyntaxKind.IdentifierName);
                    {
                        await N(SyntaxKind.IdentifierToken, "b");
                    }
                    await N(SyntaxKind.ColonColonToken);
                    await N(SyntaxKind.SimpleTypeName);
                    {
                        await N(SyntaxKind.IdentifierToken, "T");
                    }
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeDeclarationStatementWithAdding()
    {
        await UsingStatementAsync("local a = b :: T + b :: T");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
            }
            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.AddExpression);
                {
                    await N(SyntaxKind.TypeCastExpression);
                    {
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "b");
                        }
                        await N(SyntaxKind.ColonColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                    await N(SyntaxKind.PlusToken);
                    await N(SyntaxKind.TypeCastExpression);
                    {
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "b");
                        }
                        await N(SyntaxKind.ColonColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeDeclarationStatementWithUnary()
    {
        await UsingStatementAsync("local a = -b :: T");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
            }
            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.UnaryMinusExpression);
                {
                    await N(SyntaxKind.MinusToken);
                    await N(SyntaxKind.TypeCastExpression);
                    {
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "b");
                        }
                        await N(SyntaxKind.ColonColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParseTypeDeclarationStatementWithPow()
    {
        await UsingStatementAsync("local a = b ^ b :: T");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
            }
            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.ExponentiateExpression);
                {
                    await N(SyntaxKind.IdentifierName);
                    {
                        await N(SyntaxKind.IdentifierToken, "b");
                    }
                    await N(SyntaxKind.HatToken);
                    await N(SyntaxKind.TypeCastExpression);
                    {
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "b");
                        }
                        await N(SyntaxKind.ColonColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParseEmptyTypePack()
    {
        await UsingStatementAsync("function a(): () end");

        await N(SyntaxKind.FunctionDeclarationStatement);
        {
            await N(SyntaxKind.FunctionKeyword);
            await N(SyntaxKind.SimpleFunctionName);
            {
                await N(SyntaxKind.IdentifierToken, "a");
            }
            await N(SyntaxKind.ParameterList);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.CloseParenthesisToken);
            }
            await N(SyntaxKind.TypeBinding);
            {
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.TypePack);
                {
                    await N(SyntaxKind.OpenParenthesisToken);
                    await N(SyntaxKind.CloseParenthesisToken);
                }
            }
            await M(SyntaxKind.StatementList);
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParseEmptyTypeArgument()
    {
        await UsingStatementAsync("type T = T<>");

        await N(SyntaxKind.TypeDeclarationStatement);
        {
            await N(SyntaxKind.TypeKeyword);
            await N(SyntaxKind.IdentifierToken, "T");
            await N(SyntaxKind.EqualsToken);
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
                await N(SyntaxKind.TypeArgumentList);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeFunctionDeclarationStatement()
    {
        await UsingStatementAsync("type function myTypeFunc() return types.number end");

        await N(SyntaxKind.TypeFunctionDeclarationStatement);
        {
            await N(SyntaxKind.TypeKeyword);
            await N(SyntaxKind.FunctionKeyword);
            await N(SyntaxKind.IdentifierToken, "myTypeFunc");
            await N(SyntaxKind.ParameterList);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.CloseParenthesisToken);
            }
            await N(SyntaxKind.StatementList);
            {
                await N(SyntaxKind.ReturnStatement);
                {
                    await N(SyntaxKind.ReturnKeyword);
                    await N(SyntaxKind.MemberAccessExpression);
                    {
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "types");
                        }
                        await N(SyntaxKind.DotToken);
                        await N(SyntaxKind.IdentifierToken, "number");
                    }
                }
            }
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesExportedTypeFunctionDeclarationStatement()
    {
        await UsingStatementAsync("export type function myTypeFunc() return types.number end");

        await N(SyntaxKind.TypeFunctionDeclarationStatement);
        {
            await N(SyntaxKind.ExportKeyword);
            await N(SyntaxKind.TypeKeyword);
            await N(SyntaxKind.FunctionKeyword);
            await N(SyntaxKind.IdentifierToken, "myTypeFunc");
            await N(SyntaxKind.ParameterList);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.CloseParenthesisToken);
            }
            await N(SyntaxKind.StatementList);
            {
                await N(SyntaxKind.ReturnStatement);
                {
                    await N(SyntaxKind.ReturnKeyword);
                    await N(SyntaxKind.MemberAccessExpression);
                    {
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "types");
                        }
                        await N(SyntaxKind.DotToken);
                        await N(SyntaxKind.IdentifierToken, "number");
                    }
                }
            }
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeFunctionDeclarationStatementWithParameters()
    {
        await UsingStatementAsync("type function serialize(arg) return arg end");

        await N(SyntaxKind.TypeFunctionDeclarationStatement);
        {
            await N(SyntaxKind.TypeKeyword);
            await N(SyntaxKind.FunctionKeyword);
            await N(SyntaxKind.IdentifierToken, "serialize");
            await N(SyntaxKind.ParameterList);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.NamedParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "arg");
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
            await N(SyntaxKind.StatementList);
            {
                await N(SyntaxKind.ReturnStatement);
                {
                    await N(SyntaxKind.ReturnKeyword);
                    await N(SyntaxKind.IdentifierName);
                    {
                        await N(SyntaxKind.IdentifierToken, "arg");
                    }
                }
            }
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeFunctionDeclarationStatementWithReturnType()
    {
        await UsingStatementAsync("type function myTypeFunc(): T return types.number end");

        await N(SyntaxKind.TypeFunctionDeclarationStatement);
        {
            await N(SyntaxKind.TypeKeyword);
            await N(SyntaxKind.FunctionKeyword);
            await N(SyntaxKind.IdentifierToken, "myTypeFunc");
            await N(SyntaxKind.ParameterList);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.CloseParenthesisToken);
            }
            await N(SyntaxKind.TypeBinding);
            {
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.StatementList);
            {
                await N(SyntaxKind.ReturnStatement);
                {
                    await N(SyntaxKind.ReturnKeyword);
                    await N(SyntaxKind.MemberAccessExpression);
                    {
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "types");
                        }
                        await N(SyntaxKind.DotToken);
                        await N(SyntaxKind.IdentifierToken, "number");
                    }
                }
            }
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesTypeFunctionDeclarationStatementWithTypedParameters()
    {
        await UsingStatementAsync("type function serialize(arg: T) return arg end");

        await N(SyntaxKind.TypeFunctionDeclarationStatement);
        {
            await N(SyntaxKind.TypeKeyword);
            await N(SyntaxKind.FunctionKeyword);
            await N(SyntaxKind.IdentifierToken, "serialize");
            await N(SyntaxKind.ParameterList);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.NamedParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "arg");
                    await N(SyntaxKind.TypeBinding);
                    {
                        await N(SyntaxKind.ColonToken);
                        await N(SyntaxKind.SimpleTypeName);
                        {
                            await N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
            await N(SyntaxKind.StatementList);
            {
                await N(SyntaxKind.ReturnStatement);
                {
                    await N(SyntaxKind.ReturnKeyword);
                    await N(SyntaxKind.IdentifierName);
                    {
                        await N(SyntaxKind.IdentifierToken, "arg");
                    }
                }
            }
            await N(SyntaxKind.EndKeyword);
        }
        EOF();
    }
}
