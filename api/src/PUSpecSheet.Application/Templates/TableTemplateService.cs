using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

public sealed class TableTemplateService(
    PuSpecSheetDbContext db,
    TableTemplateReader reader,
    TemplateVersionGuard guard) : ITableTemplateService
{
    public async Task<IReadOnlyList<TableTemplateSummaryDto>> GetSummariesAsync(
        int? sheetTypeId,
        CancellationToken cancellationToken)
    {
        var query = db.TableTemplates.AsNoTracking();
        if (sheetTypeId is not null)
        {
            query = query.Where(template => template.SheetTypeId == sheetTypeId);
        }

        return await query
            .OrderBy(template => template.SheetTypeId)
            .ThenBy(template => template.DisplayOrder)
            .ThenBy(template => template.Name)
            .Select(template => new
            {
                template,
                Latest = template.Versions.OrderByDescending(version => version.VersionNumber).First(),
            })
            .Select(entry => new TableTemplateSummaryDto(
                entry.template.Id,
                entry.template.SheetTypeId,
                entry.template.Name,
                entry.template.DisplayOrder,
                entry.Latest.VersionNumber,
                entry.Latest.Orientation))
            .ToListAsync(cancellationToken);
    }

    public Task<TableTemplateDto> GetAsync(int id, int? versionNumber, CancellationToken cancellationToken)
    {
        return reader.ReadAsync(id, versionNumber, cancellationToken);
    }

    public async Task<TableTemplateDto> CreateAsync(CreateTableTemplateRequest request, CancellationToken cancellationToken)
    {
        var sheetTypeExists = await db.SheetTypes.AnyAsync(
            sheetType => sheetType.Id == request.SheetTypeId,
            cancellationToken);

        if (!sheetTypeExists)
        {
            throw new InvalidRequestException($"Sheet type {request.SheetTypeId} doesn't exist.");
        }

        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(request.SheetTypeId, name, null, cancellationToken);

        var lastOrder = await db.TableTemplates
            .Where(template => template.SheetTypeId == request.SheetTypeId)
            .MaxAsync(template => (int?)template.DisplayOrder, cancellationToken);

        // Every table starts with its header, the one section it always has.
        var header = new TemplateSection { Name = "Header", DisplayOrder = 1 };
        SectionInstanceRules.ApplyHeader(header);

        var template = new TableTemplate
        {
            SheetTypeId = request.SheetTypeId,
            Name = name,
            DisplayOrder = (lastOrder ?? 0) + 1,
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

            var hasColumnBlocks = await db.TemplateColumnBlocks.AnyAsync(
                block => block.TableTemplateVersionId == latest.Id,
                cancellationToken);
            if (request.Orientation != TemplateOrientation.Horizontal && hasColumnBlocks)
            {
                throw new ConflictException("Only horizontal tables have column blocks. Remove the column blocks first.");
            }

            latest.Orientation = request.Orientation;
        }

        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(id, null, cancellationToken);
    }

    public async Task<TableTemplateDto> CreateVersionAsync(int id, CancellationToken cancellationToken)
    {
        await FindAsync(id, cancellationToken);
        var latest = await LatestVersionAsync(id, cancellationToken);

        await TemplateVersionCopier.CopyAsync(db, latest, latest.VersionNumber + 1, cancellationToken);
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

        var siblings = await db.TableTemplates
            .Where(sibling => sibling.SheetTypeId == template.SheetTypeId)
            .ToListAsync(cancellationToken);

        DisplayOrdering.Renumber(siblings, sibling => sibling.DisplayOrder, (sibling, order) => sibling.DisplayOrder = order);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<TableTemplate> FindAsync(int id, CancellationToken cancellationToken)
    {
        var template = await db.TableTemplates.FindAsync([id], cancellationToken);
        if (template is null)
        {
            throw new NotFoundException($"Table template {id} was not found.");
        }

        return template;
    }

    private Task<TableTemplateVersion> LatestVersionAsync(int templateId, CancellationToken cancellationToken)
    {
        return db.TableTemplateVersions
            .Where(version => version.TableTemplateId == templateId)
            .OrderByDescending(version => version.VersionNumber)
            .FirstAsync(cancellationToken);
    }

    private async Task EnsureNameIsFreeAsync(int sheetTypeId, string name, int? exceptId, CancellationToken cancellationToken)
    {
        var taken = await db.TableTemplates.AnyAsync(
            template => template.SheetTypeId == sheetTypeId && template.Name == name && template.Id != exceptId,
            cancellationToken);

        if (taken)
        {
            throw new ConflictException($"This sheet type already has a table called \"{name}\".");
        }
    }
}
