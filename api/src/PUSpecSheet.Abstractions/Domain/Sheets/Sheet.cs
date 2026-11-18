using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A sheet as it was shown at a moment: its published revisions then, or with its drafts laid over them.
/// </summary>
public record Sheet
{
    public required int Id { get; init; }

    public required Guid ReferenceId { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    /// <summary>
    /// The version the sheet was published as at <see cref="AsOf"/>, or null before it has been published.
    /// </summary>
    public SheetVersion? Version { get; init; }

    public required DateTime AsOf { get; init; }

    /// <summary>
    /// Whether the sheet is shown with its drafts, each item's draft where it has one.
    /// </summary>
    public required bool IsDraft { get; init; }

    public SpecSheetPhase? SpecSheetPhase { get; init; }

    public SpecSheetType? SpecSheetType { get; init; }

    public required ICollection<SheetTable> Tables { get; init; } = [];

    public required ICollection<SheetProperty> Properties { get; init; } = [];

    public required ICollection<SheetLinkedTable> LinkedTables { get; init; } = [];

    public required ICollection<SheetExternalReference> ExternalReferences { get; init; } = [];
}
