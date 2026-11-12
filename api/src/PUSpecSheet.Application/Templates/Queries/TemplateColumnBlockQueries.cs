using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Templates;

namespace PUSpecSheet.Application.Templates.Queries;

/// <summary>The column blocks of horizontal template versions.</summary>
public static class TemplateColumnBlockQueries
{
    /// <summary>A version's column blocks, read only.</summary>
    public static async Task<List<TemplateColumnBlock>> ListAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .AsNoTracking()
            .Where(block => block.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>A version's column blocks, tracked for editing.</summary>
    public static async Task<List<TemplateColumnBlock>> ListForEditAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .Where(candidate => candidate.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The ids of a version's column blocks, in display order.</summary>
    public static async Task<List<int>> ListIdsAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .Where(block => block.TableTemplateVersionId == versionId)
            .OrderBy(block => block.DisplayOrder)
            .Select(block => block.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The column block, tracked for editing; null when there is no such block.</summary>
    public static async Task<TemplateColumnBlock?> FindForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks.FindAsync([id], cancellationToken);
    }

    public static async Task<bool> AnyAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks.AnyAsync(
            block => block.TableTemplateVersionId == versionId,
            cancellationToken);
    }

    /// <summary>Whether the column block is part of the version.</summary>
    public static async Task<bool> IsInVersionAsync(PuSpecSheetDbContext db, int id, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks.AnyAsync(
            block => block.Id == id && block.TableTemplateVersionId == versionId,
            cancellationToken);
    }

    /// <summary>The highest display order among a version's column blocks, or null when it has none.</summary>
    public static async Task<int?> GetLastDisplayOrderAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .Where(block => block.TableTemplateVersionId == versionId)
            .MaxAsync(block => (int?)block.DisplayOrder, cancellationToken);
    }

    /// <summary>The column block, read only, when it is part of the version; null otherwise.</summary>
    public static async Task<TemplateColumnBlock?> FindInVersionAsync(PuSpecSheetDbContext db, int id, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == id && candidate.TableTemplateVersionId == versionId,
                cancellationToken);
    }

    /// <summary>Every column block of the versions, read only.</summary>
    public static async Task<List<TemplateColumnBlock>> ListInVersionsAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .AsNoTracking()
            .Where(block => versionIds.Contains(block.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }
}
