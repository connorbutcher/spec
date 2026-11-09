using System.Text.Json;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.CellTypes.LabelledText;

namespace PUSpecSheet.Domain.Tests.CellTypes;

/// <summary>A labelled text cell's slots: set on the cell type, changed by the template cell, and changed again on the sheet.</summary>
public sealed class LabelledTextTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        AllowOutOfOrderMetadataProperties = true,
    };

    private static readonly LabelledTextCellConfiguration Hands = new()
    {
        MaxLength = 20,
        Slots = [new LabelledTextSlot("rh", "RH"), new LabelledTextSlot("lh", "LH")],
        LabelColor = "#dc2626",
    };

    [Fact]
    public void SettingsReadBackFromJson_AreEqualToTheOriginal()
    {
        CellInstanceSettings settings = new LabelledTextInstanceSettings { Slots = [new LabelledTextSlot("both", "Both")] };

        var json = JsonSerializer.Serialize(settings, Options);
        var read = JsonSerializer.Deserialize<CellInstanceSettings>(json, Options);

        Assert.Equal("""{"kind":"LabelledText","slots":[{"key":"both","label":"Both"}]}""", json);
        Assert.Equal(settings, read);
        Assert.Equal(settings.GetHashCode(), read!.GetHashCode());
    }

    [Fact]
    public void SettingsWithNoSlots_AreEmpty()
    {
        Assert.True(new LabelledTextInstanceSettings().IsEmpty);
        Assert.False(new LabelledTextInstanceSettings { Slots = [new LabelledTextSlot("rh", "RH")] }.IsEmpty);
    }

    [Fact]
    public void AnOverride_ReplacesOnlyWhatItSets()
    {
        var applied = (LabelledTextCellConfiguration)Hands.Apply(new LabelledTextCellConfiguration { Slots = [new LabelledTextSlot("upper", "Upper")] });

        Assert.Equal(20, applied.MaxLength);
        Assert.Equal("#dc2626", applied.LabelColor);
        Assert.Equal([new LabelledTextSlot("upper", "Upper")], applied.Slots);
    }

    [Fact]
    public void ConfigurationsWithTheSameSlots_AreEqual()
    {
        var copy = Hands with { Slots = [new LabelledTextSlot("rh", "RH"), new LabelledTextSlot("lh", "LH")] };

        Assert.Equal(Hands, copy);
        Assert.NotEqual(Hands, Hands with { Slots = [new LabelledTextSlot("lh", "LH"), new LabelledTextSlot("rh", "RH")] });
    }

    [Fact]
    public void TheSheetsSlots_WinOverTheTemplates()
    {
        var sheet = new LabelledTextInstanceSettings { Slots = [new LabelledTextSlot("both", "Both")] };

        Assert.Equal("both", LabelledTextSlots.Effective(Hands, sheet).Single().Key);
        Assert.Equal(["rh", "lh"], LabelledTextSlots.Effective(Hands, null).Select(slot => slot.Key));
        Assert.Equal(LabelledTextRules.DefaultSlots, LabelledTextSlots.Effective(new LabelledTextCellConfiguration(), null));
    }

    [Theory]
    [InlineData("RH", "RH", "lower-case")]
    [InlineData("rh", "Right hand side of the engine", "at most")]
    public void BadSlots_AreRefused(string key, string label, string expected)
    {
        var problem = (Hands with { Slots = [new LabelledTextSlot(key, label)] }).Validate();

        Assert.Contains(expected, problem, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoSlotsWithTheSameKey_AreRefused()
    {
        var problem = (Hands with { Slots = [new LabelledTextSlot("rh", "RH"), new LabelledTextSlot("rh", "LH")] }).Validate();

        Assert.Equal("Each value needs its own key.", problem);
    }
}
