namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The revision repositories of every kind of sheet item, for the classes that work on all of them at once.</summary>
public sealed class SheetRevisionRepositories
{
    private readonly ISheetTableRevisionRepository tables;
    private readonly ISheetSectionRevisionRepository sections;
    private readonly ISheetRowRevisionRepository rows;
    private readonly ISheetColumnBlockRevisionRepository columnBlocks;

    public SheetRevisionRepositories(
        ISheetTableRevisionRepository tables,
        ISheetSectionRevisionRepository sections,
        ISheetRowRevisionRepository rows,
        ISheetColumnBlockRevisionRepository columnBlocks)
    {
        this.tables = tables;
        this.sections = sections;
        this.rows = rows;
        this.columnBlocks = columnBlocks;
    }

    public ISheetTableRevisionRepository Tables => tables;

    public ISheetSectionRevisionRepository Sections => sections;

    public ISheetRowRevisionRepository Rows => rows;

    public ISheetColumnBlockRevisionRepository ColumnBlocks => columnBlocks;
}
