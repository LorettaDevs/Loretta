using Xunit;
using Xunit.Abstractions;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing
{
    public class TypeParsingErrorTests : ParsingTestsBase
    {
        public TypeParsingErrorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Parser_ParsesTableType_WithMultipleIndexers_ButErrors()
        {
            UsingType(
                "{[Type]: Type, [Type]: Type}",
                // (1,16): error LUA1017: Only one indexer is allowed per table type
                // {[Type]: Type, [Type]: Type}
                Diagnostic(ErrorCode.ERR_OnlyOneTableTypeIndexerIsAllowed, "[Type]: Type").WithLocation(1, 16));

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
                    N(SyntaxKind.CommaToken);
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
        public void Parser_DoesNotIdentifyDoubleIndexersNaively()
        {
            UsingType(
                "{prop: T, [T]: T, prop: T, prop: T, [T]: T}",
                // (1,37): error LUA1017: Only one indexer is allowed per table type
                // {prop: T, [T]: T, prop: T, prop: T, [T]: T}
                Diagnostic(ErrorCode.ERR_OnlyOneTableTypeIndexerIsAllowed, "[T]: T").WithLocation(1, 37));

            N(SyntaxKind.TableType);
            {
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.TableTypeProperty);
                {
                    N(SyntaxKind.IdentifierToken, "prop");
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.TableTypeIndexer);
                {
                    N(SyntaxKind.OpenBracketToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.CloseBracketToken);
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.TableTypeProperty);
                {
                    N(SyntaxKind.IdentifierToken, "prop");
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.TableTypeProperty);
                {
                    N(SyntaxKind.IdentifierToken, "prop");
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.TableTypeIndexer);
                {
                    N(SyntaxKind.OpenBracketToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.CloseBracketToken);
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ErrorsOnMixingOfNilableAndIntersectionTypes()
        {
            UsingType(
                "T? & T",
                // (1,1): error LUA1014: Using nilable types directly in intersections is not allowed
                // T? & T
                Diagnostic(ErrorCode.ERR_MixingNilableAndIntersectionNotAllowed, "T? & T").WithLocation(1, 1));

            N(SyntaxKind.IntersectionType);
            {
                N(SyntaxKind.NilableType);
                {
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.QuestionToken);
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
        public void Parser_ErrorsOnMixingOfIntersectionAndUnionTypes()
        {
            UsingType(
                "T | T & T",
                // (1,1): error LUA1015: Mixing union and intersection types is not allowed
                // T | T & T
                Diagnostic(ErrorCode.ERR_MixingUnionsAndIntersectionsNotAllowed, "T | T & T").WithLocation(1, 1));

            N(SyntaxKind.IntersectionType);
            {
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
                N(SyntaxKind.AmpersandToken);
                N(SyntaxKind.SimpleTypeName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
            }
        }

        [Fact]
        public void Parser_ErrorsOnMixingOfNilableAndIntersectionTypes_AsWellAsNilableAndIntersectionTypes()
        {
            UsingType(
                "T | T & T?",
                // (1,1): error LUA1014: Using nilable types directly in intersections is not allowed
                // T | T & T?
                Diagnostic(ErrorCode.ERR_MixingNilableAndIntersectionNotAllowed, "T | T & T?").WithLocation(1, 1),
                // (1,1): error LUA1015: Mixing union and intersection types is not allowed
                // T | T & T?
                Diagnostic(ErrorCode.ERR_MixingUnionsAndIntersectionsNotAllowed, "T | T & T?").WithLocation(1, 1));

            N(SyntaxKind.IntersectionType);
            {
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
                N(SyntaxKind.AmpersandToken);
                N(SyntaxKind.NilableType);
                {
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.QuestionToken);
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ErrorsOnTypeParametersAfterTypePackParameters()
        {
            UsingType(
                "<T, T..., T> () -> nil",
                // (1,11): error LUA1018: Normal type parameters must come before pack type parameters
                // <T, T..., T> () -> nil
                Diagnostic(ErrorCode.ERR_NormalTypeParametersComeBeforePacks, "T").WithLocation(1, 11));

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
                        N(SyntaxKind.DotDotDotToken);
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.TypeParameter);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.GreaterThanToken);
                }
                // /TypeParameterList

                N(SyntaxKind.OpenParenthesisToken);
                N(SyntaxKind.CloseParenthesisToken);

                N(SyntaxKind.MinusGreaterThanToken);

                N(SyntaxKind.NilType);
                {
                    N(SyntaxKind.NilKeyword);
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ErrorsOnMultipleIndexers()
        {
            UsingType(
                "{[T]: T, [T]: T}",
                // (1,10): error LUA1017: Only one indexer is allowed per table type
                // {[T]: T, [T]: T}
                Diagnostic(ErrorCode.ERR_OnlyOneTableTypeIndexerIsAllowed, "[T]: T").WithLocation(1, 10));

            N(SyntaxKind.TableType);
            {
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.TableTypeIndexer);
                {
                    N(SyntaxKind.OpenBracketToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.CloseBracketToken);
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.TableTypeIndexer);
                {
                    N(SyntaxKind.OpenBracketToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.CloseBracketToken);
                    N(SyntaxKind.ColonToken);
                    N(SyntaxKind.SimpleTypeName);
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void Parser_ErrorsWhenAcceptTypedLuaIsFalse_AndTypedLuaStructuresAreFound()
        {
            var options = LuaSyntaxOptions.All.With(acceptTypedLua: false);
            ParseAndValidate(
                """
                type T = T
                export type T = T
                local x: T = 1 :: T
                local x = function<T>(p: T, ...: T): T end
                local function x<T>(p: T, ...: T): T end
                function x<T>(p: T, ...: T): T end
                """,
                options,
                // (1,1): error LUA1016: Typed lua is not supported in this lua version
                // type T = T
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, "type T = T").WithLocation(1, 1),
                // (2,1): error LUA1016: Typed lua is not supported in this lua version
                // export type T = T
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, "export type T = T").WithLocation(2, 1),
                // (3,8): error LUA1016: Typed lua is not supported in this lua version
                // local x: T = 1 :: T
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, ": T").WithLocation(3, 8),
                // (3,14): error LUA1016: Typed lua is not supported in this lua version
                // local x: T = 1 :: T
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, "1 :: T").WithLocation(3, 14),
                // (4,11): error LUA1011: Invalid expression part 'function'
                // local x = function<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_InvalidExpressionPart, "function").WithArguments("function").WithLocation(4, 11),
                // (4,19): error LUA1001: Identifier expected
                // local x = function<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "<").WithLocation(4, 19),
                // (4,19): error LUA1016: Typed lua is not supported in this lua version
                // local x = function<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, "<T>").WithLocation(4, 19),
                // (4,24): error LUA1016: Typed lua is not supported in this lua version
                // local x = function<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, ": T").WithLocation(4, 24),
                // (4,32): error LUA1016: Typed lua is not supported in this lua version
                // local x = function<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, ": T").WithLocation(4, 32),
                // (4,36): error LUA1016: Typed lua is not supported in this lua version
                // local x = function<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, ": T").WithLocation(4, 36),
                // (5,17): error LUA1016: Typed lua is not supported in this lua version
                // local function x<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, "<T>").WithLocation(5, 17),
                // (5,22): error LUA1016: Typed lua is not supported in this lua version
                // local function x<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, ": T").WithLocation(5, 22),
                // (5,30): error LUA1016: Typed lua is not supported in this lua version
                // local function x<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, ": T").WithLocation(5, 30),
                // (5,34): error LUA1016: Typed lua is not supported in this lua version
                // local function x<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, ": T").WithLocation(5, 34),
                // (6,11): error LUA1016: Typed lua is not supported in this lua version
                // function x<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, "<T>").WithLocation(6, 11),
                // (6,16): error LUA1016: Typed lua is not supported in this lua version
                // function x<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, ": T").WithLocation(6, 16),
                // (6,24): error LUA1016: Typed lua is not supported in this lua version
                // function x<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, ": T").WithLocation(6, 24),
                // (6,28): error LUA1016: Typed lua is not supported in this lua version
                // function x<T>(p: T, ...: T): T end
                Diagnostic(ErrorCode.ERR_TypedLuaNotSupportedInLuaVersion, ": T").WithLocation(6, 28));
        }
    }
}
