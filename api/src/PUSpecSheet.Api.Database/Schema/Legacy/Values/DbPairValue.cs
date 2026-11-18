namespace PUSpecSheet.Api.Database.Schema.Legacy.Values;

public record DbPairValue
{
    public required int Id { get; init; }

    public required int SpecSheetTableCellId { get; set; }

    public required string? PairValueX { get; set; }

    public required string? PairValueY { get; set; }
}
