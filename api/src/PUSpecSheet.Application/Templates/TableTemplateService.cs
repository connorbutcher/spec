using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

public sealed class TableTemplateService(PuSpecSheetDbContext db, TableTemplateReader reader) : ITableTemplateService
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

        var templates = await query
            .OrderBy(template => template.SheetTypeId)
            .ThenBy(template => template.DisplayOrder)
            .ThenBy(template => template.Name)
            .ToListAsync(cancellationToken);

        return templates.Select(template => template.ToSummaryDto()).ToList();
    }

    public Task<TableTemplateDto> GetAsync(int id, CancellationToken cancellationToken)
    {
        return reader.ReadAsync(id, cancellationToken);
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

        var template = new TableTemplate
        {
            SheetTypeId = request.SheetTypeId,
            Name = name,
            Orientation = request.Orientation,
            DisplayOrder = (lastOrder ?? 0) + 1,
        };

        db.TableTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadAsync(template.Id, cancellationToken);
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
        template.Orientation = request.Orientation;
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var template = await FindAsync(id, cancellationToken);
        await SheetDataGuard.EnsureTemplateUnusedAsync(db, id, cancellationToken);

        // Sections, rows and cells go with it through the database's cascading deletes.
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
