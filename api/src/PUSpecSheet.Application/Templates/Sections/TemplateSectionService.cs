using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Database.Entities.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates.Sections;

public sealed class TemplateSectionService : ITemplateSectionService
{
    private readonly ITemplateSectionRepository sections;
    private readonly IUnitOfWork unitOfWork;
    private readonly TableTemplateReader reader;
    private readonly TemplateVersionGuard guard;

    public TemplateSectionService(
        ITemplateSectionRepository sections,
        IUnitOfWork unitOfWork,
        TableTemplateReader reader,
        TemplateVersionGuard guard)
    {
        this.sections = sections;
        this.unitOfWork = unitOfWork;
        this.reader = reader;
        this.guard = guard;
    }

    public async Task<TableTemplateDto> CreateAsync(CreateTemplateSectionRequest request, CancellationToken cancellationToken)
    {
        var versionId = request.TableTemplateVersionId;
        await guard.EnsureEditableAsync(versionId, cancellationToken);

        if (request.ParentSectionId is int parentId)
        {
            await EnsureCanHoldSectionsAsync(versionId, parentId, cancellationToken);
        }

        var lastOrder = await sections.GetLastDisplayOrderAsync(versionId, request.ParentSectionId, cancellationToken);

        // The header is created with the table, so every section added here is an addable section.
        var section = new TemplateSection
        {
            TableTemplateVersionId = versionId,
            ParentSectionId = request.ParentSectionId,
            Name = request.Name.Trim(),
            DisplayOrder = (lastOrder ?? 0) + 1,
        };
        SectionInstanceRules.ApplyNewAddable(section);

        sections.Add(section);
        await unitOfWork.SaveChangesAsync(cancellationToken);
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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(section.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var section = await FindEditableAsync(id, cancellationToken);
        EnsureNotHeader(section, "move");

        var siblings = await sections.ListSiblingsForEditAsync(section.TableTemplateVersionId, section.ParentSectionId, cancellationToken);

        DisplayOrdering.Move(siblings, section, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(section.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var section = await FindEditableAsync(id, cancellationToken);
        EnsureNotHeader(section, "delete");

        // Child sections can't cascade in SQL Server (the version already cascades to every section), so
        // the whole subtree is removed here. Rows and cells cascade from their sections.
        var versionSections = await sections.ListForEditAsync(section.TableTemplateVersionId, cancellationToken);

        sections.RemoveRange(CollectSubtree(section, versionSections));

        var siblings = versionSections
            .Where(candidate => candidate.ParentSectionId == section.ParentSectionId && candidate.Id != section.Id)
            .ToList();

        DisplayOrdering.Renumber(siblings, sibling => sibling.DisplayOrder, SetOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
        var parentRole = await sections.FindRoleAsync(versionId, parentId, cancellationToken);

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

    private async Task<TemplateSection> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var section = await sections.FindForEditAsync(id, cancellationToken);
        if (section is null)
        {
            throw new NotFoundException($"Section {id} was not found.");
        }

        await guard.EnsureEditableAsync(section.TableTemplateVersionId, cancellationToken);
        return section;
    }
}
