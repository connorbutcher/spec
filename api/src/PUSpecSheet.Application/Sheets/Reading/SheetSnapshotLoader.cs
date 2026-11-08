using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.History;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Reading;

/// <summary>
/// Loads a sheet for one view point. A live view reads each item's current published revision plus any
/// drafts; a past view reads only the revisions whose published range covers that moment, which is what
/// the (item, published, superseded) indexes are for.
/// </summary>
public sealed class SheetSnapshotLoader(PuSpecSheetDbContext db, RowValueStore valueStore, SheetChangeHistoryLoader changeLoader)
{
    public async Task<SheetSnapshot> LoadAsync(int sheetId, SheetViewPoint view, int currentUserId, CancellationToken cancellationToken)
    {
        var sheet = await LoadSheetAsync(sheetId, cancellationToken);
        var versions = await LoadVersionsAsync(sheetId, cancellationToken);
        var (moment, versionNumber) = ResolveMoment(view, versions);
        var tables = await LoadTablesAsync(sheetId, cancellationToken);
        var sections = await LoadSectionsAsync(sheetId, cancellationToken);
        var rows = await LoadRowsAsync(sheetId, cancellationToken);
        var columnBlocks = await LoadColumnBlocksAsync(sheetId, cancellationToken);

        // Revisions are found by the sheet they carry, which the (sheet, status) and (sheet, published)
        // indexes lead with, so the cost follows the size of this sheet rather than the whole database.
        var tableRevisions = await ResolveRevisionsAsync(db.TableRevisionsOf(sheetId), revision => revision.SheetTableId, moment, currentUserId, cancellationToken);
        var sectionRevisions = await ResolveRevisionsAsync(db.SectionRevisionsOf(sheetId), revision => revision.SheetSectionId, moment, currentUserId, cancellationToken);
        var rowRevisions = await ResolveRevisionsAsync(db.RowRevisionsOf(sheetId), revision => revision.SheetRowId, moment, currentUserId, cancellationToken);
        var columnBlockRevisions = await ResolveRevisionsAsync(db.ColumnBlockRevisionsOf(sheetId), revision => revision.SheetColumnBlockId, moment, currentUserId, cancellationToken);
        var values = await LoadShownValuesAsync(db.RowRevisionsOf(sheetId), rowRevisions, moment, cancellationToken);

        var versionIds = tables.Select(table => table.TableTemplateVersionId).Distinct().ToList();
        var templateRows = await LoadTemplateRowsAsync(versionIds, cancellationToken);
        await AttachTemplateCellsAsync(rows, templateRows, cancellationToken);
        var availableTemplates = await LoadAvailableTemplatesAsync(sheet.SheetTypeId, cancellationToken);

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
            TemplateSections = await LoadTemplateSectionsAsync(versionIds, cancellationToken),
            TemplateColumnBlocks = await LoadTemplateColumnBlocksAsync(versionIds, cancellationToken),
            TemplateRows = templateRows,
            CellTypes = await LoadCellTypesAsync(cancellationToken),
            AvailableTemplates = availableTemplates,
            LatestTemplateVersions = await LoadLatestTemplateVersionsAsync(availableTemplates, cancellationToken),
            UserNames = await LoadUserNamesAsync(cancellationToken),
        };
        return snapshot with { Changes = await changeLoader.LoadAsync(snapshot, moment, cancellationToken) };
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

    private static async Task<Dictionary<int, RevisionResolution<TRevision>>> ResolveRevisionsAsync<TRevision>(
        IQueryable<TRevision> query,
        Func<TRevision, int> itemId,
        DateTime? moment,
        int currentUserId,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        var revisions = await LoadRevisionsAsync(query, moment, cancellationToken);
        return RevisionResolver.Resolve(revisions, itemId, currentUserId);
    }

    private static async Task<List<TRevision>> LoadRevisionsAsync<TRevision>(
        IQueryable<TRevision> query,
        DateTime? moment,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        return await InView(query, moment).AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// The revisions a view can show. The live view needs every revision that hasn't been replaced, which is
    /// the current published one and any drafts. A past view needs the one published revision whose range
    /// covers the moment.
    /// </summary>
    private static IQueryable<TRevision> InView<TRevision>(IQueryable<TRevision> query, DateTime? moment)
        where TRevision : class, ISheetRevision
    {
        return moment is { } at
            ? query.Where(revision => revision.Status == RevisionStatus.Published
                && revision.PublishedAtUtc <= at
                && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > at))
            : query.CurrentAndDrafts();
    }

