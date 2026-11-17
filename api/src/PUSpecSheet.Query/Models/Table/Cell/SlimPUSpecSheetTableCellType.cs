namespace PUSpecSheet.Query.Models.Table.Cell;

public record SlimPUSpecSheetTableCellType
{
    public required int Id { get; init; }

    public required Guid Key { get; init; }

    public required string Title { get; init; }

    public required string Type { get; init; }
}
