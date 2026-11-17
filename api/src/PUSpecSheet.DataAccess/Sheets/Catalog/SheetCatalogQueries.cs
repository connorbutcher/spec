using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Catalog;

/// <summary>A phase's sheets as a list: what tells each apart and where its versions and drafts stand.</summary>
public static class SheetCatalogQueries
{
    /// <summary>The phase's sheets, or only one of them, oldest first.</summary>
    public static async Task<List<SheetSummaryDto>> ListAsync(PuSpecSheetDbContext db, int phaseId, int? sheetId, CancellationToken cancellationToken)
    {
        var sheets = db.Sheets.Where(sheet => sheet.PhaseId == phaseId && (sheetId == null || sheet.Id == sheetId));
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
                    .Select(value => new SheetIdentifierValueDto { IdentifierId = value.SheetTypeIdentifierId, Value = value.Value })
                    .ToList(),
                Latest = sheet.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => new { version.VersionNumber, version.PublishedAtUtc })
                    .FirstOrDefault(),
                PendingSubmissionCount = db.SheetSubmissions.Count(submission => submission.SheetId == sheet.Id && submission.Status == SubmissionStatus.Pending),
            })
            .ToListAsync(cancellationToken);

        var sheetIds = sheets.Select(sheet => sheet.Id);
        var drafts = new Dictionary<int, int>();
        Add(drafts, await CountDraftsAsync(db.SheetTableRevisions, sheetIds, cancellationToken));
        Add(drafts, await CountDraftsAsync(db.SheetSectionRevisions, sheetIds, cancellationToken));
        Add(drafts, await CountDraftsAsync(db.SheetColumnBlockRevisions, sheetIds, cancellationToken));
        Add(drafts, await CountDraftsAsync(db.SheetRowRevisions, sheetIds, cancellationToken));

        return rows
            .Select(row => new SheetSummaryDto
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
}
