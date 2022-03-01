using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loretta.CodeAnalysis.Lua.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Utilities
{
    public class StringUtilsTests
    {
        [Theory]
        [InlineData("a", "a")]
        [InlineData(" a", "a")]
        [InlineData("\ta\t", "a")]
        [InlineData(" a ", "a")]
        [InlineData("a ", "a")]
        [InlineData("\v\t\r\n a\v\r\n\t ", "a")]
        public void StringUtils_Trim_WorksCorrectly(string input, string expected)
        {
            var trimmed = StringUtils.Trim(input);
            Assert.Equal(expected, trimmed);
        }

        [Theory]
        [InlineData("a", "a")]
        [InlineData(" a", "a")]
        [InlineData("\ta\t", "a")]
        [InlineData(" a ", "a")]
        [InlineData("a ", "a")]
        [InlineData("\v\t\r\n a\v\r\n\t ", "a")]
        public void StringUtils_TrimSpan_WorksCorrectly(string input, string expected)
        {
            var trimmed = StringUtils.Trim(input.AsSpan());
            Assert.Equal(expected, trimmed.ToString());
        }
    }
}
