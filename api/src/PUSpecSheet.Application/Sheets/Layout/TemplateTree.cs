using PUSpecSheet.Application.Templates.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Layout;

/// <summary>One template version's sections with their rows and cells, for building sheet content from it.</summary>
public sealed class TemplateTree
{
    private readonly Dictionary<int, DbTemplateSection> byId;
    private readonly ILookup<int?, DbTemplateSection> byParent;

    private TemplateTree(List<DbTemplateSection> sections, List<DbTemplateColumnBlock> columnBlocks)
    {
        ColumnBlocks = columnBlocks
            .OrderBy(block => block.DisplayOrder)
            .ThenBy(block => block.Id)
            .ToList();
        byId = sections.ToDictionary(section => section.Id);
        byParent = sections.ToLookup(section => section.ParentSectionId);
    }

    public static async Task<TemplateTree> LoadAsync(
        PuSpecSheetDbContext db,
        int versionId,
        CancellationToken cancellationToken)
    {
        return new TemplateTree(
            await TemplateSectionQueries.ListForLayoutAsync(db, versionId, cancellationToken),
            await TemplateColumnBlockQueries.ListAsync(db, versionId, cancellationToken));
    }

    /// <summary>The version's column blocks, left to right. Empty for a vertical table.</summary>
    public IReadOnlyList<DbTemplateColumnBlock> ColumnBlocks { get; }

    public DbTemplateSection? Find(int templateSectionId)
    {
        return byId.GetValueOrDefault(templateSectionId);
    }

    /// <summary>The sections directly under a section (or the top-level ones for null), in template order.</summary>
    public IReadOnlyList<DbTemplateSection> ChildrenOf(int? parentTemplateSectionId)
    {
        return byParent[parentTemplateSectionId]
            .OrderBy(section => section.DisplayOrder)
            .ThenBy(section => section.Id)
            .ToList();
    }
}
