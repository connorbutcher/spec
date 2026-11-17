using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Templates.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates.Sections;

public sealed class TemplateSectionsService : ITemplateSectionsService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly TableTemplateReader _reader;
    private readonly TemplateVersionGuard _guard;

    public TemplateSectionsService(
        PuSpecSheetDbContext db,
        TableTemplateReader reader,
        TemplateVersionGuard guard)
    {
        _db = db;
        _reader = reader;
        _guard = guard;
    }

    public async Task<TableTemplateDto> CreateAsync(CreateTemplateSectionRequest request, CancellationToken cancellationToken)
    {
        var versionId = request.TableTemplateVersionId;
        await _guard.EnsureEditableAsync(versionId, cancellationToken);

        if (request.ParentSectionId is int parentId)
        {
            await EnsureCanHoldSectionsAsync(versionId, parentId, cancellationToken);
        }

        var lastOrder = await TemplateSectionQueries.GetLastDisplayOrderAsync(_db, versionId, request.ParentSectionId, cancellationToken);

        // The header is created with the table, so every section added here is an addable section.
        var section = new DbTemplateSection
        {
            TableTemplateVersionId = versionId,
            ParentSectionId = request.ParentSectionId,
            Name = request.Name.Trim(),
            DisplayOrder = (lastOrder ?? 0) + 1,
        };
        SectionInstanceRules.ApplyNewAddable(section);

        _db.TemplateSections.Add(section);
        await _db.SaveChangesAsync(cancellationToken);
        return await _reader.ReadVersionAsync(versionId, cancellationToken);
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

        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadVersionAsync(section.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var section = await FindEditableAsync(id, cancellationToken);
        EnsureNotHeader(section, "move");

        var siblings = await TemplateSectionQueries.ListSiblingsForEditAsync(_db, section.TableTemplateVersionId, section.ParentSectionId, cancellationToken);

        DisplayOrdering.Move(siblings, section, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadVersionAsync(section.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var section = await FindEditableAsync(id, cancellationToken);
        EnsureNotHeader(section, "delete");

        // Child sections can't cascade in SQL Server (the version already cascades to every section), so
        // the whole subtree is removed here. Rows and cells cascade from their sections.
        var versionSections = await TemplateSectionQueries.ListForEditAsync(_db, section.TableTemplateVersionId, cancellationToken);

        _db.TemplateSections.RemoveRange(CollectSubtree(section, versionSections));

        var siblings = versionSections
            .Where(candidate => candidate.ParentSectionId == section.ParentSectionId && candidate.Id != section.Id)
            .ToList();

        DisplayOrdering.Renumber(siblings, sibling => sibling.DisplayOrder, SetOrder);
        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadVersionAsync(section.TableTemplateVersionId, cancellationToken);
    }

    private static List<DbTemplateSection> CollectSubtree(DbTemplateSection root, IReadOnlyList<DbTemplateSection> sections)
    {
        var childrenByParent = sections.ToLookup(section => section.ParentSectionId);
        var subtree = new List<DbTemplateSection>();
        var pending = new Stack<DbTemplateSection>([root]);

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

    private static void SetOrder(DbTemplateSection section, int order)
    {
        section.DisplayOrder = order;
    }

    /// <summary>The header is part of every table: it can't be moved or removed on its own.</summary>
    private static void EnsureNotHeader(DbTemplateSection section, string action)
    {
        if (section.Role == SectionRole.Header)
        {
            throw new ConflictException($"The header is part of the table and can't be {action}d.");
        }
    }

    private async Task EnsureCanHoldSectionsAsync(int versionId, int parentId, CancellationToken cancellationToken)
    {
        var parentRole = await TemplateSectionQueries.FindRoleAsync(_db, versionId, parentId, cancellationToken);

        if (parentRole is null)
        {
            throw new InvalidRequestException($"Section {parentId} isn't part of this version.");
        }

        // An addable section can hold its own rows and sub-sections together; the header holds rows only.
        if (parentRole == SectionRole.Header)
        {
            throw new ConflictException("The header holds rows only. Sub-sections go in addable sections.");
        }
    }

    private async Task<DbTemplateSection> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var section = await TemplateSectionQueries.FindForEditAsync(_db, id, cancellationToken);
        if (section is null)
        {
            throw new NotFoundException($"Section {id} was not found.");
        }

        await _guard.EnsureEditableAsync(section.TableTemplateVersionId, cancellationToken);
        return section;
    }
}
