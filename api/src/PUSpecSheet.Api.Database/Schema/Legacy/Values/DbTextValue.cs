namespace PUSpecSheet.Api.Database.Schema.Legacy.Values;

public record DbTextValue
{
    public required int Id { get; init; }

    public required int SpecSheetTableCellId { get; set; }

    public required string? Value { get; set; }
}
