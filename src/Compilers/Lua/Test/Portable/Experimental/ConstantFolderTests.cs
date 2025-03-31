using Loretta.CodeAnalysis.Lua.Experimental;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Loretta.CodeAnalysis.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Experimental;

public sealed class ConstantFolderTests : LuaTestBase
{
    [Test]
    // Unary operators
    //     Negation
    [Arguments("-1", -1L)]
    [Arguments("-1.0", -1.0)]
    [Arguments("-1.5", -1.5)]
    //     Logical not
    [Arguments("not nil", true)]
    [Arguments("not true", false)]
    [Arguments("not false", true)]
    [Arguments("not 1", false)]
    [Arguments("not 'a'", false)]
    [Arguments("not function()end", false)]
    //     Bitwise not
    [Arguments("~1.0", (double) ~1L)]
    [Arguments("~1", ~1L)]
    //     Length
    [Arguments("#''", 0.0)]
    [Arguments("#'a'", 1.0)]
    [Arguments("#'ab'", 2.0)]
    [Arguments("#'abc'", 3.0)]
    // Binary operators
    //     Addition
    [Arguments("1 + 1", 2L)]
    [Arguments("1.5 + 1.5", 3.0)]
    [Arguments("1.5 + 1", 2.5)]
    //         Overflow (can't test for doubles as infinity a that doesn't get folded)
    [Arguments("9223372036854775807 + 1", unchecked(9223372036854775807 + 1))]
    //     Subtraction
    [Arguments("1 - 1", 0L)]
    [Arguments("1.5 - 1.5", 0.0)]
    [Arguments("1.5 - 1", 0.5)]
    //         Underflow (can't test for doubles as infinity doesn't get folded)
    [Arguments("-9223372036854775807 - 5", unchecked(-9223372036854775807 - 5))]
    //     Multiplication
    [Arguments("1.5 * 2.5", 1.5 * 2.5)]
    [Arguments("1 * 2", 1L * 2)]
    [Arguments("1.5 * 2", 3.0)]
    //         Overflow
    [Arguments("9223372036854775807 * 2", -2L)]
    [Arguments("9223372036854775807 * -20", 20L)]
    //     Division
    [Arguments("1.5 / 1.5", 1.0)]
    [Arguments("5 / 2", 2.5)]
    [Arguments("5.0 / 2", 2.5)]
    [Arguments("2 / 5", 0.4)]
    //         Something that would overflow in division integer
    [Arguments("9223372036854775807 / -1", 9223372036854775807 / -1.0)]
    //     Modulo
    [Arguments("5 % 2", 1L)]
    [Arguments("5 % 2.5", 0.0)]
    [Arguments("5.5 % 1", 0.5)]
    //     Exponentiation
    [Arguments("2 ^ 2", 4.0)]
    [Arguments("4 ^ 0.5", 2.0)]
    //     Concatenation
    [Arguments("'a' .. 'b'", "ab")]
    [Arguments("'a' .. true", "atrue")]
    [Arguments("'a' .. false", "afalse")]
    //     Equality
    [Arguments("'a' == 'a'", true)]
    [Arguments("'a' == 'b'", false)]
    [Arguments("1 == 1", true)]
    [Arguments("1 == 2", false)]
    [Arguments("1.0 == 1", true)]
    [Arguments("1.1 == 1", false)]
    [Arguments("nil == nil", true)]
    [Arguments("true == true", true)]
    [Arguments("true == false", false)]
    [Arguments("false == false", true)]
    [Arguments("'a' == false", false)]
    //     Inequality
    [Arguments("'a' != 'a'", false)]
    [Arguments("'a' != 'b'", true)]
    [Arguments("1 != 1", false)]
    [Arguments("1 != 2", true)]
    [Arguments("1.0 != 1", false)]
    [Arguments("1.1 != 1", true)]
    [Arguments("nil != nil", false)]
    [Arguments("1 != nil", true)]
    [Arguments("true != true", false)]
    [Arguments("true != false", true)]
    [Arguments("false != false", false)]
    //     Less than
    [Arguments("1 < 2", true)]
    [Arguments("1 < 1", false)]
    [Arguments("2 < 1", false)]
    [Arguments("1 < 1.5", true)]
    [Arguments("1.5 < 1", false)]
    [Arguments("1.5 < 1.5", false)]
    [Arguments("'a' < 'b'", true)]
    [Arguments("'a' < 'a'", false)]
    [Arguments("'b' < 'a'", false)]
    //     Less than or equals
    [Arguments("1 <= 1", true)]
    [Arguments("1 <= 2", true)]
    [Arguments("2 <= 1", false)]
    [Arguments("1.5 <= 1.5", true)]
    [Arguments("1.5 <= 2", true)]
    [Arguments("2 <= 1.5", false)]
    [Arguments("'a' <= 'a'", true)]
    [Arguments("'a' <= 'b'", true)]
    [Arguments("'b' <= 'a'", false)]
    //     Greater than
    [Arguments("2 > 1", true)]
    [Arguments("1 > 1", false)]
    [Arguments("1 > 2", false)]
    [Arguments("1.5 > 1", true)]
    [Arguments("1 > 1.5", false)]
    [Arguments("1.5 > 1.5", false)]
    [Arguments("'b' > 'a'", true)]
    [Arguments("'a' > 'a'", false)]
    [Arguments("'a' > 'b'", false)]
    //     Greater than or equal
    [Arguments("1 >= 1", true)]
    [Arguments("2 >= 1", true)]
    [Arguments("1 >= 2", false)]
    [Arguments("1.5 >= 1.5", true)]
    [Arguments("2 >= 1.5", true)]
    [Arguments("1.5 >= 2", false)]
    [Arguments("'a' >= 'a'", true)]
    [Arguments("'b' >= 'a'", true)]
    [Arguments("'a' >= 'b'", false)]
    //     Logical and
    [Arguments("nil and 2", null)]
    [Arguments("true and 2", 2L)]
    [Arguments("false and 2", false)]
    [Arguments("1 and 2", 2L)]
    [Arguments("'a' and 2", 2L)]
    [Arguments("function()end and 2", 2L)]
    //     Logical or
    [Arguments("nil or 2", 2L)]
    [Arguments("true or 2", true)]
    [Arguments("false or 2", 2L)]
    [Arguments("1 or 2", 1L)]
    [Arguments("'a' or 2", "a")]
    [Arguments("2 or function()end", 2L)]
    //     Bitwise or
    [Arguments("1 | 1", 1L)]
    [Arguments("1 | 1.0", 1L)]
    [Arguments("1.0 | 1", 1L)]
    [Arguments("1.0 | 1.0", 1.0)]
    [Arguments("1 | 2", 3L)]
    //     Bitwise and
    [Arguments("1 & 1", 1L)]
    [Arguments("1 & 1.0", 1L)]
    [Arguments("1.0 & 1", 1L)]
    [Arguments("1.0 & 1.0", 1.0)]
    [Arguments("1 & 2", 0L)]
    //     Right shift
    [Arguments("511 >> 3", 511L >> 3)]
    [Arguments("511 >> 3.0", 511L >> 3)]
    [Arguments("511.0 >> 3", 511L >> 3)]
    [Arguments("511.0 >> 3.0", (double) (511L >> 3))]
    //     Left shift
    [Arguments("511 << 3", 511L << 3)]
    [Arguments("511 << 3.0", 511L << 3)]
    [Arguments("511.0 << 3", 511L << 3)]
    [Arguments("511.0 << 3.0", (double) (511L << 3))]
    //     Bitwise xor
    [Arguments("42 ~ 21", 42L ^ 21L)]
    [Arguments("42 ~ 21.0", 42L ^ 21L)]
    [Arguments("42.0 ~ 21", 42L ^ 21L)]
    [Arguments("42.0 ~ 21.0", (double) (42L ^ 21L))]
    [Arguments("42 ~ 42", 0L)]
    [Arguments("42 ~ 42.0", 0L)]
    [Arguments("42.0 ~ 42", 0L)]
    [Arguments("42.0 ~ 42.0", 0.0)]
    public async Task ConstantFolder_FoldsOperationsCorrectly(string source, object? expected)
    {
        var sourceNode = await ParseAndValidateExpressionAsync(source, LuaSyntaxOptions.AllWithIntegers);

        await Assert.That(sourceNode.ConstantFold(ConstantFoldingOptions.Default)).IsTypeOf<LiteralExpressionSyntax>()
                    .And.Satisfies(static folded => folded.Token, src => src.HasValue(expected));
    }

