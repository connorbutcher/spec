namespace PUSpecSheet.Application.Persistence.Templates;

/// <summary>The repositories of a template and everything in its versions, for the classes that read whole templates.</summary>
public sealed class TemplateRepositories
{
    private readonly ITableTemplateRepository templates;
    private readonly ITemplateSectionRepository sections;
    private readonly ITemplateRowRepository rows;
    private readonly ITemplateCellRepository cells;
    private readonly ITemplateColumnBlockRepository columnBlocks;

    public TemplateRepositories(
        ITableTemplateRepository templates,
        ITemplateSectionRepository sections,
        ITemplateRowRepository rows,
        ITemplateCellRepository cells,
        ITemplateColumnBlockRepository columnBlocks)
    {
        this.templates = templates;
        this.sections = sections;
        this.rows = rows;
        this.cells = cells;
        this.columnBlocks = columnBlocks;
    }

    public ITableTemplateRepository Templates => templates;

    public ITemplateSectionRepository Sections => sections;

    public ITemplateRowRepository Rows => rows;

    public ITemplateCellRepository Cells => cells;

    public ITemplateColumnBlockRepository ColumnBlocks => columnBlocks;
}