    private async Task<Sheet> LoadSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");
    }

    private async Task<List<SheetVersion>> LoadVersionsAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetVersions
            .AsNoTracking()
            .Include(version => version.PublishedBy)
            .Where(version => version.SheetId == sheetId)
            .OrderBy(version => version.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<SheetTable>> LoadTablesAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .AsNoTracking()
            .Include(table => table.TableTemplateVersion)
            .ThenInclude(version => version.TableTemplate)
            .Where(table => table.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<SheetSection>> LoadSectionsAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Include(section => section.TemplateSection)
            .Where(section => section.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// The sheet's rows and their cells. Each cell's template cell is attached afterwards (see
    /// <see cref="AttachTemplateCellsAsync"/>) rather than joined here, where it would be read and built
    /// again for every cell made from it.
    /// </summary>
    private async Task<List<SheetRow>> LoadRowsAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => row.SheetSection.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gives every cell its template cell from the template rows already read, and reads only those that
    /// aren't among them in one more query.
    /// </summary>
    private async Task AttachTemplateCellsAsync(List<SheetRow> rows, List<TemplateRow> templateRows, CancellationToken cancellationToken)
    {
        var templateCells = templateRows.SelectMany(row => row.Cells).ToDictionary(cell => cell.Id);
        var cells = rows.SelectMany(row => row.Cells).ToList();
        var missingIds = cells
            .Select(cell => cell.TemplateCellId)
            .Where(id => !templateCells.ContainsKey(id))
            .Distinct()
            .ToList();
        if (missingIds.Count > 0)
        {
            foreach (var templateCell in await db.TemplateCells.AsNoTracking().Where(cell => missingIds.Contains(cell.Id)).ToListAsync(cancellationToken))
            {
                templateCells[templateCell.Id] = templateCell;
            }
        }

        foreach (var cell in cells)
        {
            cell.TemplateCell = templateCells[cell.TemplateCellId];
        }
    }

    private async Task<List<SheetColumnBlock>> LoadColumnBlocksAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlocks
            .AsNoTracking()
            .Include(block => block.TemplateColumnBlock)
            .Where(block => block.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// The values of the row revisions shown. They are read for every revision the view could show, found
    /// by the sheet in the database rather than sent as a list of ids (see <see cref="RowRevisionScope"/>),
    /// and other people's drafts are then left out.
    /// </summary>
    private async Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadShownValuesAsync(
        IQueryable<SheetRowRevision> sheetRowRevisions,
        Dictionary<int, RevisionResolution<SheetRowRevision>> rowRevisions,
        DateTime? moment,
        CancellationToken cancellationToken)
    {
        var shownRowRevisionIds = rowRevisions.Values
            .Where(resolution => resolution.Shown is not null)
            .Select(resolution => resolution.Shown!.Id)
            .ToHashSet();
        if (shownRowRevisionIds.Count == 0)
        {
            return [];
        }

        var scope = RowRevisionScope.Of(InView(sheetRowRevisions, moment).Select(revision => revision.Id));
        var values = await valueStore.LoadAsync(scope, cancellationToken);
        foreach (var revisionId in values.Keys.Where(revisionId => !shownRowRevisionIds.Contains(revisionId)).ToList())
        {
            values.Remove(revisionId);
        }

        return values;
    }

    private async Task<List<TemplateSection>> LoadTemplateSectionsAsync(List<int> versionIds, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(section => versionIds.Contains(section.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<TemplateColumnBlock>> LoadTemplateColumnBlocksAsync(List<int> versionIds, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .AsNoTracking()
            .Where(block => versionIds.Contains(block.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<TemplateRow>> LoadTemplateRowsAsync(List<int> versionIds, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => versionIds.Contains(row.TemplateSection.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<int, CellType>> LoadCellTypesAsync(CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .AsNoTracking()
            .Include(cellType => cellType.Options)
            .ToDictionaryAsync(cellType => cellType.Id, cancellationToken);
    }

    private async Task<List<TableTemplate>> LoadAvailableTemplatesAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .AsNoTracking()
            .Where(template => template.SheetTypeId == sheetTypeId)
            .OrderBy(template => template.DisplayOrder)
            .ThenBy(template => template.Name)
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<int, int>> LoadLatestTemplateVersionsAsync(List<TableTemplate> availableTemplates, CancellationToken cancellationToken)
    {
        var availableIds = availableTemplates.Select(template => template.Id).ToList();
        return await db.TableTemplateVersions
            .AsNoTracking()
            .Where(version => availableIds.Contains(version.TableTemplateId))
            .GroupBy(version => version.TableTemplateId)
            .Select(group => new { TemplateId = group.Key, Latest = group.Max(version => version.VersionNumber) })
            .ToDictionaryAsync(entry => entry.TemplateId, entry => entry.Latest, cancellationToken);
    }

    private async Task<Dictionary<int, string>> LoadUserNamesAsync(CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }
}
