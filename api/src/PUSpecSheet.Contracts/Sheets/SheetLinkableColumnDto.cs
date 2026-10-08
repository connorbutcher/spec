namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A column of a sheet table that a linked dropdown elsewhere on the sheet can take its choices from. The
/// column is every cell of the table built from the template cell <see cref="TemplateCellId"/>.
/// </summary>
public sealed record SheetLinkableColumnDto(int TemplateCellId, string Label);
