using System.Text.Json;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Domain.Tests.CellTypes;

/// <summary>The settings chosen for a cell on the sheet are stored and sent as JSON named by cell kind.</summary>
public sealed class CellInstanceSettingsTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        AllowOutOfOrderMetadataProperties = true,
    };

    [Fact]
    public void Serialize_WritesTheKindAsTheDiscriminator()
    {
        CellInstanceSettings settings = new LinkedDropdownInstanceSettings { SourceSheetTableId = 4, SourceTemplateCellId = 15 };

        var json = JsonSerializer.Serialize(settings, Options);

        Assert.Equal("""{"kind":"LinkedDropdown","sourceSheetTableId":4,"sourceTemplateCellId":15}""", json);
    }

    [Fact]
    public void Deserialize_ReadsTheDerivedTypeWhereverTheKindIs()
    {
        var settings = JsonSerializer.Deserialize<CellInstanceSettings>(
            """{"sourceSheetTableId":4,"kind":"LinkedDropdown","sourceTemplateCellId":15}""",
            Options);

        Assert.Equal(new LinkedDropdownInstanceSettings { SourceSheetTableId = 4, SourceTemplateCellId = 15 }, settings);
    }

    [Fact]
    public void SettingsWithNothingSet_AreEmpty()
    {
        Assert.True(new LinkedDropdownInstanceSettings().IsEmpty);
        Assert.False(new LinkedDropdownInstanceSettings { SourceSheetTableId = 4 }.IsEmpty);
    }

    [Fact]
    public void EveryKindWithSettings_StartsWithSettingsOfThatKind()
    {
        foreach (var kind in Enum.GetValues<CellKind>())
        {
            var empty = CellInstanceSettingsCatalog.CreateEmpty(kind);

            Assert.Equal(empty is not null, kind.HasInstanceSettings());
            if (empty is not null)
            {
                Assert.Equal(kind, empty.Kind);
                Assert.True(empty.IsEmpty);

                // It must also be registered on CellInstanceSettings, or it couldn't be stored.
                Assert.Contains($"\"kind\":\"{kind}\"", JsonSerializer.Serialize(empty, Options));
            }
        }
    }

    [Fact]
    public void OnlyALinkedDropdown_HasSettingsToday()
    {
        Assert.True(CellKind.LinkedDropdown.HasInstanceSettings());
        Assert.False(CellKind.Text.HasInstanceSettings());
        Assert.False(CellKind.TextDropdown.HasInstanceSettings());
    }
}
