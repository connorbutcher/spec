using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.CellTypes.Styles;

[JsonConverter(typeof(JsonStringEnumConverter<CellTextAlign>))]
public enum CellTextAlign
{
    Left,

    Center,

    Right,
}
