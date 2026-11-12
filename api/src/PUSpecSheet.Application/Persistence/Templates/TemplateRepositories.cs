namespace PUSpecSheet.Application.Persistence.Templates;

/// <summary>The repositories of a template and everything in its versions, for the classes that read whole templates.</summary>
public sealed class TemplateRepositories(
    ITableTemplateRepository templates,
    ITemplateSectionRepository sections,
    ITemplateRowRepository rows,
    ITemplateCellRepository cells,
    ITemplateColumnBlockRepository columnBlocks)
{
    public ITableTemplateRepository Templates => templates;

    public ITemplateSectionRepository Sections => sections;

    public ITemplateRowRepository Rows => rows;

    public ITemplateCellRepository Cells => cells;

    public ITemplateColumnBlockRepository ColumnBlocks => columnBlocks;
}
