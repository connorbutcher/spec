using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public sealed class SheetDataService : ISheetDataService
{
    private const int LockTimeoutMilliseconds = 15000;

    private readonly PuSpecSheetDbContext _db;

    public SheetDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<DbSheet?> FindAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Sheets
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<DbSheet?> FindWithIdentifiersAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Sheets
            .AsNoTracking()
            .Include(sheet => sheet.IdentifierValues)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<DbSheet?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Sheets.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Sheets.AnyAsync(sheet => sheet.Id == id, cancellationToken);
    }

    public async Task<int?> FindIdAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await _db.Sheets
            .Where(sheet => sheet.PhaseId == phaseId && sheet.SheetTypeId == sheetTypeId && !sheet.IdentifierValues.Any())
            .OrderBy(sheet => sheet.Id)
            .Select(sheet => (int?)sheet.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<DbSheet>> ListWithIdentifiersAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await _db.Sheets
            .AsNoTracking()
            .Include(sheet => sheet.IdentifierValues)
            .Where(sheet => sheet.PhaseId == phaseId && sheet.SheetTypeId == sheetTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbSheetVersion>> ListVersionsAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _db.SheetVersions
            .AsNoTracking()
            .Include(version => version.PublishedBy)
            .Where(version => version.SheetId == sheetId)
            .OrderBy(version => version.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetLastVersionNumberAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _db.SheetVersions
            .Where(version => version.SheetId == sheetId)
            .MaxAsync(version => (int?)version.VersionNumber, cancellationToken) ?? 0;
    }

    public async Task<bool> AnyVersionWithNoteAsync(string note, CancellationToken cancellationToken)
    {
        return await _db.SheetVersions.AnyAsync(version => version.Note == note, cancellationToken);
    }

    public async Task<List<SheetSummary>> ListAsync(int phaseId, int? sheetId, CancellationToken cancellationToken)
    {
        var sheets = _db.Sheets.Where(sheet => sheet.PhaseId == phaseId && (sheetId == null || sheet.Id == sheetId));
        var rows = await sheets
            .AsNoTracking()
            .OrderBy(sheet => sheet.Id)
            .Select(sheet => new
            {
                sheet.Id,
                sheet.PublicId,
                sheet.SheetTypeId,
                sheet.CreatedAtUtc,
                Identifiers = sheet.IdentifierValues
                    .OrderBy(value => value.SheetTypeIdentifierId)
                    .Select(value => new SheetIdentifierValue { IdentifierId = value.SheetTypeIdentifierId, Value = value.Value })
                    .ToList(),
                Latest = sheet.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => new { version.VersionNumber, version.PublishedAtUtc })
                    .FirstOrDefault(),
                PendingSubmissionCount = _db.SheetSubmissions.Count(submission => submission.SheetId == sheet.Id && submission.Status == SubmissionStatus.Pending),
            })
            .ToListAsync(cancellationToken);

        var sheetIds = sheets.Select(sheet => sheet.Id);
        var drafts = new Dictionary<int, int>();
        Add(drafts, await CountDraftsAsync(_db.SheetTableRevisions, sheetIds, cancellationToken));
        Add(drafts, await CountDraftsAsync(_db.SheetSectionRevisions, sheetIds, cancellationToken));
        Add(drafts, await CountDraftsAsync(_db.SheetColumnBlockRevisions, sheetIds, cancellationToken));
        Add(drafts, await CountDraftsAsync(_db.SheetRowRevisions, sheetIds, cancellationToken));

        return rows
            .Select(row => new SheetSummary
            {
                Id = row.Id,
                PublicId = row.PublicId,
                PhaseId = phaseId,
                SheetTypeId = row.SheetTypeId,
                Identifiers = row.Identifiers,
                LatestVersionNumber = row.Latest?.VersionNumber,
                LatestPublishedAtUtc = row.Latest is null ? null : DateTime.SpecifyKind(row.Latest.PublishedAtUtc, DateTimeKind.Utc),
                CreatedAtUtc = DateTime.SpecifyKind(row.CreatedAtUtc, DateTimeKind.Utc),
                DraftCount = drafts.GetValueOrDefault(row.Id),
                PendingSubmissionCount = row.PendingSubmissionCount,
            })
            .ToList();
    }

    private static async Task<Dictionary<int, int>> CountDraftsAsync<TRevision>(
        IQueryable<TRevision> revisions,
        IQueryable<int> sheetIds,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        return await revisions
            .Where(revision => sheetIds.Contains(revision.SheetId))
            .Drafts()
            .GroupBy(revision => revision.SheetId)
            .Select(group => new { SheetId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(group => group.SheetId, group => group.Count, cancellationToken);
    }

    private static void Add(Dictionary<int, int> totals, Dictionary<int, int> counts)
    {
        foreach (var (sheetId, count) in counts)
        {
            totals[sheetId] = totals.GetValueOrDefault(sheetId) + count;
        }
    }

    // Held until the transaction ends, so two people creating the same sheet at once get one sheet and a conflict.
    public async Task LockIdentityAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var resource = $"SheetIdentity:{phaseId}:{sheetTypeId}";
        await _db.Database.ExecuteSqlAsync(
            $"""
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = {resource},
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = {LockTimeoutMilliseconds};
            IF @result < 0 THROW 50000, 'Someone else is creating a sheet of this type and it is taking too long. Try again.', 1;
            """,
            cancellationToken);
    }
}
