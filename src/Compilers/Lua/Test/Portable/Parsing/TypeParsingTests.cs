using Xunit;
using Xunit.Abstractions;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing
{
    public class TypeParsingTests : ParsingTestsBase
    {
        private const string TypeArgumentListString = "<Type, Type..., ...Type, Type.Member>";

        public TypeParsingTests(ITestOutputHelper output) : base(output)
        {
        }

        private void CheckTypeArgumentList()
        {
            N(SyntaxKind.TypeArgumentList);
            {
                N(SyntaxKind.LessThanToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "Type");
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.GenericTypePack);
                {
                    N(SyntaxKind.IdentifierToken, "Type");
                    N(SyntaxKind.DotDotDotToken);
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.VariadicTypePack);
                {
                    N(SyntaxKind.DotDotDotToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "Type");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.CompositeTypeName);
                {
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "Type");
                    }
                    N(SyntaxKind.DotToken);
                    N(SyntaxKind.IdentifierToken, "Member");
                }
                N(SyntaxKind.GreaterThanToken);
            }
        }

        [Fact]
        public void Parser_ParsesSimpleTypeName()
        {
            UsingType("Type");

            N(SyntaxKind.SimpleTypeName);
            {
                N(SyntaxKind.IdentifierToken, "Type");
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesSimpleTypeName_WithTypeArgumentList()
        {
            UsingType($"Type{TypeArgumentListString}");

            N(SyntaxKind.SimpleTypeName);
            {
                N(SyntaxKind.IdentifierToken, "Type");
                CheckTypeArgumentList();
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesCompositeTypeName()
        {
            UsingType("Type.Member");

            N(SyntaxKind.CompositeTypeName);
            {
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "Type");
                }
                N(SyntaxKind.DotToken);
                N(SyntaxKind.IdentifierToken, "Member");
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesCompositeTypeName_WithTypeArgumentList()
        {
            UsingType($"Type.Member{TypeArgumentListString}");

            N(SyntaxKind.CompositeTypeName);
            {
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "Type");
                }
                N(SyntaxKind.DotToken);
                N(SyntaxKind.IdentifierToken, "Member");
                CheckTypeArgumentList();
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTypeofType_WithStrings()
        {
            UsingType("typeof('hi')");

            N(SyntaxKind.TypeofType);
            {
                N(SyntaxKind.TypeofKeyword);
                N(SyntaxKind.OpenParenthesisToken);
                {
                    N(SyntaxKind.StringLiteralExpression);
                    {
                        N(SyntaxKind.StringLiteralToken, "'hi'");
                    }
                }
                N(SyntaxKind.CloseParenthesisToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTypeofType_WithNumbers()
        {
            UsingType("typeof(1)");

            N(SyntaxKind.TypeofType);
            {
                N(SyntaxKind.TypeofKeyword);
                N(SyntaxKind.OpenParenthesisToken);
                {
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "1");
                    }
                }
                N(SyntaxKind.CloseParenthesisToken);
            }
        }

        [Fact]
        public void Parser_ParsesTyepofType_WithTables()
        {
            UsingType("typeof({ 1 })");

            N(SyntaxKind.TypeofType);
            {
                N(SyntaxKind.TypeofKeyword);
                N(SyntaxKind.OpenParenthesisToken);
                {
                    N(SyntaxKind.TableConstructorExpression);
                    N(SyntaxKind.OpenBraceToken);
                    {
                        N(SyntaxKind.UnkeyedTableField);
                        {
                            N(SyntaxKind.NumericalLiteralExpression);
                            {
                                N(SyntaxKind.NumericLiteralToken, "1");
                            }
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.CloseParenthesisToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTypeofType_WithComplexExpression()
        {
            UsingType("typeof(tbl[1].member:method { 'hi' })");

            N(SyntaxKind.TypeofType);
            {
                N(SyntaxKind.TypeofKeyword);
                N(SyntaxKind.OpenParenthesisToken);
                {
                    N(SyntaxKind.MethodCallExpression);
                    {
                        N(SyntaxKind.MemberAccessExpression);
                        {
                            N(SyntaxKind.ElementAccessExpression);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "tbl");
                                }
                                N(SyntaxKind.OpenBracketToken);
                                N(SyntaxKind.NumericalLiteralExpression);
                                {
                                    N(SyntaxKind.NumericLiteralToken, "1");
                                }
                                N(SyntaxKind.CloseBracketToken);
                            }
                            N(SyntaxKind.DotToken);
                            N(SyntaxKind.IdentifierToken, "member");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.IdentifierToken, "method");
                        N(SyntaxKind.TableConstructorFunctionArgument);
                        {
                            N(SyntaxKind.TableConstructorExpression);
                            {
                                N(SyntaxKind.OpenBraceToken);
                                N(SyntaxKind.UnkeyedTableField);
                                {
                                    N(SyntaxKind.StringLiteralExpression);
                                    {
                                        N(SyntaxKind.StringLiteralToken, "'hi'");
                                    }
                                }
                                N(SyntaxKind.CloseBraceToken);
                            }
                        }
                    }
                }
                N(SyntaxKind.CloseParenthesisToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesArrayType_WithSimpleTypeNameElement()
        {
            UsingType("{Type}");

            N(SyntaxKind.ArrayType);
            {
                N(SyntaxKind.OpenBraceToken);
                {
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "Type");
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesArrayType_WithCompositeTypeNameElement()
        {
            UsingType("{Type.Member}");

            N(SyntaxKind.ArrayType);
            {
                N(SyntaxKind.OpenBraceToken);
                {
                    N(SyntaxKind.CompositeTypeName);
                    {
                        N(SyntaxKind.SimpleTypeName);
                        {
                            N(SyntaxKind.IdentifierToken, "Type");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierToken, "Member");
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesArrayType_WithArrayTypeElement()
        {
            UsingType("{{Type}}");

            N(SyntaxKind.ArrayType);
            {
                N(SyntaxKind.OpenBraceToken);
                {
                    N(SyntaxKind.ArrayType);
                    {
                        N(SyntaxKind.OpenBraceToken);
                        {
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "Type");
                            }
                        }
                        N(SyntaxKind.CloseBraceToken);
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTableType_WithIndexer()
        {
            UsingType("{[Type]: Type}");

            N(SyntaxKind.TableType);
            {
                N(SyntaxKind.OpenBraceToken);
                {
                    N(SyntaxKind.TableTypeIndexer);
                    {
                        N(SyntaxKind.OpenBracketToken);
                        N(SyntaxKind.SimpleTypeName);
                        {
                            N(SyntaxKind.IdentifierToken, "Type");
                        }
                        N(SyntaxKind.CloseBracketToken);
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.SimpleTypeName);
                        {
                            N(SyntaxKind.IdentifierToken, "Type");
                        }
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTypeTable_WithProperty()
        {
            UsingType("{prop1: Type1, prop2: Type2, prop3: Type3}");

            N(SyntaxKind.TableType);
            {
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.TableTypeProperty);
                {
                    N(SyntaxKind.IdentifierToken, "prop1");
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "Type1");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.TableTypeProperty);
                {
                    N(SyntaxKind.IdentifierToken, "prop2");
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "Type2");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.TableTypeProperty);
                {
                    N(SyntaxKind.IdentifierToken, "prop3");
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "Type3");
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesFunctionTypes_WithoutTypeParameters_AndNoTrailingVariadicPack_AndTypeReturn()
        {
            UsingType("(T) -> T");

            N(SyntaxKind.FunctionType);
            {
                N(SyntaxKind.OpenParenthesisToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
                N(SyntaxKind.CloseParenthesisToken);
                N(SyntaxKind.SlimArrowToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesFunctionTypes_WithoutTypeParameters_AndTrailingVariadicPack_AndTypePackReturn()
        {
            UsingType("(T, ...T) -> (T, ...T)");

            N(SyntaxKind.FunctionType);
            {
                N(SyntaxKind.OpenParenthesisToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.VariadicTypePack);
                {
                    N(SyntaxKind.DotDotDotToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                N(SyntaxKind.CloseParenthesisToken);

                N(SyntaxKind.SlimArrowToken);

                N(SyntaxKind.TypePack);
                {
                    N(SyntaxKind.OpenParenthesisToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.VariadicTypePack);
                    {
                        N(SyntaxKind.DotDotDotToken);
                        N(SyntaxKind.SimpleTypeName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                    N(SyntaxKind.CloseParenthesisToken);
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesFunctionType_WithTypeParameters_AndTrailingVariadicPack_AndTypeReturn()
        {
            UsingType("<T, T = T, T... = ...T, T... = T...> (T, ...T) -> T");

            N(SyntaxKind.FunctionType);
            {
                N(SyntaxKind.TypeParameterList);
                {
                    N(SyntaxKind.LessThanToken);
                    N(SyntaxKind.TypeParameter);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.TypeParameter);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                        N(SyntaxKind.EqualsType);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.TypeParameter);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                        N(SyntaxKind.DotDotDotToken);
                        N(SyntaxKind.EqualsType);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.VariadicTypePack);
                            {
                                N(SyntaxKind.DotDotDotToken);
                                N(SyntaxKind.SimpleTypeName);
                                {
                                    N(SyntaxKind.IdentifierToken, "T");
                                }
                            }
                        }
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.TypeParameter);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                        N(SyntaxKind.DotDotDotToken);
                        N(SyntaxKind.EqualsType);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.GenericTypePack);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                                N(SyntaxKind.DotDotDotToken);
                            }
                        }
                    }
                    N(SyntaxKind.GreaterThanToken);
                }
                // /TypeParameterList

                N(SyntaxKind.OpenParenthesisToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.VariadicTypePack);
                {
                    N(SyntaxKind.DotDotDotToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                N(SyntaxKind.CloseParenthesisToken);

                N(SyntaxKind.SlimArrowToken);

                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesStringType()
        {
            UsingType("'value'");

            N(SyntaxKind.StringType);
            {
                N(SyntaxKind.StringLiteralToken, "'value'");
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTrueType()
        {
            UsingType("true");

            N(SyntaxKind.TrueType);
            {
                N(SyntaxKind.TrueKeyword, "true");
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesFalseType()
        {
            UsingType("false");

            N(SyntaxKind.FalseType);
            {
                N(SyntaxKind.FalseKeyword, "false");
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesNilType()
        {
            UsingType("nil");

            N(SyntaxKind.NilType);
            {
                N(SyntaxKind.NilKeyword, "nil");
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesParenthesizedTypes()
        {
            UsingType("(T)");

            N(SyntaxKind.ParenthesizedType);
            {
                N(SyntaxKind.OpenParenthesisToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
                N(SyntaxKind.CloseParenthesisToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesNilableTypes()
        {
            UsingType("{T}?");

            N(SyntaxKind.NilableType);
            {
                N(SyntaxKind.ArrayType);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.QuestionToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesIntersectionType()
        {
            UsingType("T & T");

            N(SyntaxKind.IntersectionType);
            {
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
                N(SyntaxKind.AmpersandToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesUnionType()
        {
            UsingType("T | T");

            N(SyntaxKind.UnionType);
            {
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
                N(SyntaxKind.PipeToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
            }
            EOF();
        }
    }
}
