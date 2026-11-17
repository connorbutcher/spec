using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Domain.Values;

[JsonDerivedType(typeof(TextValue), typeDiscriminator: "text")]
[JsonDerivedType(typeof(NumericValue), typeDiscriminator: "numeric")]
[JsonDerivedType(typeof(PairValue), typeDiscriminator: "pair")]
[JsonDerivedType(typeof(LinkedSpecSheetValue), typeDiscriminator: "linkedSpecSheet")]
public record CellValue
{
    public required int Id { get; init; }

    public required int SpecSheetTableCellId { get; set; }
}
