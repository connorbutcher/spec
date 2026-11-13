using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.SheetTypes.Queries;
using PUSpecSheet.Application.Templates.Queries;
using PUSpecSheet.Application.Templates.Sections;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Database;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

public sealed class TableTemplatesService : ITableTemplatesService
{
    private readonly PuSpecSheetDbContext db;
    private readonly TableTemplateReader reader;
    private readonly TemplateVersionGuard guard;

    public TableTemplatesService(
        PuSpecSheetDbContext db,
        TableTemplateReader reader,
        TemplateVersionGuard guard)
    {
        this.db = db;
        this.reader = reader;
        this.guard = guard;
    }

    public async Task<IReadOnlyList<TableTemplateSummaryDto>> GetSummariesAsync(
        int? sheetTypeId,
        CancellationToken cancellationToken)
    {
        return await TableTemplateQueries.ListSummariesAsync(db, sheetTypeId, cancellationToken);
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

        var lastOrder = await TableTemplateQueries.GetLastDisplayOrderAsync(db, request.SheetTypeId, cancellationToken);
        var template = NewTemplate(request, name, (lastOrder ?? 0) + 1);
        db.TableTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);

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

            var hasColumnBlocks = await TemplateColumnBlockQueries.AnyAsync(db, latest.Id, cancellationToken);
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

        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(id, null, cancellationToken);
    }

    public async Task<TableTemplateDto> CreateVersionAsync(int id, CancellationToken cancellationToken)
    {
        await FindAsync(id, cancellationToken);
        var latest = await LatestVersionAsync(id, cancellationToken);

        var copy = TemplateVersionCopier.Copy(
            latest,
            latest.VersionNumber + 1,
            await TemplateSectionQueries.ListWithRowsAsync(db, latest.Id, cancellationToken),
            await TemplateColumnBlockQueries.ListAsync(db, latest.Id, cancellationToken));
        db.TableTemplateVersions.Add(copy);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadAsync(id, null, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var template = await FindAsync(id, cancellationToken);
        await guard.EnsureTemplateUnusedAsync(id, cancellationToken);

        // Versions, sections, rows and cells go with it through the database's cascading deletes.
        db.TableTemplates.Remove(template);
        await db.SaveChangesAsync(cancellationToken);

        var siblings = await TableTemplateQueries.ListForEditAsync(db, template.SheetTypeId, cancellationToken);

        DisplayOrdering.Renumber(siblings, sibling => sibling.DisplayOrder, (sibling, order) => sibling.DisplayOrder = order);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<DbTableTemplate> FindAsync(int id, CancellationToken cancellationToken)
    {
        var template = await TableTemplateQueries.FindForEditAsync(db, id, cancellationToken);
        if (template is null)
        {
            throw new NotFoundException($"Table template {id} was not found.");
        }

        return template;
    }

    private Task<DbTableTemplateVersion> LatestVersionAsync(int templateId, CancellationToken cancellationToken)
    {
        return TableTemplateQueries.GetLatestVersionForEditAsync(db, templateId, cancellationToken);
    }

    private async Task EnsureNameIsFreeAsync(int sheetTypeId, string name, int? exceptId, CancellationToken cancellationToken)
    {
        var taken = await TableTemplateQueries.NameTakenAsync(db, sheetTypeId, name, exceptId, cancellationToken);

        if (taken)
        {
            throw new ConflictException($"This sheet type already has a table called \"{name}\".");
        }
    }

    /// <summary>A template whose first version holds only its header, the one section every table always has.</summary>
    private static DbTableTemplate NewTemplate(CreateTableTemplateRequest request, string name, int displayOrder)
    {
        var header = new DbTemplateSection { Name = "Header", DisplayOrder = 1 };
        SectionInstanceRules.ApplyHeader(header);
        return new DbTableTemplate
        {
            SheetTypeId = request.SheetTypeId,
            Name = name,
            DisplayOrder = displayOrder,
            Versions =
            [
                new DbTableTemplateVersion
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
        var sheetTypeExists = await SheetTypeQueries.ExistsAsync(db, sheetTypeId, cancellationToken);
        if (!sheetTypeExists)
        {
            throw new InvalidRequestException($"Sheet type {sheetTypeId} doesn't exist.");
        }
    }
}
