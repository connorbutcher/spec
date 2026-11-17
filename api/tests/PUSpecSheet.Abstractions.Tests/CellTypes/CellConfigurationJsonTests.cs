using System.Text.Json;
using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.Styles;

namespace PUSpecSheet.Abstractions.Tests.CellTypes;

public sealed class CellConfigurationJsonTests
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        AllowOutOfOrderMetadataProperties = true,
    };

    [Fact]
    public void Serialize_WritesTheKindAsTheDiscriminator()
    {
        CellConfiguration configuration = new NumberDropdownCellConfiguration { Unit = "mm" };

        var json = JsonSerializer.Serialize(configuration, _options);

        Assert.Equal("""{"kind":"NumberDropdown","decimalPlaces":null,"unit":"mm"}""", json);
    }

    [Fact]
    public void Deserialize_ReadsTheDerivedTypeWhereverTheKindIs()
    {
        var configuration = JsonSerializer.Deserialize<CellConfiguration>("""{"maxLength":20,"kind":"Text"}""", _options);

        Assert.Equal(new TextCellConfiguration { MaxLength = 20 }, configuration);
    }

    [Fact]
    public void Style_RoundTripsWithTheAlignmentAsText()
    {
        var style = new CellStyle { Align = CellTextAlign.Center, Italic = true };

        var json = JsonSerializer.Serialize(style, _options);

        Assert.Contains("\"align\":\"Center\"", json);
        Assert.Equal(style, JsonSerializer.Deserialize<CellStyle>(json, _options));
    }
}
