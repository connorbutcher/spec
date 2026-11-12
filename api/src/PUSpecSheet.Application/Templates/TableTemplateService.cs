using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.SheetTypes;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Application.Templates.Sections;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Database.Entities.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

public sealed class TableTemplateService : ITableTemplateService
{
    private readonly ITableTemplateRepository templates;
    private readonly ITemplateSectionRepository sections;
    private readonly ITemplateColumnBlockRepository columnBlocks;
    private readonly ISheetTypeRepository sheetTypes;
    private readonly IUnitOfWork unitOfWork;
    private readonly TableTemplateReader reader;
    private readonly TemplateVersionGuard guard;

    public TableTemplateService(
        ITableTemplateRepository templates,
        ITemplateSectionRepository sections,
        ITemplateColumnBlockRepository columnBlocks,
        ISheetTypeRepository sheetTypes,
        IUnitOfWork unitOfWork,
        TableTemplateReader reader,
        TemplateVersionGuard guard)
    {
        this.templates = templates;
        this.sections = sections;
        this.columnBlocks = columnBlocks;
        this.sheetTypes = sheetTypes;
        this.unitOfWork = unitOfWork;
        this.reader = reader;
        this.guard = guard;
    }

    public async Task<IReadOnlyList<TableTemplateSummaryDto>> GetSummariesAsync(
        int? sheetTypeId,
        CancellationToken cancellationToken)
    {
        return await templates.ListSummariesAsync(sheetTypeId, cancellationToken);
    }

    public Task<TableTemplateDto> GetAsync(int id, int? versionNumber, CancellationToken cancellationToken)
    {
        return reader.ReadAsync(id, versionNumber, cancellationToken);
    }

    public async Task<TableTemplateDto> CreateAsync(CreateTableTemplateRequest request, CancellationToken cancellationToken)
    {
        await EnsureSheetTypeExistsAsync(request.SheetTypeId, cancellationToken);
        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(request.SheetTypeId, name, null, cancellationToken);

        var lastOrder = await templates.GetLastDisplayOrderAsync(request.SheetTypeId, cancellationToken);
        var template = NewTemplate(request, name, (lastOrder ?? 0) + 1);
        templates.Add(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await reader.ReadAsync(template.Id, null, cancellationToken);
    }

    public async Task<TableTemplateDto> UpdateAsync(
        int id,
        UpdateTableTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var template = await FindAsync(id, cancellationToken);

        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(template.SheetTypeId, name, id, cancellationToken);
        template.Name = name;

        // The name belongs to the table; orientation is part of the layout, so it's versioned.
        var latest = await LatestVersionAsync(id, cancellationToken);
        if (latest.Orientation != request.Orientation)
        {
            await guard.EnsureEditableAsync(latest.Id, cancellationToken);

            var hasColumnBlocks = await columnBlocks.AnyAsync(latest.Id, cancellationToken);
            if (request.Orientation != TemplateOrientation.Horizontal && hasColumnBlocks)
            {
                throw new ConflictException("Only horizontal tables have column blocks. Remove the column blocks first.");
            }

            latest.Orientation = request.Orientation;
        }

        if (latest.StickyColumnCount != request.StickyColumnCount)
        {
            await guard.EnsureEditableAsync(latest.Id, cancellationToken);
            latest.StickyColumnCount = request.StickyColumnCount;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(id, null, cancellationToken);
    }

    public async Task<TableTemplateDto> CreateVersionAsync(int id, CancellationToken cancellationToken)
    {
        await FindAsync(id, cancellationToken);
        var latest = await LatestVersionAsync(id, cancellationToken);

        var copy = TemplateVersionCopier.Copy(
            latest,
            latest.VersionNumber + 1,
            await sections.ListWithRowsAsync(latest.Id, cancellationToken),
            await columnBlocks.ListAsync(latest.Id, cancellationToken));
        templates.AddVersion(copy);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await reader.ReadAsync(id, null, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var template = await FindAsync(id, cancellationToken);
        await guard.EnsureTemplateUnusedAsync(id, cancellationToken);

        // Versions, sections, rows and cells go with it through the database's cascading deletes.
        templates.Remove(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var siblings = await templates.ListForEditAsync(template.SheetTypeId, cancellationToken);

        DisplayOrdering.Renumber(siblings, sibling => sibling.DisplayOrder, (sibling, order) => sibling.DisplayOrder = order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<TableTemplate> FindAsync(int id, CancellationToken cancellationToken)
    {
        var template = await templates.FindForEditAsync(id, cancellationToken);
        if (template is null)
        {
            throw new NotFoundException($"Table template {id} was not found.");
        }

        return template;
    }

    private Task<TableTemplateVersion> LatestVersionAsync(int templateId, CancellationToken cancellationToken)
    {
        return templates.GetLatestVersionForEditAsync(templateId, cancellationToken);
    }

    private async Task EnsureNameIsFreeAsync(int sheetTypeId, string name, int? exceptId, CancellationToken cancellationToken)
    {
        var taken = await templates.NameTakenAsync(sheetTypeId, name, exceptId, cancellationToken);

        if (taken)
        {
            throw new ConflictException($"This sheet type already has a table called \"{name}\".");
        }
    }

    /// <summary>A template whose first version holds only its header, the one section every table always has.</summary>
    private static TableTemplate NewTemplate(CreateTableTemplateRequest request, string name, int displayOrder)
    {
        var header = new TemplateSection { Name = "Header", DisplayOrder = 1 };
        SectionInstanceRules.ApplyHeader(header);
        return new TableTemplate
        {
            SheetTypeId = request.SheetTypeId,
            Name = name,
            DisplayOrder = displayOrder,
            Versions =
            [
                new TableTemplateVersion
                {
                    VersionNumber = 1,
                    Orientation = request.Orientation,
                    Sections = [header],
                },
            ],
        };
    }

    private async Task EnsureSheetTypeExistsAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        var sheetTypeExists = await sheetTypes.ExistsAsync(sheetTypeId, cancellationToken);
        if (!sheetTypeExists)
        {
            throw new InvalidRequestException($"Sheet type {sheetTypeId} doesn't exist.");
        }
    }
}
