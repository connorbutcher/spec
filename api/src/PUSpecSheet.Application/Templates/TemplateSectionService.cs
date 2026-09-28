using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

public sealed class TemplateSectionService(PuSpecSheetDbContext db, TableTemplateReader reader) : ITemplateSectionService
{
    public async Task<TableTemplateDto> CreateAsync(CreateTemplateSectionRequest request, CancellationToken cancellationToken)
    {
        var templateExists = await db.TableTemplates.AnyAsync(
            template => template.Id == request.TableTemplateId,
            cancellationToken);

        if (!templateExists)
        {
            throw new NotFoundException($"Table template {request.TableTemplateId} was not found.");
        }

        if (request.ParentSectionId is int parentId)
        {
            await EnsureCanHoldSectionsAsync(request.TableTemplateId, parentId, cancellationToken);
        }

        var lastOrder = await db.TemplateSections
            .Where(section => section.TableTemplateId == request.TableTemplateId
                && section.ParentSectionId == request.ParentSectionId)
            .MaxAsync(section => (int?)section.DisplayOrder, cancellationToken);

        db.TemplateSections.Add(new TemplateSection
        {
            TableTemplateId = request.TableTemplateId,
            ParentSectionId = request.ParentSectionId,
            Name = request.Name.Trim(),
            DisplayOrder = (lastOrder ?? 0) + 1,
        });

        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(request.TableTemplateId, cancellationToken);
    }

    public async Task<TableTemplateDto> UpdateAsync(
        int id,
        UpdateTemplateSectionRequest request,
        CancellationToken cancellationToken)
    {
        var section = await FindAsync(id, cancellationToken);

        section.Name = request.Name.Trim();
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadAsync(section.TableTemplateId, cancellationToken);
    }

    public async Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var section = await FindAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(section, cancellationToken);

        DisplayOrdering.Move(siblings, section, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadAsync(section.TableTemplateId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var section = await FindAsync(id, cancellationToken);

        // Child sections can't cascade in SQL Server (the template already cascades to every section), so
        // the whole subtree is removed here. Rows and cells cascade from their sections.
        var templateSections = await db.TemplateSections
            .Where(candidate => candidate.TableTemplateId == section.TableTemplateId)
            .ToListAsync(cancellationToken);

        var subtree = CollectSubtree(section, templateSections);
        db.TemplateSections.RemoveRange(subtree);

        var siblings = templateSections
            .Where(candidate => candidate.ParentSectionId == section.ParentSectionId && candidate.Id != section.Id)
            .ToList();

        DisplayOrdering.Renumber(siblings, sibling => sibling.DisplayOrder, SetOrder);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadAsync(section.TableTemplateId, cancellationToken);
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

    private async Task EnsureCanHoldSectionsAsync(int templateId, int parentId, CancellationToken cancellationToken)
    {
        var parent = await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.Id == parentId && section.TableTemplateId == templateId)
            .Select(section => new { section.Name, HasRows = section.Rows.Any() })
            .SingleOrDefaultAsync(cancellationToken);

        if (parent is null)
        {
            throw new InvalidRequestException($"Section {parentId} isn't part of this template.");
        }

        if (parent.HasRows)
        {
            throw new ConflictException(
                $"\"{parent.Name}\" has rows. A section holds either sections or rows, so delete its rows first.");
        }
    }

    private async Task<TemplateSection> FindAsync(int id, CancellationToken cancellationToken)
    {
        var section = await db.TemplateSections.FindAsync([id], cancellationToken);
        if (section is null)
        {
            throw new NotFoundException($"Section {id} was not found.");
        }

        return section;
    }

    private Task<List<TemplateSection>> LoadSiblingsAsync(TemplateSection section, CancellationToken cancellationToken)
    {
        return db.TemplateSections
            .Where(candidate => candidate.TableTemplateId == section.TableTemplateId
                && candidate.ParentSectionId == section.ParentSectionId)
            .ToListAsync(cancellationToken);
    }
}
