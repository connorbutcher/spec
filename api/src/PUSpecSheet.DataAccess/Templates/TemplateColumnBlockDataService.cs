using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public sealed class TemplateColumnBlockDataService : ITemplateColumnBlockDataService
{
    private readonly PuSpecSheetDbContext _db;

    public TemplateColumnBlockDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<DbTemplateColumnBlock>> ListAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateColumnBlocks
            .AsNoTracking()
            .Where(block => block.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbTemplateColumnBlock>> ListForEditAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateColumnBlocks
            .Where(candidate => candidate.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateColumnBlocks
            .Where(block => block.TableTemplateVersionId == versionId)
            .OrderBy(block => block.DisplayOrder)
            .Select(block => block.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<DbTemplateColumnBlock?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.TemplateColumnBlocks.FindAsync([id], cancellationToken);
    }

    public async Task<bool> AnyAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateColumnBlocks.AnyAsync(
            block => block.TableTemplateVersionId == versionId,
            cancellationToken);
    }

    public async Task<bool> IsInVersionAsync(int id, int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateColumnBlocks.AnyAsync(
            block => block.Id == id && block.TableTemplateVersionId == versionId,
            cancellationToken);
    }

    public async Task<int?> GetLastDisplayOrderAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateColumnBlocks
            .Where(block => block.TableTemplateVersionId == versionId)
            .MaxAsync(block => (int?)block.DisplayOrder, cancellationToken);
    }

    public async Task<DbTemplateColumnBlock?> FindInVersionAsync(int id, int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateColumnBlocks
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == id && candidate.TableTemplateVersionId == versionId,
                cancellationToken);
    }

    public async Task<List<DbTemplateColumnBlock>> ListInVersionsAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await _db.TemplateColumnBlocks
            .AsNoTracking()
            .Where(block => versionIds.Contains(block.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }
}
