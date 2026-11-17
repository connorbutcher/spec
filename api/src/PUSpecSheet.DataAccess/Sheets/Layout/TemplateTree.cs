using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Sheets.Layout;

public sealed class TemplateTree
{
    private readonly Dictionary<int, DbTemplateSection> _byId;
    private readonly ILookup<int?, DbTemplateSection> _byParent;

    public TemplateTree(List<DbTemplateSection> sections, List<DbTemplateColumnBlock> columnBlocks)
    {
        ColumnBlocks = columnBlocks
            .OrderBy(block => block.DisplayOrder)
            .ThenBy(block => block.Id)
            .ToList();
        _byId = sections.ToDictionary(section => section.Id);
        _byParent = sections.ToLookup(section => section.ParentSectionId);
    }

    public IReadOnlyList<DbTemplateColumnBlock> ColumnBlocks { get; }

    public DbTemplateSection? Find(int templateSectionId)
    {
        return _byId.GetValueOrDefault(templateSectionId);
    }

    public IReadOnlyList<DbTemplateSection> ChildrenOf(int? parentTemplateSectionId)
    {
        return _byParent[parentTemplateSectionId]
            .OrderBy(section => section.DisplayOrder)
            .ThenBy(section => section.Id)
            .ToList();
    }
}