    [Test]
    // Unary operator
    //     Negation
    [Arguments("-a")]
    [Arguments("-{}")]
    [Arguments("-'1'")]
    //     Logical not
    [Arguments("not func()")]
    //     Bitwise not
    [Arguments("~a")]
    [Arguments("~1.5")]
    [Arguments("~'1'")]
    //     Length
    [Arguments("#{}")]
    [Arguments("#{nil}")]
    // Binary operator
    //     Addition
    [Arguments("nil + true")]
    [Arguments("function()end + true")]
    [Arguments("'1' + '1'")]
    //         Infinity
    [Arguments("1.7976931348623157E+308 + 1.7976931348623157E+308")]
    //     Subtraction
    [Arguments("nil - true")]
    [Arguments("function()end - true")]
    [Arguments("'1' - '1'")]
    //        Infinity
    // [Arguments("-1.7976931348623157E+308 - 1.7976931348623157E+308")] // Can't do this because unary op gets folded.
    //     Multiplication
    [Arguments("nil * 2")]
    [Arguments("function()end * 2")]
    [Arguments("'1' * '1'")]
    //         Infinity
    [Arguments("1.7976931348623157E+308 * 2")]
    //     Division
    [Arguments("2 / a")]
    [Arguments("1.7976931348623157E+308 / true")]
    [Arguments("'1' / '1'")]
    //     Modulo
    [Arguments("true % 2")]
    [Arguments("2 % f()")]
    [Arguments("'1' % '1'")]
    //     Exponentiation
    [Arguments("1.7976931348623157E+308 ^ 2")]
    //     Concatenation
    [Arguments("1 .. 2")]
    //     Equality
    [Arguments("{} == {}")]
    [Arguments("function()end == function()end")]
    [Arguments("a == a")]
    //     Inequality
    [Arguments("{} != {}")]
    [Arguments("function()end != function()end")]
    [Arguments("a != a")]
    //     Less than
    [Arguments("true < true")]
    [Arguments("true < false")]
    [Arguments("function()end < function()end")]
    //     Less than or equals
    [Arguments("true <= true")]
    [Arguments("a <= a")]
    [Arguments("function()end <= function()end")]
    //     Greater than
    [Arguments("true > true")]
    [Arguments("true > false")]
    [Arguments("function()end > function()end")]
    //     Greater than or equals
    [Arguments("true >= true")]
    [Arguments("true >= false")]
    [Arguments("function()end >= function()end")]
    //     Logical and
    [Arguments("func() and 1")]
    [Arguments("a and 1")]
    [Arguments("{} and 2")]
    //     Logical or
    [Arguments("func() or 1")]
    [Arguments("a or 1")]
    [Arguments("{} or 2")]
    //     Bitwise or
    [Arguments("1.5 | 1")]
    [Arguments("1 | 1.5")]
    [Arguments("1.1 | 1.1")]
    [Arguments("a | a")]
    [Arguments("function()end | function()end")]
    [Arguments("'1' | '1'")]
    //     Bitwise and
    [Arguments("1.5 & 1")]
    [Arguments("1 & 1.5")]
    [Arguments("1.1 & 1.1")]
    [Arguments("a & a")]
    [Arguments("function()end & function()end")]
    [Arguments("'1' & '1'")]
    //     Right shift
    [Arguments("1.5 >> 1")]
    [Arguments("1 >> 1.5")]
    [Arguments("1.5 >> 1.5")]
    [Arguments("a >> a")]
    [Arguments("function()end >> function()end")]
    [Arguments("'1' >> '1'")]
    //     Left shift
    [Arguments("1.5 << 1")]
    [Arguments("1 << 1.5")]
    [Arguments("1.5 << 1.5")]
    [Arguments("a << a")]
    [Arguments("function()end << function()end")]
    [Arguments("'1' << '1'")]
    //     Bitwise xor
    [Arguments("1.5 ~ 1.5")]
    [Arguments("1.1 ~ 1.1")]
    [Arguments("'1' ~ '1'")]
    public async Task ConstantFolder_DoesNotFoldOtherOperations(string source)
    {
        var sourceNode = await ParseAndValidateExpressionAsync(source, LuaSyntaxOptions.AllWithIntegers);

        await Assert.That(sourceNode.ConstantFold(ConstantFoldingOptions.Default)).IsEqualTo(sourceNode);
    }

