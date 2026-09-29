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

        // At the top level, the first section is the table's header and the rest are sections people add
        // on the sheet. Inside a section, new sections start as a single block that's always there.
        var isHeader = request.ParentSectionId is null && !await HasHeaderAsync(versionId, null, cancellationToken);
        var isAddable = request.ParentSectionId is null && !isHeader;

        db.TemplateSections.Add(new TemplateSection
        {
            TableTemplateVersionId = versionId,
            ParentSectionId = request.ParentSectionId,
            Name = request.Name.Trim(),
            DisplayOrder = (lastOrder ?? 0) + 1,
            Role = isAddable ? SectionRole.Repeating : SectionRole.Fixed,
            MinInstances = isAddable ? 0 : 1,
            MaxInstances = isAddable ? null : 1,
            InitialInstances = isAddable ? 0 : 1,
        });

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

        var isTopLevel = section.ParentSectionId is null;
        if (isTopLevel && request.Role == SectionRole.Fixed
            && await HasHeaderAsync(section.TableTemplateVersionId, section.Id, cancellationToken))
        {
            throw new ConflictException("This table already has a header. Other top-level sections are added on the sheet.");
        }

        SectionInstanceRules.Apply(request, section, isTopLevel);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(section.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var section = await FindEditableAsync(id, cancellationToken);
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

    /// <summary>Whether the version has a top-level fixed section (its header) other than <paramref name="exceptId"/>.</summary>
    private Task<bool> HasHeaderAsync(int versionId, int? exceptId, CancellationToken cancellationToken)
    {
        return db.TemplateSections.AnyAsync(
            section => section.TableTemplateVersionId == versionId
                && section.ParentSectionId == null
                && section.Role == SectionRole.Fixed
                && section.Id != exceptId,
            cancellationToken);
    }

    private async Task EnsureCanHoldSectionsAsync(int versionId, int parentId, CancellationToken cancellationToken)
    {
        var parent = await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.Id == parentId && section.TableTemplateVersionId == versionId)
            .Select(section => new { section.Name, HasRows = section.Rows.Any() })
            .SingleOrDefaultAsync(cancellationToken);

        if (parent is null)
        {
            throw new InvalidRequestException($"Section {parentId} isn't part of this version.");
        }

        if (parent.HasRows)
        {
            throw new ConflictException(
                $"\"{parent.Name}\" has rows. A section holds either sections or rows, so delete its rows first.");
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
