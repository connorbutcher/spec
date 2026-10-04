using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A table on a sheet, built from one version of a table template and keeping that layout. A horizontal
/// table also has <see cref="ColumnBlocks"/>, left to right, each running through every row.
/// </summary>
public sealed record SheetTableDto(
    int Id,
    Guid PublicId,
    int TableTemplateId,
    string TemplateName,
    int TemplateVersionNumber,
    TemplateOrientation Orientation,
    string? Title,
    int DisplayOrder,
    SheetLockDto? Lock,
    bool IsPending,
    IReadOnlyList<SheetSectionDto> Sections,
    IReadOnlyList<AddableSectionDto> AddableSections,
    IReadOnlyList<SheetColumnBlockDto> ColumnBlocks,
    IReadOnlyList<AddableColumnBlockDto> AddableColumnBlocks);
