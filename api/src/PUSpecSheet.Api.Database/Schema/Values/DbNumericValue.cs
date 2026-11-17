namespace PUSpecSheet.Api.Database.Schema.Values;

public record DbNumericValue
{
    public required int Id { get; init; }

    public required int SpecSheetTableCellId { get; set; }

    public required double? Value { get; set; }
}
