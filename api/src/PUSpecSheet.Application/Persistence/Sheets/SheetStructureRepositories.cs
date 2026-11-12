namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The repositories of a sheet and the items on it, for the classes that read the whole sheet.</summary>
public sealed class SheetStructureRepositories(
    ISheetRepository sheets,
    ISheetTableRepository tables,
    ISheetSectionRepository sections,
    ISheetRowRepository rows,
    ISheetColumnBlockRepository columnBlocks)
{
    public ISheetRepository Sheets => sheets;

    public ISheetTableRepository Tables => tables;

    public ISheetSectionRepository Sections => sections;

    public ISheetRowRepository Rows => rows;

    public ISheetColumnBlockRepository ColumnBlocks => columnBlocks;
}
