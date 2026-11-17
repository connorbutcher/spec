namespace PUSpecSheet.Abstractions.Sheets;

// One note on a sheet table, as stored (as a JSON array) on the table's revision.
public sealed record SheetTableNote
{
    public required string Note { get; init; }

    public required string SourceSystem { get; init; }

    public bool IsObsolete { get; init; }

    public int CreatedByUserId { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
