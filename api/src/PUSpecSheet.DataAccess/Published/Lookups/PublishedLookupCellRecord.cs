using PUSpecSheet.Abstractions.CellTypes;

namespace PUSpecSheet.DataAccess.Published.Lookups;

/// <summary>
/// One cell read for a lookup match, with the row revision that holds its value: enough to place it, tell
/// a heading from a value, and name it by its lookup key.
/// </summary>
public sealed record PublishedLookupCellRecord
{
    public required int RowRevisionId { get; init; }

    public required int RowId { get; init; }

    public required Guid RowPublicId { get; init; }

    public required int SectionId { get; init; }

    public required int RowOrder { get; init; }

    public required int CellId { get; init; }

    public required Guid CellPublicId { get; init; }

    public int? ColumnBlockId { get; init; }

    public required int Column { get; init; }

    public required CellKind Kind { get; init; }

    public string? Caption { get; init; }

    public string? LookupKey { get; init; }

    public required bool IsHeader { get; init; }
}
