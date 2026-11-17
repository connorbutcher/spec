namespace PUSpecSheet.Query.Models.Table;

public record SlimPUSpecSheetTableNote
{
    /// <summary>
    /// Gets the unique identifier of the PU Spec Sheet table note.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the note.
    /// </summary>
    public required string Note { get; init; }

    /// <summary>
    /// Gets the system the note was left from. e.g. "puspecsheet", "buildcards".
    /// </summary>
    public required string SourceSystem { get; init; }

    /// <summary>
    /// Gets whether the note is obsolete.
    /// </summary>
    public required bool IsObsolete { get; init; }

    /// <summary>
    /// Gets the name of the user who left the note.
    /// </summary>
    public required string CreatedBy { get; set; }

    /// <summary>
    /// Gets the date and time when the note was left.
    /// </summary>
    public required DateTime CreatedDateTime { get; set; }
}
