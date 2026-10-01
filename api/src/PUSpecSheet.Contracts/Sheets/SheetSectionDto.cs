using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A section copy on a sheet table. Its own rows come first, then its sub-sections. The template's
/// name, role and instance counts are carried along so the screen can lay it out like the template.
/// </summary>
public sealed record SheetSectionDto(
    int Id,
    Guid PublicId,
    int TemplateSectionId,
    string Name,
    SectionRole Role,
    int MinInstances,
    int? MaxInstances,
    int InitialInstances,
    int DisplayOrder,
    SheetLockDto? Lock,
    bool IsPending,
    bool CanRemove,
    IReadOnlyList<SheetRowDto> Rows,
    IReadOnlyList<SheetSectionDto> Sections,
    IReadOnlyList<AddableSectionDto> AddableSections,
    IReadOnlyList<AddableRowDto> AddableRows,
    SheetChangeDto? LastChange);
