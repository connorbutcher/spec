using PUSpecSheet.Domain.SheetTypes;

namespace PUSpecSheet.Domain.Tests.SheetTypes;

public sealed class SheetIdentifierRulesTests
{
    [Theory]
    [InlineData("1.50", "1.5")]
    [InlineData(" 12 ", "12")]
    [InlineData("1,000.0", "1000")]
    public void TryNormalise_WritesNumbersOneWay(string raw, string expected)
    {
        var ok = SheetIdentifierRules.TryNormalise(Definition(SheetIdentifierKind.Number), raw, out var stored, out _);

        Assert.True(ok);
        Assert.Equal(expected, stored);
    }

    [Fact]
    public void TryNormalise_WritesDatesAsIsoDates()
    {
        var ok = SheetIdentifierRules.TryNormalise(Definition(SheetIdentifierKind.Date), "2026-11-03", out var stored, out _);

        Assert.True(ok);
        Assert.Equal("2026-11-03", stored);
    }

    [Fact]
    public void TryNormalise_TakesTheOptionsOwnSpelling()
    {
        var definition = Definition(SheetIdentifierKind.Option, options: ["Build A", "Build B"]);

        var ok = SheetIdentifierRules.TryNormalise(definition, "build b", out var stored, out _);

        Assert.True(ok);
        Assert.Equal("Build B", stored);
    }

    [Fact]
    public void TryNormalise_RefusesAValueThatIsNotAnOption()
    {
        var definition = Definition(SheetIdentifierKind.Option, options: ["Build A"]);

        Assert.False(SheetIdentifierRules.TryNormalise(definition, "Build C", out _, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void TryNormalise_RefusesTextThatIsNotANumber()
    {
        Assert.False(SheetIdentifierRules.TryNormalise(Definition(SheetIdentifierKind.Number), "ESN", out _, out _));
    }

    [Fact]
    public void TryNormalise_BlankIsNullWhenOptional_AndRefusedWhenRequired()
    {
        Assert.True(SheetIdentifierRules.TryNormalise(Definition(SheetIdentifierKind.Text), "  ", out var stored, out _));
        Assert.Null(stored);
        Assert.False(SheetIdentifierRules.TryNormalise(Definition(SheetIdentifierKind.Text, isRequired: true), null, out _, out var error));
        Assert.Equal("Serial number is required.", error);
    }

    [Fact]
    public void TryNormalise_RefusesAValueLongerThanAllowed()
    {
        var raw = new string('x', SheetIdentifierRules.MaxValueLength + 1);

        Assert.False(SheetIdentifierRules.TryNormalise(Definition(SheetIdentifierKind.Text), raw, out _, out _));
    }

    [Fact]
    public void IdentityKey_IgnoresCaseOfTextAndTheOrderValuesAreGiven()
    {
        var serial = Definition(SheetIdentifierKind.Text, id: 1);
        var build = Definition(SheetIdentifierKind.Number, id: 2);

        var first = SheetIdentifierRules.IdentityKey([new SheetIdentifierValue(serial, "esn-1"), new SheetIdentifierValue(build, "3")]);
        var second = SheetIdentifierRules.IdentityKey([new SheetIdentifierValue(build, "3"), new SheetIdentifierValue(serial, "ESN-1")]);

        Assert.Equal(first, second);
    }

    [Fact]
    public void IdentityKey_IsEmptyWithNoValues()
    {
        Assert.Equal(string.Empty, SheetIdentifierRules.IdentityKey([]));
    }

    [Fact]
    public void IdentityKey_TellsDifferentValuesApart()
    {
        var serial = Definition(SheetIdentifierKind.Text);

        Assert.NotEqual(
            SheetIdentifierRules.IdentityKey([new SheetIdentifierValue(serial, "ESN-1")]),
            SheetIdentifierRules.IdentityKey([new SheetIdentifierValue(serial, "ESN-2")]));
    }

    [Fact]
    public void IdentityKey_FitsTheColumnAtTheMostIdentifiersAndLongestValues()
    {
        var values = Enumerable.Range(int.MaxValue - SheetIdentifierRules.MaxIdentifiers, SheetIdentifierRules.MaxIdentifiers)
            .Select(id => new SheetIdentifierValue(Definition(SheetIdentifierKind.Text, id: id), new string('x', SheetIdentifierRules.MaxValueLength)));

        Assert.True(SheetIdentifierRules.IdentityKey(values).Length <= 450);
    }

    [Theory]
    [InlineData("Serial Number", "serialNumber")]
    [InlineData("ESN", "esn")]
    [InlineData("Build no.", "buildNo")]
    [InlineData("2nd build", "identifier2ndBuild")]
    [InlineData("---", "identifier")]
    public void KeyFromName_MakesACamelCaseKey(string name, string expected)
    {
        Assert.Equal(expected, SheetIdentifierRules.KeyFromName(name));
    }

    private static SheetIdentifierDefinition Definition(SheetIdentifierKind kind, int id = 1, bool isRequired = false, IReadOnlyList<string>? options = null)
    {
        return new SheetIdentifierDefinition(id, "Serial number", kind, isRequired, options ?? []);
    }
}
