namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The repositories of a sheet and the items on it, for the classes that read the whole sheet.</summary>
public sealed class SheetStructureRepositories
{
    private readonly ISheetRepository sheets;
    private readonly ISheetTableRepository tables;
    private readonly ISheetSectionRepository sections;
    private readonly ISheetRowRepository rows;
    private readonly ISheetColumnBlockRepository columnBlocks;

    public SheetStructureRepositories(
        ISheetRepository sheets,
        ISheetTableRepository tables,
        ISheetSectionRepository sections,
        ISheetRowRepository rows,
        ISheetColumnBlockRepository columnBlocks)
    {
        this.sheets = sheets;
        this.tables = tables;
        this.sections = sections;
        this.rows = rows;
        this.columnBlocks = columnBlocks;
    }

    public ISheetRepository Sheets => sheets;

    public ISheetTableRepository Tables => tables;

    public ISheetSectionRepository Sections => sections;

    public ISheetRowRepository Rows => rows;

    public ISheetColumnBlockRepository ColumnBlocks => columnBlocks;
}
