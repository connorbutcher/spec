namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// A row of a sheet section. A lock held by the viewer means they can edit it; a lock held by someone
/// else means it's read-only until they publish it or change it back. <see cref="IsPending"/> rows have never
/// been published, so only their author can see them.
/// </summary>
public sealed record SheetRowDto
{
    public required int Id { get; init; }

    public required Guid PublicId { get; init; }

    public required int TemplateRowId { get; init; }

    public required int DisplayOrder { get; init; }

    public SheetLockDto? Lock { get; init; }

    public required bool IsPending { get; init; }

    public required bool CanRemove { get; init; }

    public required IReadOnlyList<SheetCellDto> Cells { get; init; }

    public SheetChangeDto? LastChange { get; init; }
}
