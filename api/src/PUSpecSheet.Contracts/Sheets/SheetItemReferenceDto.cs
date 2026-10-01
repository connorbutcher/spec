namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// Where a sheet item lives, found from its public identifier. The identifier is the same in every version
/// of the sheet, so this answers for the item whichever version it is being asked about. The ids of the
/// parents are null where they don't apply (a table has no section, for instance).
/// </summary>
public sealed record SheetItemReferenceDto(
    SheetItemKind Kind,
    Guid PublicId,
    int Id,
    int SheetId,
    Guid SheetPublicId,
    int PhaseId,
    int SheetTypeId,
    int? TableId,
    int? SectionId,
    int? RowId,
    bool IsDeleted);
