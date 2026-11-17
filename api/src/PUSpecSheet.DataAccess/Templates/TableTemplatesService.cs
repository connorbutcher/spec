using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.SheetTypes;
using PUSpecSheet.DataAccess.Templates.Sections;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public sealed class TableTemplatesService : ITableTemplatesService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly TableTemplateReader _reader;
    private readonly TemplateVersionGuard _guard;
    private readonly ISheetTypeDataService _sheetTypes;
    private readonly ITableTemplateDataService _tableTemplateData;
    private readonly ITemplateSectionDataService _templateSectionData;
    private readonly ITemplateColumnBlockDataService _templateColumnBlockData;

    public TableTemplatesService(
        PuSpecSheetDbContext db,
        TableTemplateReader reader,
        TemplateVersionGuard guard,
        ISheetTypeDataService sheetTypes,
        ITableTemplateDataService tableTemplateData,
        ITemplateSectionDataService templateSectionData,
        ITemplateColumnBlockDataService templateColumnBlockData)
    {
        _db = db;
        _reader = reader;
        _guard = guard;
        _sheetTypes = sheetTypes;
        _tableTemplateData = tableTemplateData;
        _templateSectionData = templateSectionData;
        _templateColumnBlockData = templateColumnBlockData;
    }

    public async Task<IReadOnlyList<TableTemplateSummary>> GetSummariesAsync(
        int? sheetTypeId,
        CancellationToken cancellationToken)
    {
        return await _tableTemplateData.ListSummariesAsync(sheetTypeId, cancellationToken);
    }

    public Task<TableTemplate> GetAsync(int id, int? versionNumber, CancellationToken cancellationToken)
    {
        return _reader.ReadAsync(id, versionNumber, cancellationToken);
    }

    public async Task<TableTemplate> CreateAsync(CreateTableTemplateRequest request, CancellationToken cancellationToken)
    {
        await EnsureSheetTypeExistsAsync(request.SheetTypeId, cancellationToken);
        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(request.SheetTypeId, name, null, cancellationToken);

        var lastOrder = await _tableTemplateData.GetLastDisplayOrderAsync(request.SheetTypeId, cancellationToken);
        var template = NewTemplate(request, name, (lastOrder ?? 0) + 1);
        _db.TableTemplates.Add(template);
        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadAsync(template.Id, null, cancellationToken);
    }

    public async Task<TableTemplate> UpdateAsync(
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
            await _guard.EnsureEditableAsync(latest.Id, cancellationToken);

            var hasColumnBlocks = await _templateColumnBlockData.AnyAsync(latest.Id, cancellationToken);
            if (request.Orientation != TemplateOrientation.Horizontal && hasColumnBlocks)
            {
                throw new ConflictException("Only horizontal tables have column blocks. Remove the column blocks first.");
            }

            latest.Orientation = request.Orientation;
        }

        if (latest.StickyColumnCount != request.StickyColumnCount)
        {
            await _guard.EnsureEditableAsync(latest.Id, cancellationToken);
            latest.StickyColumnCount = request.StickyColumnCount;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await _reader.ReadAsync(id, null, cancellationToken);
    }

    public async Task<TableTemplate> CreateVersionAsync(int id, CancellationToken cancellationToken)
    {
        await FindAsync(id, cancellationToken);
        var latest = await LatestVersionAsync(id, cancellationToken);

        var copy = TemplateVersionCopier.Copy(
            latest,
            latest.VersionNumber + 1,
            await _templateSectionData.ListWithRowsAsync(latest.Id, cancellationToken),
            await _templateColumnBlockData.ListAsync(latest.Id, cancellationToken));
        _db.TableTemplateVersions.Add(copy);
        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadAsync(id, null, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var template = await FindAsync(id, cancellationToken);
        await _guard.EnsureTemplateUnusedAsync(id, cancellationToken);

        // Versions, sections, rows and cells go with it through the database's cascading deletes.
        _db.TableTemplates.Remove(template);
        await _db.SaveChangesAsync(cancellationToken);

        var siblings = await _tableTemplateData.ListForEditAsync(template.SheetTypeId, cancellationToken);

        DisplayOrdering.Renumber(siblings, sibling => sibling.DisplayOrder, (sibling, order) => sibling.DisplayOrder = order);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<DbTableTemplate> FindAsync(int id, CancellationToken cancellationToken)
    {
        var template = await _tableTemplateData.FindForEditAsync(id, cancellationToken);
        if (template is null)
        {
            throw new NotFoundException($"Table template {id} was not found.");
        }

        return template;
    }

    private Task<DbTableTemplateVersion> LatestVersionAsync(int templateId, CancellationToken cancellationToken)
    {
        return _tableTemplateData.GetLatestVersionForEditAsync(templateId, cancellationToken);
    }

    private async Task EnsureNameIsFreeAsync(int sheetTypeId, string name, int? exceptId, CancellationToken cancellationToken)
    {
        var taken = await _tableTemplateData.NameTakenAsync(sheetTypeId, name, exceptId, cancellationToken);

        if (taken)
        {
            throw new ConflictException($"This sheet type already has a table called \"{name}\".");
        }
    }

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
        var sheetTypeExists = await _sheetTypes.ExistsAsync(sheetTypeId, cancellationToken);
        if (!sheetTypeExists)
        {
            throw new InvalidRequestException($"Sheet type {sheetTypeId} doesn't exist.");
        }
    }
}
