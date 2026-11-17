using System.Text.Json;
using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.CellTypes.LabelledText;

namespace PUSpecSheet.Abstractions.Tests.CellTypes;

/// <summary>A labelled text cell's slots: set on the cell type, changed by the template cell, and changed again on the sheet.</summary>
public sealed class LabelledTextTests
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        AllowOutOfOrderMetadataProperties = true,
    };

    private static readonly LabelledTextCellConfiguration _hands = new()
    {
        MaxLength = 20,
        Slots = [new LabelledTextSlot { Key = "rh", Label = "RH" }, new LabelledTextSlot { Key = "lh", Label = "LH" }],
        LabelColor = "#dc2626",
    };

    [Fact]
    public void SettingsReadBackFromJson_AreEqualToTheOriginal()
    {
        CellInstanceSettings settings = new LabelledTextInstanceSettings { Slots = [new LabelledTextSlot
        {
            Key = "both",
            Label = "Both",
        }] };

        var json = JsonSerializer.Serialize(settings, _options);
        var read = JsonSerializer.Deserialize<CellInstanceSettings>(json, _options);

        Assert.Equal("""{"kind":"LabelledText","slots":[{"key":"both","label":"Both"}]}""", json);
        Assert.Equal(settings, read);
        Assert.Equal(settings.GetHashCode(), read!.GetHashCode());
    }

    [Fact]
    public void SettingsWithNoSlots_AreEmpty()
    {
        Assert.True(new LabelledTextInstanceSettings().IsEmpty);
        Assert.False(new LabelledTextInstanceSettings { Slots = [new LabelledTextSlot { Key = "rh", Label = "RH" }] }.IsEmpty);
    }

    [Fact]
    public void AnOverride_ReplacesOnlyWhatItSets()
    {
        var applied = (LabelledTextCellConfiguration)_hands.Apply(new LabelledTextCellConfiguration { Slots = [new LabelledTextSlot
        {
            Key = "upper",
            Label = "Upper",
        }] });

        Assert.Equal(20, applied.MaxLength);
        Assert.Equal("#dc2626", applied.LabelColor);
        Assert.Equal([new LabelledTextSlot { Key = "upper", Label = "Upper" }], applied.Slots);
    }

    [Fact]
    public void ConfigurationsWithTheSameSlots_AreEqual()
    {
        var copy = _hands with { Slots = [new LabelledTextSlot { Key = "rh", Label = "RH" }, new LabelledTextSlot
        {
            Key = "lh",
            Label = "LH",
        }] };

        Assert.Equal(_hands, copy);
        Assert.NotEqual(_hands, _hands with { Slots = [new LabelledTextSlot
        {
            Key = "lh",
            Label = "LH",
        }, new LabelledTextSlot { Key = "rh", Label = "RH" }] });
    }

    [Fact]
    public void TheSheetsSlots_WinOverTheTemplates()
    {
        var sheet = new LabelledTextInstanceSettings { Slots = [new LabelledTextSlot { Key = "both", Label = "Both" }] };

        Assert.Equal("both", LabelledTextSlots.Effective(_hands, sheet).Single().Key);
        Assert.Equal(["rh", "lh"], LabelledTextSlots.Effective(_hands, null).Select(slot => slot.Key));
        Assert.Equal(LabelledTextRules.DefaultSlots, LabelledTextSlots.Effective(new LabelledTextCellConfiguration(), null));
    }

    [Theory]
    [InlineData("RH", "RH", "lower-case")]
    [InlineData("rh", "Right hand side of the engine", "at most")]
    public void BadSlots_AreRefused(string key, string label, string expected)
    {
        var problem = (_hands with { Slots = [new LabelledTextSlot { Key = key, Label = label }] }).Validate();

        Assert.Contains(expected, problem, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoSlotsWithTheSameKey_AreRefused()
    {
        var problem = (_hands with { Slots = [new LabelledTextSlot
        {
            Key = "rh",
            Label = "RH",
        }, new LabelledTextSlot { Key = "rh", Label = "LH" }] }).Validate();

        Assert.Equal("Each value needs its own key.", problem);
    }
}