    [Test]
    // Unary operators
    //     Negation
    [Arguments("-'1'", -1L)]
    [Arguments("-'1.0'", -1.0)]
    [Arguments("-'1.5'", -1.5)]
    //     Bitwise not
    [Arguments("~'1.0'", (double) ~1L)]
    [Arguments("~'1'", (double) ~1L)]
    // Binary operators
    //     Addition
    [Arguments("'1' + 1", 2L)]
    [Arguments("1.5 + '1.5'", 3.0)]
    [Arguments("'1.5' + 1", 2.5)]
    //         Overflow (can't test for doubles as infinity a that doesn't get folded)
    [Arguments("'9223372036854775807' + 1", unchecked(9223372036854775807 + 1))]
    //     Subtraction
    [Arguments("'1' - 1", 0L)]
    [Arguments("1.5 - '1.5'", 0.0)]
    [Arguments("'1.5' - 1", 0.5)]
    //         Underflow (can't test for doubles as infinity doesn't get folded)
    [Arguments("'-9223372036854775808' - 2", unchecked(-9223372036854775808 - 2))]
    //     Multiplication
    [Arguments("'1.5' * 2.5", 1.5 * 2.5)]
    [Arguments("1 * '2'", 1L * 2)]
    [Arguments("'1.5' * 2", 3.0)]
    //         Overflow
    [Arguments("'9223372036854775807' * 2", -2L)]
    [Arguments("'9223372036854775807' * -20", 20L)]
    //     Division
    [Arguments("'1.5' / 1.5", 1.0)]
    [Arguments("'5' / 2", 2.5)]
    [Arguments("5.0 / '2'", 2.5)]
    [Arguments("'2' / 5", 0.4)]
    //         Something that would overflow in division integer
    [Arguments("'9223372036854775807' / -1", 9223372036854775807 / -1.0)]
    //     Modulo
    [Arguments("'5' % 2", 1L)]
    [Arguments("5 % '2.5'", 0.0)]
    [Arguments("'5.5' % 1", 0.5)]
    //     Exponentiation
    [Arguments("'2' ^ 2", 4.0)]
    [Arguments("4 ^ '0.5'", 2.0)]
    //     Bitwise or
    [Arguments("'1' | 1", 1L)]
    [Arguments("1 | '1.0'", 1L)]
    [Arguments("'1.0' | 1", 1L)]
    [Arguments("1.0 | '1.0'", 1.0)]
    [Arguments("'1' | 2", 3L)]
    //     Bitwise and
    [Arguments("'1' & 1", 1L)]
    [Arguments("1 & '1.0'", 1L)]
    [Arguments("'1.0' & 1", 1L)]
    [Arguments("1.0 & '1.0'", 1.0)]
    [Arguments("'1' & 2", 0L)]
    //     Right shift
    [Arguments("'511' >> 3", 511L >> 3)]
    [Arguments("511 >> '3.0'", 511L >> 3)]
    [Arguments("'511.0' >> 3", 511L >> 3)]
    [Arguments("511.0 >> '3.0'", (double) (511L >> 3))]
    //     Left shift
    [Arguments("'511' << 3", 511L << 3)]
    [Arguments("511 << '3.0'", 511L << 3)]
    [Arguments("'511.0' << 3", 511L << 3)]
    [Arguments("511.0 << '3.0'", (double) (511L << 3))]
    //     Bitwise xor
    [Arguments("'42' ~ 21", 42L ^ 21L)]
    [Arguments("42 ~ '21.0'", 42L ^ 21L)]
    [Arguments("'42.0' ~ 21", 42L ^ 21L)]
    [Arguments("42.0 ~ '21.0'", (double) (42L ^ 21L))]
    [Arguments("'42' ~ 42", 0L)]
    [Arguments("42 ~ '42.0'", 0L)]
    [Arguments("'42.0' ~ 42", 0L)]
    [Arguments("42.0 ~ '42.0'", 0.0)]
    public async Task ConstantFolder_FoldsOperationsCorrectlyWithStringExtractionEnabled(string source, object expected)
    {
        var sourceNode = await ParseAndValidateExpressionAsync(source, LuaSyntaxOptions.AllWithIntegers);

        // ReSharper disable once WithExpressionModifiesAllMembers
        var options = ConstantFoldingOptions.Default with { ExtractNumbersFromStrings = true };
        await Assert.That(sourceNode.ConstantFold(options)).IsTypeOf<LiteralExpressionSyntax>().And.Satisfies(
            static folded => folded.Token,
            value => value.HasValue(expected));
    }
}
