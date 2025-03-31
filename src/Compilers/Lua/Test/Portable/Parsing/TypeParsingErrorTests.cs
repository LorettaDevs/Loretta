
namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing;

public sealed class TypeParsingErrorTests : ParsingTestsBase
{
    [Test]
    public async Task Parser_ParsesTableType_WithMultipleIndexers_ButErrors()
    {
        await UsingTypeAsync(
            "{[Type]: Type, [Type]: Type}",
            // (1,16): error LUA1017: Only one indexer is allowed per table type
            // {[Type]: Type, [Type]: Type}
            Diagnostic(ErrorCode.ERR_OnlyOneTableTypeIndexerIsAllowed, "[Type]: Type").WithLocation(1, 16));

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
                await N(SyntaxKind.CommaToken);
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
    public async Task Parser_DoesNotIdentifyDoubleIndexersNaively()
    {
        await UsingTypeAsync(
            "{prop: T, [T]: T, prop: T, prop: T, [T]: T}",
            // (1,37): error LUA1017: Only one indexer is allowed per table type
            // {prop: T, [T]: T, prop: T, prop: T, [T]: T}
            Diagnostic(ErrorCode.ERR_OnlyOneTableTypeIndexerIsAllowed, "[T]: T").WithLocation(1, 37));

        await N(SyntaxKind.TableType);
        {
            await N(SyntaxKind.OpenBraceToken);
            await N(SyntaxKind.TableTypeProperty);
            {
                await N(SyntaxKind.IdentifierToken, "prop");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.TableTypeIndexer);
            {
                await N(SyntaxKind.OpenBracketToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CloseBracketToken);
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.TableTypeProperty);
            {
                await N(SyntaxKind.IdentifierToken, "prop");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.TableTypeProperty);
            {
                await N(SyntaxKind.IdentifierToken, "prop");
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.TableTypeIndexer);
            {
                await N(SyntaxKind.OpenBracketToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CloseBracketToken);
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CloseBraceToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ErrorsOnMixingOfNilableAndIntersectionTypes()
    {
        await UsingTypeAsync(
            "T? & T",
            // (1,1): error LUA1014: Using nilable types directly in intersections is not allowed
            // T? & T
            Diagnostic(ErrorCode.ERR_MixingNilableAndIntersectionNotAllowed, "T? & T").WithLocation(1, 1));

        await N(SyntaxKind.IntersectionType);
        {
            await N(SyntaxKind.NilableType);
            {
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.QuestionToken);
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
    public async Task Parser_ErrorsOnMixingOfIntersectionAndUnionTypes()
    {
        await UsingTypeAsync(
            "T | T & T",
            // (1,1): error LUA1015: Mixing union and intersection types is not allowed
            // T | T & T
            Diagnostic(ErrorCode.ERR_MixingUnionsAndIntersectionsNotAllowed, "T | T & T").WithLocation(1, 1));

        await N(SyntaxKind.IntersectionType);
        {
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
            await N(SyntaxKind.AmpersandToken);
            await N(SyntaxKind.SimpleTypeName);
            {
                await N(SyntaxKind.IdentifierToken, "T");
            }
        }
    }

    [Test]
    public async Task Parser_ErrorsOnMixingOfNilableAndIntersectionTypes_AsWellAsNilableAndIntersectionTypes()
    {
        await UsingTypeAsync(
            "T | T & T?",
            // (1,1): error LUA1014: Using nilable types directly in intersections is not allowed
            // T | T & T?
            Diagnostic(ErrorCode.ERR_MixingNilableAndIntersectionNotAllowed, "T | T & T?").WithLocation(1, 1),
            // (1,1): error LUA1015: Mixing union and intersection types is not allowed
            // T | T & T?
            Diagnostic(ErrorCode.ERR_MixingUnionsAndIntersectionsNotAllowed, "T | T & T?").WithLocation(1, 1));

        await N(SyntaxKind.IntersectionType);
        {
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
            await N(SyntaxKind.AmpersandToken);
            await N(SyntaxKind.NilableType);
            {
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.QuestionToken);
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ErrorsOnTypeParametersAfterTypePackParameters()
    {
        await UsingTypeAsync(
            "<T, T..., T> () -> nil",
            // (1,11): error LUA1018: Normal type parameters must come before pack type parameters
            // <T, T..., T> () -> nil
            Diagnostic(ErrorCode.ERR_NormalTypeParametersComeBeforePacks, "T").WithLocation(1, 11));

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
                    await N(SyntaxKind.DotDotDotToken);
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.TypeParameter);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.GreaterThanToken);
            }
            // /TypeParameterList

            await N(SyntaxKind.OpenParenthesisToken);
            await N(SyntaxKind.CloseParenthesisToken);

            await N(SyntaxKind.MinusGreaterThanToken);

            await N(SyntaxKind.NilType);
            {
                await N(SyntaxKind.NilKeyword);
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ErrorsOnMultipleIndexers()
    {
        await UsingTypeAsync(
            "{[T]: T, [T]: T}",
            // (1,10): error LUA1017: Only one indexer is allowed per table type
            // {[T]: T, [T]: T}
            Diagnostic(ErrorCode.ERR_OnlyOneTableTypeIndexerIsAllowed, "[T]: T").WithLocation(1, 10));

        await N(SyntaxKind.TableType);
        {
            await N(SyntaxKind.OpenBraceToken);
            await N(SyntaxKind.TableTypeIndexer);
            {
                await N(SyntaxKind.OpenBracketToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CloseBracketToken);
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.TableTypeIndexer);
            {
                await N(SyntaxKind.OpenBracketToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
                await N(SyntaxKind.CloseBracketToken);
                await N(SyntaxKind.ColonToken);
                await N(SyntaxKind.SimpleTypeName);
                {
                    await N(SyntaxKind.IdentifierToken, "T");
                }
            }
            await N(SyntaxKind.CloseBraceToken);
        }
        EOF();
    }

    [Test]
    public async Task Parser_ErrorsWhenAcceptTypedLuaIsFalse_AndTypedLuaStructuresAreFound()
    {
        var options = LuaSyntaxOptions.All.With(acceptTypedLua: false);
        await ParseAndValidateAsync(
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
