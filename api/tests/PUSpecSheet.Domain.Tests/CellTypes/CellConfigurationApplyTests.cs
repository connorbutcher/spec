using PUSpecSheet.Domain.CellTypes.Configurations;

namespace PUSpecSheet.Domain.Tests.CellTypes;

public sealed class CellConfigurationApplyTests
{
    [Fact]
    public void Apply_UsesOverriddenValuesAndKeepsTheRest()
    {
        var defaults = new NumberCellConfiguration { DecimalPlaces = 2, MinValue = 0, MaxValue = 100, Unit = "Nm" };
        var overrides = new NumberCellConfiguration { MaxValue = 50 };

        var effective = defaults.Apply(overrides);

        Assert.Equal(new NumberCellConfiguration { DecimalPlaces = 2, MinValue = 0, MaxValue = 50, Unit = "Nm" }, effective);
    }

    [Fact]
    public void Apply_WithNoOverrides_ReturnsTheDefaults()
    {
        var defaults = new TextCellConfiguration { MaxLength = 200 };

        Assert.Equal(defaults, defaults.Apply(null));
    }

    [Fact]
    public void Apply_IgnoresOverridesForAnotherKind()
    {
        var defaults = new TextCellConfiguration { MaxLength = 200 };

        Assert.Equal(defaults, defaults.Apply(new NumberCellConfiguration { DecimalPlaces = 1 }));
    }

    [Fact]
    public void IsEmpty_IsTrueOnlyWhenNothingIsSet()
    {
        Assert.True(new TextCellConfiguration().IsEmpty());
        Assert.False(new TextCellConfiguration { Multiline = false }.IsEmpty());
    }

    [Fact]
    public void Validate_RefusesMinAboveMax()
    {
        var configuration = new NumberCellConfiguration { MinValue = 10, MaxValue = 1 };

        Assert.NotNull(configuration.Validate());
    }
}
