using System.Text.Json;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Domain.Tests.CellTypes;

public sealed class CellConfigurationJsonTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        AllowOutOfOrderMetadataProperties = true,
    };

    [Fact]
    public void Serialize_WritesTheKindAsTheDiscriminator()
    {
        CellConfiguration configuration = new NumberDropdownCellConfiguration { Unit = "mm" };

        var json = JsonSerializer.Serialize(configuration, Options);

        Assert.Equal("""{"kind":"NumberDropdown","decimalPlaces":null,"unit":"mm"}""", json);
    }

    [Fact]
    public void Deserialize_ReadsTheDerivedTypeWhereverTheKindIs()
    {
        var configuration = JsonSerializer.Deserialize<CellConfiguration>("""{"maxLength":20,"kind":"Text"}""", Options);

        Assert.Equal(new TextCellConfiguration { MaxLength = 20 }, configuration);
    }

    [Fact]
    public void Style_RoundTripsWithTheAlignmentAsText()
    {
        var style = new CellStyle { Align = CellTextAlign.Center, Italic = true };

        var json = JsonSerializer.Serialize(style, Options);

        Assert.Contains("\"align\":\"Center\"", json);
        Assert.Equal(style, JsonSerializer.Deserialize<CellStyle>(json, Options));
    }
}
