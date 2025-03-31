using Loretta.CodeAnalysis.Lua.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Utilities;

public sealed class StringUtilsTests
{
    [Test]
    [Arguments("a", "a")]
    [Arguments(" a", "a")]
    [Arguments("\ta\t", "a")]
    [Arguments(" a ", "a")]
    [Arguments("a ", "a")]
    [Arguments("\v\t\r\n a\v\r\n\t ", "a")]
    public async Task StringUtils_Trim_WorksCorrectly(string input, string expected)
    {
        var trimmed = StringUtils.Trim(input);
        await Assert.That(trimmed).IsEqualTo(expected);
    }

    [Test]
    [Arguments("a", "a")]
    [Arguments(" a", "a")]
    [Arguments("\ta\t", "a")]
    [Arguments(" a ", "a")]
    [Arguments("a ", "a")]
    [Arguments("\v\t\r\n a\v\r\n\t ", "a")]
    public async Task StringUtils_TrimSpan_WorksCorrectly(string input, string expected)
    {
        var trimmed = StringUtils.Trim(input.AsSpan());
        await Assert.That(trimmed.ToString()).IsEqualTo(expected);
    }
}
