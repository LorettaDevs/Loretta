using Xunit;
using Xunit.Abstractions;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing
{
    public class TypeParsingTests(ITestOutputHelper output) : ParsingTestsBase(output)
    {
        private const string TypeArgumentListString = "<Type, Type..., ...Type, Type.Member>";

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
                N(SyntaxKind.MinusGreaterThanToken);
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

                N(SyntaxKind.MinusGreaterThanToken);

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

                N(SyntaxKind.MinusGreaterThanToken);

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

        [Fact]
        public void Parser_ParsesLocalVariableWithTypeBinding()
        {
            UsingStatement("local Var: T = true");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "Var");
                    }
                }
                N(SyntaxKind.TypeBinding);
                {
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }

                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.TrueLiteralExpression);
                    {
                        N(SyntaxKind.TrueKeyword);
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesNumericForLoop()
        {
            UsingStatement("for i:T = 1, 5 do end");

            N(SyntaxKind.NumericForStatement);
            {
                N(SyntaxKind.ForKeyword);
                N(SyntaxKind.TypedIdentifierName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "i");
                    }
                    N(SyntaxKind.TypeBinding);
                    {
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.SimpleTypeName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                N(SyntaxKind.EqualsToken);
                N(SyntaxKind.NumericalLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "1");
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.NumericalLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "5");
                }
                N(SyntaxKind.DoKeyword);
                M(SyntaxKind.StatementList);
                { }
                N(SyntaxKind.EndKeyword);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesGenericForLoop()
        {
            UsingStatement("for i:T in iter() do end");

            N(SyntaxKind.GenericForStatement);
            {
                N(SyntaxKind.ForKeyword);
                N(SyntaxKind.TypedIdentifierName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "i");
                    }
                    N(SyntaxKind.TypeBinding);
                    {
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.SimpleTypeName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.FunctionCallExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "iter");
                    }
                    N(SyntaxKind.ExpressionListFunctionArgument);
                    {
                        N(SyntaxKind.OpenParenthesisToken);
                        N(SyntaxKind.CloseParenthesisToken);
                    }
                }
                N(SyntaxKind.DoKeyword);
                M(SyntaxKind.StatementList);
                { }
                N(SyntaxKind.EndKeyword);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesGenericForLoopWithOccasionalTyping()
        {
            UsingStatement("for i: T, v in iter() do end");

            N(SyntaxKind.GenericForStatement);
            {
                N(SyntaxKind.ForKeyword);
                N(SyntaxKind.TypedIdentifierName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "i");
                    }
                    N(SyntaxKind.TypeBinding);
                    {
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.SimpleTypeName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.TypedIdentifierName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "v");
                    }
                }
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.FunctionCallExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "iter");
                    }
                    N(SyntaxKind.ExpressionListFunctionArgument);
                    {
                        N(SyntaxKind.OpenParenthesisToken);
                        N(SyntaxKind.CloseParenthesisToken);
                    }
                }
                N(SyntaxKind.DoKeyword);
                M(SyntaxKind.StatementList);
                { }
                N(SyntaxKind.EndKeyword);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTypedNamedParameters()
        {
            UsingStatement("function a(b:T, c:A) end");

            N(SyntaxKind.FunctionDeclarationStatement);
            {
                N(SyntaxKind.FunctionKeyword);
                N(SyntaxKind.SimpleFunctionName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenthesisToken);
                    N(SyntaxKind.NamedParameter);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                        N(SyntaxKind.TypeBinding);
                        {
                            N(SyntaxKind.ColonToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.NamedParameter);
                    {
                        N(SyntaxKind.IdentifierToken, "c");
                        N(SyntaxKind.TypeBinding);
                        {
                            N(SyntaxKind.ColonToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "A");
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenthesisToken);
                }
                M(SyntaxKind.StatementList);
                N(SyntaxKind.EndKeyword);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesOccasionallyTypedNamedParameters()
        {
            UsingStatement("function a(b, c:A) end");

            N(SyntaxKind.FunctionDeclarationStatement);
            {
                N(SyntaxKind.FunctionKeyword);
                N(SyntaxKind.SimpleFunctionName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenthesisToken);
                    N(SyntaxKind.NamedParameter);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.NamedParameter);
                    {
                        N(SyntaxKind.IdentifierToken, "c");
                        N(SyntaxKind.TypeBinding);
                        {
                            N(SyntaxKind.ColonToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "A");
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenthesisToken);
                }
                M(SyntaxKind.StatementList);
                N(SyntaxKind.EndKeyword);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesNamedParameterAndVararg()
        {
            UsingStatement("function a(b:T, ...:A) end");

            N(SyntaxKind.FunctionDeclarationStatement);
            {
                N(SyntaxKind.FunctionKeyword);
                N(SyntaxKind.SimpleFunctionName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenthesisToken);
                    N(SyntaxKind.NamedParameter);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                        N(SyntaxKind.TypeBinding);
                        {
                            N(SyntaxKind.ColonToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.VarArgParameter);
                    {
                        N(SyntaxKind.DotDotDotToken);
                        N(SyntaxKind.TypeBinding);
                        {
                            N(SyntaxKind.ColonToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "A");
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenthesisToken);
                }
                M(SyntaxKind.StatementList);
                N(SyntaxKind.EndKeyword);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesAnonymousFunctionParameters()
        {
            UsingStatement("local a = function(b:T, c:T) end");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                }
                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.AnonymousFunctionExpression);
                    {
                        N(SyntaxKind.FunctionKeyword);
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenthesisToken);
                            N(SyntaxKind.NamedParameter);
                            {
                                N(SyntaxKind.IdentifierToken, "b");
                                N(SyntaxKind.TypeBinding);
                                {
                                    N(SyntaxKind.ColonToken);
                                    N(SyntaxKind.SimpleTypeName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "T");
                                    }
                                }
                            }
                            N(SyntaxKind.CommaToken);
                            N(SyntaxKind.NamedParameter);
                            {
                                N(SyntaxKind.IdentifierToken, "c");
                                N(SyntaxKind.TypeBinding);
                                {
                                    N(SyntaxKind.ColonToken);
                                    N(SyntaxKind.SimpleTypeName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "T");
                                    }
                                }
                            }
                            N(SyntaxKind.CloseParenthesisToken);
                        }
                        M(SyntaxKind.StatementList);
                        N(SyntaxKind.EndKeyword);
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesNamedFunctionReturnType()
        {
            UsingStatement("function a() : T end");

            N(SyntaxKind.FunctionDeclarationStatement);
            {
                N(SyntaxKind.FunctionKeyword);
                N(SyntaxKind.SimpleFunctionName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenthesisToken);
                    N(SyntaxKind.CloseParenthesisToken);
                }
                N(SyntaxKind.TypeBinding);
                {
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                M(SyntaxKind.StatementList);
                N(SyntaxKind.EndKeyword);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesAnonymousFunctionReturnType()
        {
            UsingStatement("local a = function() : T end");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                }
                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.AnonymousFunctionExpression);
                    {
                        N(SyntaxKind.FunctionKeyword);
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenthesisToken);
                            N(SyntaxKind.CloseParenthesisToken);
                        }
                        N(SyntaxKind.TypeBinding);
                        {
                            N(SyntaxKind.ColonToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                        M(SyntaxKind.StatementList);
                        N(SyntaxKind.EndKeyword);
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTypeDeclarationStatement()
        {
            UsingStatement("type a = T");

            N(SyntaxKind.TypeDeclarationStatement);
            {
                N(SyntaxKind.TypeKeyword);
                N(SyntaxKind.IdentifierToken, "a");
                N(SyntaxKind.EqualsToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesExportedTypeDeclarationStatement()
        {
            UsingStatement("export type a = T");

            N(SyntaxKind.TypeDeclarationStatement);
            {
                N(SyntaxKind.ExportKeyword);
                N(SyntaxKind.TypeKeyword);
                N(SyntaxKind.IdentifierToken, "a");
                N(SyntaxKind.EqualsToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTypeCastExpression()
        {
            UsingStatement("local a = b :: T");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                }
                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.TypeCastExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                        N(SyntaxKind.ColonColonToken);
                        N(SyntaxKind.SimpleTypeName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTypeDeclaratioStatementWithAdding()
        {
            UsingStatement("local a = b :: T + b :: T");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                }
                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.AddExpression);
                    {
                        N(SyntaxKind.TypeCastExpression);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "b");
                            }
                            N(SyntaxKind.ColonColonToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                        N(SyntaxKind.PlusToken);
                        N(SyntaxKind.TypeCastExpression);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "b");
                            }
                            N(SyntaxKind.ColonColonToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesTypeDeclaratioStatementWithUnary()
        {
            UsingStatement("local a = -b :: T");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                }
                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.UnaryMinusExpression);
                    {
                        N(SyntaxKind.MinusToken);
                        N(SyntaxKind.TypeCastExpression);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "b");
                            }
                            N(SyntaxKind.ColonColonToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParseTypeDeclarationStatementWithPow()
        {
            UsingStatement("local a = b ^ b :: T");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                }
                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.ExponentiateExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                        N(SyntaxKind.HatToken);
                        N(SyntaxKind.TypeCastExpression);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "b");
                            }
                            N(SyntaxKind.ColonColonToken);
                            N(SyntaxKind.SimpleTypeName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParseEmptyTypePack()
        {
            UsingStatement("function a(): () end");

            N(SyntaxKind.FunctionDeclarationStatement);
            {
                N(SyntaxKind.FunctionKeyword);
                N(SyntaxKind.SimpleFunctionName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenthesisToken);
                    N(SyntaxKind.CloseParenthesisToken);
                }
                N(SyntaxKind.TypeBinding);
                {
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.TypePack);
                    {
                        N(SyntaxKind.OpenParenthesisToken);
                        N(SyntaxKind.CloseParenthesisToken);
                    }
                }
                M(SyntaxKind.StatementList);
                N(SyntaxKind.EndKeyword);
            }
            EOF();
        }

        [Fact]
        public void Parser_ParseEmptyTypeArgument()
        {
            UsingStatement("type T = T<>");

            N(SyntaxKind.TypeDeclarationStatement);
            {
                N(SyntaxKind.TypeKeyword);
                N(SyntaxKind.IdentifierToken, "T");
                N(SyntaxKind.EqualsToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                    N(SyntaxKind.TypeArgumentList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
            }
            EOF();
        }
    }
}
