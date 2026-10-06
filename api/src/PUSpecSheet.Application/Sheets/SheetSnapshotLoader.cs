using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Loads a sheet for one view point. A live view reads each item's current published revision plus any
/// drafts; a past view reads only the revisions whose published range covers that moment, which is what
/// the (item, published, superseded) indexes are for.
/// </summary>
public sealed class SheetSnapshotLoader(PuSpecSheetDbContext db, RowValueStore valueStore, SheetChangeHistoryLoader changeLoader)
{
    public async Task<SheetSnapshot> LoadAsync(int sheetId, SheetViewPoint view, int currentUserId, CancellationToken cancellationToken)
    {
        var sheet = await db.Sheets
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");

        var versions = await db.SheetVersions
            .AsNoTracking()
            .Include(version => version.PublishedBy)
            .Where(version => version.SheetId == sheetId)
            .OrderBy(version => version.VersionNumber)
            .ToListAsync(cancellationToken);

        var (moment, versionNumber) = ResolveMoment(view, versions);

        var tables = await db.SheetTables
            .AsNoTracking()
            .Include(table => table.TableTemplateVersion)
            .ThenInclude(version => version.TableTemplate)
            .Where(table => table.SheetId == sheetId)
            .ToListAsync(cancellationToken);

        var sections = await db.SheetSections
            .AsNoTracking()
            .Include(section => section.TemplateSection)
            .Where(section => section.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);

        var rows = await db.SheetRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .ThenInclude(cell => cell.TemplateCell)
            .Where(row => row.SheetSection.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);

        var columnBlocks = await db.SheetColumnBlocks
            .AsNoTracking()
            .Include(block => block.TemplateColumnBlock)
            .Where(block => block.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);

        // Revisions are found through their items' ids, which the revision indexes lead with, so the cost
        // follows the size of this sheet rather than the number of revisions in the whole database.
        var tableIds = tables.Select(table => table.Id).ToList();
        var sectionIds = sections.Select(section => section.Id).ToList();
        var rowIds = rows.Select(row => row.Id).ToList();
        var columnBlockIds = columnBlocks.Select(block => block.Id).ToList();

        var tableRevisions = RevisionResolver.Resolve(
            await LoadRevisionsAsync(
                db.SheetTableRevisions.Where(revision => tableIds.Contains(revision.SheetTableId)),
                moment,
                cancellationToken),
            revision => revision.SheetTableId,
            currentUserId);

        var sectionRevisions = RevisionResolver.Resolve(
            await LoadRevisionsAsync(
                db.SheetSectionRevisions.Where(revision => sectionIds.Contains(revision.SheetSectionId)),
                moment,
                cancellationToken),
            revision => revision.SheetSectionId,
            currentUserId);

        var rowRevisions = RevisionResolver.Resolve(
            await LoadRevisionsAsync(
                db.SheetRowRevisions.Where(revision => rowIds.Contains(revision.SheetRowId)),
                moment,
                cancellationToken),
            revision => revision.SheetRowId,
            currentUserId);

        var columnBlockRevisions = RevisionResolver.Resolve(
            await LoadRevisionsAsync(
                db.SheetColumnBlockRevisions.Where(revision => columnBlockIds.Contains(revision.SheetColumnBlockId)),
                moment,
                cancellationToken),
            revision => revision.SheetColumnBlockId,
            currentUserId);

        var shownRowRevisionIds = rowRevisions.Values
            .Where(resolution => resolution.Shown is not null)
            .Select(resolution => resolution.Shown!.Id)
            .ToList();
        var values = await valueStore.LoadAsync(shownRowRevisionIds, cancellationToken);

        var versionIds = tables.Select(table => table.TableTemplateVersionId).Distinct().ToList();
        var templateSections = await db.TemplateSections
            .AsNoTracking()
            .Where(section => versionIds.Contains(section.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
        var templateColumnBlocks = await db.TemplateColumnBlocks
            .AsNoTracking()
            .Where(block => versionIds.Contains(block.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
        var templateRows = await db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => versionIds.Contains(row.TemplateSection.TableTemplateVersionId))
            .ToListAsync(cancellationToken);

        var availableTemplates = await db.TableTemplates
            .AsNoTracking()
            .Where(template => template.SheetTypeId == sheet.SheetTypeId)
            .OrderBy(template => template.DisplayOrder)
            .ThenBy(template => template.Name)
            .ToListAsync(cancellationToken);
        var availableIds = availableTemplates.Select(template => template.Id).ToList();
        var latestVersions = await db.TableTemplateVersions
            .AsNoTracking()
            .Where(version => availableIds.Contains(version.TableTemplateId))
            .GroupBy(version => version.TableTemplateId)
            .Select(group => new { TemplateId = group.Key, Latest = group.Max(version => version.VersionNumber) })
            .ToDictionaryAsync(entry => entry.TemplateId, entry => entry.Latest, cancellationToken);

        var userNames = await db.Users
            .AsNoTracking()
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);

        var snapshot = new SheetSnapshot
        {
            Sheet = sheet,
            AsOfUtc = moment,
            ViewedVersionNumber = versionNumber,
            Versions = versions,
            Tables = tables,
            Sections = sections,
            Rows = rows,
            ColumnBlocks = columnBlocks,
            TableRevisions = tableRevisions,
            SectionRevisions = sectionRevisions,
            RowRevisions = rowRevisions,
            ColumnBlockRevisions = columnBlockRevisions,
            Values = values,
            UserNames = userNames,
            TemplateSections = templateSections,
            TemplateColumnBlocks = templateColumnBlocks,
            TemplateRows = templateRows,
            AvailableTemplates = availableTemplates,
            LatestTemplateVersions = latestVersions,
        };
        snapshot.Changes = await changeLoader.LoadAsync(snapshot, moment, cancellationToken);
        return snapshot;
    }

    private static (DateTime? Moment, int? VersionNumber) ResolveMoment(SheetViewPoint view, IReadOnlyList<SheetVersion> versions)
    {
        if (view.VersionNumber is { } number)
        {
            var version = versions.SingleOrDefault(candidate => candidate.VersionNumber == number)
                ?? throw new NotFoundException($"Version {number} of this sheet was not found.");
            return (version.PublishedAtUtc, number);
        }

        return (view.AsOfUtc, null);
    }

    private static async Task<List<TRevision>> LoadRevisionsAsync<TRevision>(
        IQueryable<TRevision> query,
        DateTime? moment,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        // The live view needs every revision that hasn't been replaced, which is the current published
        // one and any drafts. A past view needs the one published revision whose range covers the moment.
        var filtered = moment is { } at
            ? query.Where(revision => revision.Status == RevisionStatus.Published
                && revision.PublishedAtUtc <= at
                && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > at))
            : query.Where(revision => revision.SupersededAtUtc == null);

        return await filtered.AsNoTracking().ToListAsync(cancellationToken);
    }
}
