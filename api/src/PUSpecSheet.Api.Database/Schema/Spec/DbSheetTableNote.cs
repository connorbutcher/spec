using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A note on a sheet table, with who wrote it and when. What it says and whether it is obsolete are a <see cref="DbSheetTableNoteValue"/> in each table revision that has it.
/// </summary>
public record DbSheetTableNote
{
    public required int Id { get; init; }

    public required int SheetTableId { get; init; }

    public required string SourceSystem { get; init; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbUser? CreatedByUser { get; init; }
}
