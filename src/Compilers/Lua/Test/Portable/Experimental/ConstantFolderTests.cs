using Loretta.CodeAnalysis.Lua.Experimental;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Experimental
{
    public class ConstantFolderTests : LuaTestBase
    {
        [Theory(Timeout = 250)]
        // Unary operators
        //     Negation
        [InlineData("-1", -1L)]
        [InlineData("-1.0", -1.0)]
        [InlineData("-1.5", -1.5)]
        //     Logical not
        [InlineData("not nil", true)]
        [InlineData("not true", false)]
        [InlineData("not false", true)]
        [InlineData("not 1", false)]
        [InlineData("not 'a'", false)]
        [InlineData("not function()end", false)]
        //     Bitwise not
        [InlineData("~1.0", (double) ~1L)]
        [InlineData("~1", ~1L)]
        //     Length
        [InlineData("#''", 0.0)]
        [InlineData("#'a'", 1.0)]
        [InlineData("#'ab'", 2.0)]
        [InlineData("#'abc'", 3.0)]
        // Binary operators
        //     Addition
        [InlineData("1 + 1", 2L)]
        [InlineData("1.5 + 1.5", 3.0)]
        [InlineData("1.5 + 1", 2.5)]
        //         Overflow (can't test for doubles as infinity a that doesn't get folded)
        [InlineData("9223372036854775807 + 1", unchecked(9223372036854775807 + 1))]
        //     Subtraction
        [InlineData("1 - 1", 0L)]
        [InlineData("1.5 - 1.5", 0.0)]
        [InlineData("1.5 - 1", 0.5)]
        //         Underflow (can't test for doubles as infinity doesn't get folded)
        [InlineData("-9223372036854775807 - 5", unchecked(-9223372036854775807 - 5))]
        //     Multiplication
        [InlineData("1.5 * 2.5", 1.5 * 2.5)]
        [InlineData("1 * 2", 1L * 2)]
        [InlineData("1.5 * 2", 3.0)]
        //         Overflow
        [InlineData("9223372036854775807 * 2", -2L)]
        [InlineData("9223372036854775807 * -20", 20L)]
        //     Division
        [InlineData("1.5 / 1.5", 1.0)]
        [InlineData("5 / 2", 2.5)]
        [InlineData("5.0 / 2", 2.5)]
        [InlineData("2 / 5", 0.4)]
        //         Something that would overflow in division integer
        [InlineData("9223372036854775807 / -1", unchecked(9223372036854775807 / -1.0))]
        //     Modulo
        [InlineData("5 % 2", 1L)]
        [InlineData("5 % 2.5", 0.0)]
        [InlineData("5.5 % 1", 0.5)]
        //     Exponentiation
        [InlineData("2 ^ 2", 4.0)]
        [InlineData("4 ^ 0.5", 2.0)]
        //     Concatenation
        [InlineData("'a' .. 'b'", "ab")]
        [InlineData("'a' .. true", "atrue")]
        [InlineData("'a' .. false", "afalse")]
        //     Equality
        [InlineData("'a' == 'a'", true)]
        [InlineData("'a' == 'b'", false)]
        [InlineData("1 == 1", true)]
        [InlineData("1 == 2", false)]
        [InlineData("1.0 == 1", true)]
        [InlineData("1.1 == 1", false)]
        [InlineData("nil == nil", true)]
        [InlineData("true == true", true)]
        [InlineData("true == false", false)]
        [InlineData("false == false", true)]
        [InlineData("'a' == false", false)]
        //     Inequality
        [InlineData("'a' != 'a'", false)]
        [InlineData("'a' != 'b'", true)]
        [InlineData("1 != 1", false)]
        [InlineData("1 != 2", true)]
        [InlineData("1.0 != 1", false)]
        [InlineData("1.1 != 1", true)]
        [InlineData("nil != nil", false)]
        [InlineData("1 != nil", true)]
        [InlineData("true != true", false)]
        [InlineData("true != false", true)]
        [InlineData("false != false", false)]
        //     Less than
        [InlineData("1 < 2", true)]
        [InlineData("1 < 1", false)]
        [InlineData("2 < 1", false)]
        [InlineData("1 < 1.5", true)]
        [InlineData("1.5 < 1", false)]
        [InlineData("1.5 < 1.5", false)]
        [InlineData("'a' < 'b'", true)]
        [InlineData("'a' < 'a'", false)]
        [InlineData("'b' < 'a'", false)]
        //     Less than or equals
        [InlineData("1 <= 1", true)]
        [InlineData("1 <= 2", true)]
        [InlineData("2 <= 1", false)]
        [InlineData("1.5 <= 1.5", true)]
        [InlineData("1.5 <= 2", true)]
        [InlineData("2 <= 1.5", false)]
        [InlineData("'a' <= 'a'", true)]
        [InlineData("'a' <= 'b'", true)]
        [InlineData("'b' <= 'a'", false)]
        //     Greater than
        [InlineData("2 > 1", true)]
        [InlineData("1 > 1", false)]
        [InlineData("1 > 2", false)]
        [InlineData("1.5 > 1", true)]
        [InlineData("1 > 1.5", false)]
        [InlineData("1.5 > 1.5", false)]
        [InlineData("'b' > 'a'", true)]
        [InlineData("'a' > 'a'", false)]
        [InlineData("'a' > 'b'", false)]
        //     Greater than or equal
        [InlineData("1 >= 1", true)]
        [InlineData("2 >= 1", true)]
        [InlineData("1 >= 2", false)]
        [InlineData("1.5 >= 1.5", true)]
        [InlineData("2 >= 1.5", true)]
        [InlineData("1.5 >= 2", false)]
        [InlineData("'a' >= 'a'", true)]
        [InlineData("'b' >= 'a'", true)]
        [InlineData("'a' >= 'b'", false)]
        //     Logical and
        [InlineData("nil and 2", null)]
        [InlineData("true and 2", 2L)]
        [InlineData("false and 2", false)]
        [InlineData("1 and 2", 2L)]
        [InlineData("'a' and 2", 2L)]
        [InlineData("function()end and 2", 2L)]
        //     Logical or
        [InlineData("nil or 2", 2L)]
        [InlineData("true or 2", true)]
        [InlineData("false or 2", 2L)]
        [InlineData("1 or 2", 1L)]
        [InlineData("'a' or 2", "a")]
        [InlineData("2 or function()end", 2L)]
        //     Bitwise or
        [InlineData("1 | 1", 1L)]
        [InlineData("1 | 1.0", 1L)]
        [InlineData("1.0 | 1", 1L)]
        [InlineData("1.0 | 1.0", 1.0)]
        [InlineData("1 | 2", 3L)]
        //     Bitwise and
        [InlineData("1 & 1", 1L)]
        [InlineData("1 & 1.0", 1L)]
        [InlineData("1.0 & 1", 1L)]
        [InlineData("1.0 & 1.0", 1.0)]
        [InlineData("1 & 2", 0L)]
        //     Right shift
        [InlineData("511 >> 3", 511L >> 3)]
        [InlineData("511 >> 3.0", 511L >> 3)]
        [InlineData("511.0 >> 3", 511L >> 3)]
        [InlineData("511.0 >> 3.0", (double) (511L >> 3))]
        //     Left shift
        [InlineData("511 << 3", 511L << 3)]
        [InlineData("511 << 3.0", 511L << 3)]
        [InlineData("511.0 << 3", 511L << 3)]
        [InlineData("511.0 << 3.0", (double) (511L << 3))]
        //     Bitwise xor
        [InlineData("42 ~ 21", 42L ^ 21L)]
        [InlineData("42 ~ 21.0", 42L ^ 21L)]
        [InlineData("42.0 ~ 21", 42L ^ 21L)]
        [InlineData("42.0 ~ 21.0", (double) (42L ^ 21L))]
        [InlineData("42 ~ 42", 0L)]
        [InlineData("42 ~ 42.0", 0L)]
        [InlineData("42.0 ~ 42", 0L)]
        [InlineData("42.0 ~ 42.0", 0.0)]
        public void ConstantFolder_FoldsOperationsCorrectly(string source, object expected)
        {
            var sourceNode = ParseAndValidateExpression(
                source,
                LuaSyntaxOptions.AllWithIntegers);

            var folded = Assert.IsType<LiteralExpressionSyntax>(sourceNode.ConstantFold(ConstantFoldingOptions.Default));

            Assert.Equal(expected, folded.Token.Value);
        }

        [Theory(Timeout = 250)]
        // Unary operator
        //     Negation
        [InlineData("-a")]
        [InlineData("-{}")]
        //     Logical not
        [InlineData("not func()")]
        //     Bitwise not
        [InlineData("~a")]
        [InlineData("~1.5")]
        //     Length
        [InlineData("#{}")]
        [InlineData("#{nil}")]
        // Binary operator
        //     Addition
        [InlineData("nil + true")]
        [InlineData("function()end + true")]
        //         Infinity
        [InlineData("1.7976931348623157E+308 + 1.7976931348623157E+308")]
        //     Subtraction
        [InlineData("nil - true")]
        [InlineData("function()end - true")]
        //        Infinity
        // [InlineData("-1.7976931348623157E+308 - 1.7976931348623157E+308")] // Can't do this because unary op gets folded.
        //     Multiplication
        [InlineData("nil * 2")]
        [InlineData("function()end * 2")]
        //         Infinity
        [InlineData("1.7976931348623157E+308 * 2")]
        //     Division
        [InlineData("2 / a")]
        [InlineData("1.7976931348623157E+308 / true")]
        //     Modulo
        [InlineData("true % 2")]
        [InlineData("2 % f()")]
        //     Exponentiation
        [InlineData("1.7976931348623157E+308 ^ 2")]
        //     Concatenation
        [InlineData("1 .. 2")]
        //     Equality
        [InlineData("{} == {}")]
        [InlineData("function()end == function()end")]
        [InlineData("a == a")]
        //     Inequality
        [InlineData("{} != {}")]
        [InlineData("function()end != function()end")]
        [InlineData("a != a")]
        //     Less than
        [InlineData("true < true")]
        [InlineData("true < false")]
        [InlineData("function()end < function()end")]
        //     Less than or equals
        [InlineData("true <= true")]
        [InlineData("a <= a")]
        [InlineData("function()end <= function()end")]
        //     Greater than
        [InlineData("true > true")]
        [InlineData("true > false")]
        [InlineData("function()end > function()end")]
        //     Greater than or equals
        [InlineData("true >= true")]
        [InlineData("true >= false")]
        [InlineData("function()end >= function()end")]
        //     Logical and
        [InlineData("func() and 1")]
        [InlineData("a and 1")]
        [InlineData("{} and 2")]
        //     Logical or
        [InlineData("func() or 1")]
        [InlineData("a or 1")]
        [InlineData("{} or 2")]
        //     Bitwise or
        [InlineData("1.5 | 1")]
        [InlineData("1 | 1.5")]
        [InlineData("1.1 | 1.1")]
        [InlineData("a | a")]
        [InlineData("function()end | function()end")]
        //     Bitwise and
        [InlineData("1.5 & 1")]
        [InlineData("1 & 1.5")]
        [InlineData("1.1 & 1.1")]
        [InlineData("a & a")]
        [InlineData("function()end & function()end")]
        //     Right shift
        [InlineData("1.5 >> 1")]
        [InlineData("1 >> 1.5")]
        [InlineData("1.5 >> 1.5")]
        [InlineData("a >> a")]
        [InlineData("function()end >> function()end")]
        //     Left shift
        [InlineData("1.5 << 1")]
        [InlineData("1 << 1.5")]
        [InlineData("1.5 << 1.5")]
        [InlineData("a << a")]
        [InlineData("function()end << function()end")]
        //     Bitwise xor
        [InlineData("1.5 ~ 1.5")]
        [InlineData("1.1 ~ 1.1")]
        public void ConstantFolder_DoesNotFoldOtherOperations(string source)
        {
            var sourceNode = ParseAndValidateExpression(
                source,
                LuaSyntaxOptions.AllWithIntegers);

            var folded = sourceNode.ConstantFold(ConstantFoldingOptions.Default);

            Assert.Equal(sourceNode, folded);
        }
    }
}
