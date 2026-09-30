using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

public sealed class TemplateSectionService(
    PuSpecSheetDbContext db,
    TableTemplateReader reader,
    TemplateVersionGuard guard) : ITemplateSectionService
{
    public async Task<TableTemplateDto> CreateAsync(CreateTemplateSectionRequest request, CancellationToken cancellationToken)
    {
        var versionId = request.TableTemplateVersionId;
        await guard.EnsureEditableAsync(versionId, cancellationToken);

        if (request.ParentSectionId is int parentId)
        {
            await EnsureCanHoldSectionsAsync(versionId, parentId, cancellationToken);
        }

        var lastOrder = await db.TemplateSections
            .Where(section => section.TableTemplateVersionId == versionId
                && section.ParentSectionId == request.ParentSectionId)
            .MaxAsync(section => (int?)section.DisplayOrder, cancellationToken);

        // The header is created with the table, so every section added here is an addable section.
        var section = new TemplateSection
        {
            TableTemplateVersionId = versionId,
            ParentSectionId = request.ParentSectionId,
            Name = request.Name.Trim(),
            DisplayOrder = (lastOrder ?? 0) + 1,
        };
        SectionInstanceRules.ApplyNewAddable(section);

        db.TemplateSections.Add(section);
        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadVersionAsync(versionId, cancellationToken);
    }

    public async Task<TableTemplateDto> UpdateAsync(
        int id,
        UpdateTemplateSectionRequest request,
        CancellationToken cancellationToken)
    {
        var section = await FindEditableAsync(id, cancellationToken);

        section.Name = request.Name.Trim();

        if (section.Role == SectionRole.Header)
        {
            SectionInstanceRules.ApplyHeader(section);
        }
        else
        {
            SectionInstanceRules.ApplyAddable(request, section);
        }

        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(section.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var section = await FindEditableAsync(id, cancellationToken);
        EnsureNotHeader(section, "move");

        var siblings = await db.TemplateSections
            .Where(candidate => candidate.TableTemplateVersionId == section.TableTemplateVersionId
                && candidate.ParentSectionId == section.ParentSectionId)
            .ToListAsync(cancellationToken);

        DisplayOrdering.Move(siblings, section, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(section.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var section = await FindEditableAsync(id, cancellationToken);
        EnsureNotHeader(section, "delete");

        // Child sections can't cascade in SQL Server (the version already cascades to every section), so
        // the whole subtree is removed here. Rows and cells cascade from their sections.
        var versionSections = await db.TemplateSections
            .Where(candidate => candidate.TableTemplateVersionId == section.TableTemplateVersionId)
            .ToListAsync(cancellationToken);

        db.TemplateSections.RemoveRange(CollectSubtree(section, versionSections));

        var siblings = versionSections
            .Where(candidate => candidate.ParentSectionId == section.ParentSectionId && candidate.Id != section.Id)
            .ToList();

        DisplayOrdering.Renumber(siblings, sibling => sibling.DisplayOrder, SetOrder);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(section.TableTemplateVersionId, cancellationToken);
    }

    private static List<TemplateSection> CollectSubtree(TemplateSection root, IReadOnlyList<TemplateSection> sections)
    {
        var childrenByParent = sections.ToLookup(section => section.ParentSectionId);
        var subtree = new List<TemplateSection>();
        var pending = new Stack<TemplateSection>([root]);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            subtree.Add(current);
            foreach (var child in childrenByParent[current.Id])
            {
                pending.Push(child);
            }
        }

        return subtree;
    }

    private static void SetOrder(TemplateSection section, int order)
    {
        section.DisplayOrder = order;
    }

    /// <summary>The header is part of every table: it can't be moved or removed on its own.</summary>
    private static void EnsureNotHeader(TemplateSection section, string action)
    {
        if (section.Role == SectionRole.Header)
        {
            throw new ConflictException($"The header is part of the table and can't be {action}d.");
        }
    }

    private async Task EnsureCanHoldSectionsAsync(int versionId, int parentId, CancellationToken cancellationToken)
    {
        var parent = await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.Id == parentId && section.TableTemplateVersionId == versionId)
            .Select(section => new { section.Role })
            .SingleOrDefaultAsync(cancellationToken);

        if (parent is null)
        {
            throw new InvalidRequestException($"Section {parentId} isn't part of this version.");
        }

        // An addable section can hold its own rows and sub-sections together; the header holds rows only.
        if (parent.Role == SectionRole.Header)
        {
            throw new ConflictException("The header holds rows only. Sub-sections go in addable sections.");
        }
    }

    private async Task<TemplateSection> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var section = await db.TemplateSections.FindAsync([id], cancellationToken);
        if (section is null)
        {
            throw new NotFoundException($"Section {id} was not found.");
        }

        await guard.EnsureEditableAsync(section.TableTemplateVersionId, cancellationToken);
        return section;
    }
}
