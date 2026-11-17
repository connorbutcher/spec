namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A note on a table, held on the table's revision.
/// </summary>
public record SheetTableNote
{
    public required string Note { get; init; }

    public required string SourceSystem { get; init; }

    public required bool IsObsolete { get; init; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }
}
