using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Schema.Cell;

/// <summary>
/// A table note as a table revision has it. The revision has the note if it has this row.
/// </summary>
public record DbSheetTableNoteValue
{
    public required int SheetTableRevisionId { get; init; }

    public required int SheetTableNoteId { get; init; }

    public required string Note { get; init; }

    public required bool IsObsolete { get; set; }

    public virtual DbSheetTableNote? SheetTableNote { get; init; }
}
