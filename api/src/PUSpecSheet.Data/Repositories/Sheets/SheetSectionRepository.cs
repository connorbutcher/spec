using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Repositories.Sheets;

public sealed class SheetSectionRepository : ISheetSectionRepository
{
    private readonly PuSpecSheetDbContext db;

    public SheetSectionRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<List<SheetSection>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Include(section => section.TemplateSection)
            .Where(section => section.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    public async Task<SheetSection?> FindWithTemplateAndTableAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Include(section => section.TemplateSection)
            .Include(section => section.SheetTable)
            .SingleOrDefaultAsync(section => section.Id == id, cancellationToken);
    }

    public async Task<SheetSection?> FindOnTableAsync(int id, int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id && candidate.SheetTableId == tableId, cancellationToken);
    }

    public async Task<List<SheetSectionPlace>> ListPlacesOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Where(section => section.SheetTableId == tableId)
            .Select(section => new SheetSectionPlace(section.Id, section.TemplateSectionId, section.ParentSheetSectionId))
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int?>> GetParentsOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Where(section => section.SheetTableId == tableId)
            .Select(section => new { section.Id, section.ParentSheetSectionId })
            .ToDictionaryAsync(section => section.Id, section => section.ParentSheetSectionId, cancellationToken);
    }

    public async Task<Dictionary<int, int?>> GetParentsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Where(candidate => ids.Contains(candidate.Id))
            .Select(candidate => new { candidate.Id, candidate.ParentSheetSectionId })
            .ToDictionaryAsync(entry => entry.Id, entry => entry.ParentSheetSectionId, cancellationToken);
    }

    public async Task<List<int>> ListSiblingIdsAsync(int tableId, int? parentSectionId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .Where(candidate => candidate.SheetTableId == tableId && candidate.ParentSheetSectionId == parentSectionId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        await db.SheetSections.Where(candidate => ids.Contains(candidate.Id)).ExecuteDeleteAsync(cancellationToken);
    }

    public void Add(SheetSection section)
    {
        db.SheetSections.Add(section);
    }
}
