using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Repositories.Templates;

public sealed class TemplateColumnBlockRepository(PuSpecSheetDbContext db) : ITemplateColumnBlockRepository
{
    public async Task<List<TemplateColumnBlock>> ListAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .AsNoTracking()
            .Where(block => block.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateColumnBlock>> ListForEditAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .Where(candidate => candidate.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .Where(block => block.TableTemplateVersionId == versionId)
            .OrderBy(block => block.DisplayOrder)
            .Select(block => block.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<TemplateColumnBlock?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks.FindAsync([id], cancellationToken);
    }

    public async Task<bool> AnyAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks.AnyAsync(
            block => block.TableTemplateVersionId == versionId,
            cancellationToken);
    }

    public async Task<bool> IsInVersionAsync(int id, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks.AnyAsync(
            block => block.Id == id && block.TableTemplateVersionId == versionId,
            cancellationToken);
    }

    public async Task<int?> GetLastDisplayOrderAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .Where(block => block.TableTemplateVersionId == versionId)
            .MaxAsync(block => (int?)block.DisplayOrder, cancellationToken);
    }

    public async Task<TemplateColumnBlock?> FindInVersionAsync(int id, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == id && candidate.TableTemplateVersionId == versionId,
                cancellationToken);
    }

    public async Task<List<TemplateColumnBlock>> ListInVersionsAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .AsNoTracking()
            .Where(block => versionIds.Contains(block.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }

    public void Add(TemplateColumnBlock block)
    {
        db.TemplateColumnBlocks.Add(block);
    }

    public void Remove(TemplateColumnBlock block)
    {
        db.TemplateColumnBlocks.Remove(block);
    }
}
