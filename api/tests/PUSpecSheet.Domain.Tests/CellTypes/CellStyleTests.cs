using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Domain.Tests.CellTypes;

public sealed class CellStyleTests
{
    [Fact]
    public void Apply_LaysOverridesOnTopOfTheDefaults()
    {
        var defaults = new CellStyle { Bold = true, Align = CellTextAlign.Right };
        var overrides = new CellStyle { Align = CellTextAlign.Center, TextColor = "#ff0000" };

        var effective = defaults.Apply(overrides);

        Assert.Equal(new CellStyle { Bold = true, Align = CellTextAlign.Center, TextColor = "#ff0000" }, effective);
    }

    [Theory]
    [InlineData("#1f2937", true)]
    [InlineData("1f2937", false)]
    [InlineData("red", false)]
    public void Validate_AcceptsOnlyHexColours(string colour, bool valid)
    {
        var style = new CellStyle { BackgroundColor = colour };

        Assert.Equal(valid, style.Validate() is null);
    }
}
