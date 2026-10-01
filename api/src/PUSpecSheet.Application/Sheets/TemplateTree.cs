using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets;

/// <summary>One template version's sections with their rows and cells, for building sheet content from it.</summary>
public sealed class TemplateTree
{
    private readonly Dictionary<int, TemplateSection> byId;
    private readonly ILookup<int?, TemplateSection> byParent;

    private TemplateTree(List<TemplateSection> sections)
    {
        byId = sections.ToDictionary(section => section.Id);
        byParent = sections.ToLookup(section => section.ParentSectionId);
    }

    public static async Task<TemplateTree> LoadAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        var sections = await db.TemplateSections
            .AsNoTracking()
            .Include(section => section.Rows)
            .ThenInclude(row => row.Cells)
            .Where(section => section.TableTemplateVersionId == versionId)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        return new TemplateTree(sections);
    }

    public TemplateSection? Find(int templateSectionId)
    {
        return byId.GetValueOrDefault(templateSectionId);
    }

    /// <summary>The sections directly under a section (or the top-level ones for null), in template order.</summary>
    public IReadOnlyList<TemplateSection> ChildrenOf(int? parentTemplateSectionId)
    {
        return byParent[parentTemplateSectionId]
            .OrderBy(section => section.DisplayOrder)
            .ThenBy(section => section.Id)
            .ToList();
    }
}
