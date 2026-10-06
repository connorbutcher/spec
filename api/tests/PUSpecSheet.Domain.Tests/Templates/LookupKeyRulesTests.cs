using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Tests.Templates;

/// <summary>A lookup key goes into addresses other applications call, so it is kept to plain characters.</summary>
public sealed class LookupKeyRulesTests
{
    [Theory]
    [InlineData("partNumber")]
    [InlineData("part-number")]
    [InlineData("part.number_2")]
    [InlineData("p")]
    public void LettersDigitsDotsHyphensAndUnderscores_StartingWithALetter_AreValid(string key)
    {
        Assert.True(LookupKeyRules.IsValid(key));
    }

    [Theory]
    [InlineData("")]
    [InlineData("2ndPart")]
    [InlineData("_part")]
    [InlineData("part number")]
    [InlineData("part/number")]
    [InlineData("pärt")]
    public void AnythingElse_IsNot(string key)
    {
        Assert.False(LookupKeyRules.IsValid(key));
    }

    [Fact]
    public void AKeyCanBeAsLongAsItsColumn_AndNoLonger()
    {
        Assert.True(LookupKeyRules.IsValid(new string('k', LookupKeyRules.MaxLength)));
        Assert.False(LookupKeyRules.IsValid(new string('k', LookupKeyRules.MaxLength + 1)));
    }
}
