using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A note of a table, written once by its author, with its text and whether it is obsolete in the table revision that
/// is shown.
/// </summary>
public record SheetTableNote
{
    public required int Id { get; init; }

    public required int SheetTableId { get; init; }

    public required string Note { get; init; }

    public required bool IsObsolete { get; init; }

    public required string SourceSystem { get; init; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public User? CreatedByUser { get; init; }
}
