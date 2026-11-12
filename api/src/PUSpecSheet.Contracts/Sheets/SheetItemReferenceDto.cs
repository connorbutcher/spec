namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// Where a sheet item lives, found from its public identifier. The identifier is the same in every version
/// of the sheet, so this answers for the item whichever version it is being asked about. The ids of the
/// parents are null where they don't apply (a table has no section, for instance).
/// </summary>
public sealed record SheetItemReferenceDto
{
    public SheetItemReferenceDto(
        SheetItemKind kind,
        Guid publicId,
        int id,
        int sheetId,
        Guid sheetPublicId,
        int phaseId,
        int sheetTypeId,
        int? tableId,
        int? sectionId,
        int? rowId,
        bool isDeleted)
    {
        this.Kind = kind;
        this.PublicId = publicId;
        this.Id = id;
        this.SheetId = sheetId;
        this.SheetPublicId = sheetPublicId;
        this.PhaseId = phaseId;
        this.SheetTypeId = sheetTypeId;
        this.TableId = tableId;
        this.SectionId = sectionId;
        this.RowId = rowId;
        this.IsDeleted = isDeleted;
    }

    public SheetItemKind Kind { get; init; }

    public Guid PublicId { get; init; }

    public int Id { get; init; }

    public int SheetId { get; init; }

    public Guid SheetPublicId { get; init; }

    public int PhaseId { get; init; }

    public int SheetTypeId { get; init; }

    public int? TableId { get; init; }

    public int? SectionId { get; init; }

    public int? RowId { get; init; }

    public bool IsDeleted { get; init; }
}
