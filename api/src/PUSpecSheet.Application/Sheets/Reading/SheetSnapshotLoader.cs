using PUSpecSheet.Application.CellTypes.Queries;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.History;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Application.Templates.Queries;
using PUSpecSheet.Application.Users.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Sheets;
using PUSpecSheet.Database.Entities.Templates;

namespace PUSpecSheet.Application.Sheets.Reading;

/// <summary>
/// Loads a sheet for one view point. A live view reads each item's current published revision plus any
/// drafts; a past view reads only the revisions whose published range covers that moment, which is what
/// the (item, published, superseded) indexes are for.
/// </summary>
public sealed class SheetSnapshotLoader
{
    private readonly PuSpecSheetDbContext db;
    private readonly SheetChangeHistoryLoader changeLoader;

    public SheetSnapshotLoader(
        PuSpecSheetDbContext db,
        SheetChangeHistoryLoader changeLoader)
    {
        this.db = db;
        this.changeLoader = changeLoader;
    }

    public async Task<SheetSnapshot> LoadAsync(int sheetId, SheetViewPoint view, int currentUserId, CancellationToken cancellationToken)
    {
        var found = await SheetQueries.FindAsync(db, sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");
        var versions = await SheetQueries.ListVersionsAsync(db, sheetId, cancellationToken);
        var (moment, versionNumber) = ResolveMoment(view, versions);
        var tables = await SheetTableQueries.ListOnSheetAsync(db, sheetId, cancellationToken);
        var sections = await SheetSectionQueries.ListOnSheetAsync(db, sheetId, cancellationToken);
        var rows = await SheetRowQueries.ListOnSheetAsync(db, sheetId, cancellationToken);
        var columnBlocks = await SheetColumnBlockQueries.ListOnSheetAsync(db, sheetId, cancellationToken);

        var tableRevisions = Resolve(await db.SheetTableRevisions.ListInViewAsync(sheetId, moment, cancellationToken), revision => revision.SheetTableId, view, currentUserId);
        var sectionRevisions = Resolve(await db.SheetSectionRevisions.ListInViewAsync(sheetId, moment, cancellationToken), revision => revision.SheetSectionId, view, currentUserId);
        var rowRevisions = Resolve(await db.SheetRowRevisions.ListInViewAsync(sheetId, moment, cancellationToken), revision => revision.SheetRowId, view, currentUserId);
        var columnBlockRevisions = Resolve(await db.SheetColumnBlockRevisions.ListInViewAsync(sheetId, moment, cancellationToken), revision => revision.SheetColumnBlockId, view, currentUserId);
        var shownValues = await LoadShownValuesAsync(sheetId, rowRevisions, moment, cancellationToken);

        var versionIds = tables.Select(table => table.TableTemplateVersionId).Distinct().ToList();
        var templateRows = await TemplateRowQueries.ListWithCellsInVersionsAsync(db, versionIds, cancellationToken);
        await AttachTemplateCellsAsync(rows, templateRows, cancellationToken);
        var availableTemplates = await TableTemplateQueries.ListForSheetTypeAsync(db, found.SheetTypeId, cancellationToken);

        var snapshot = new SheetSnapshot
        {
            Sheet = found,
            AsOfUtc = moment,
            ViewedVersionNumber = versionNumber,
            ReviewedSubmissionId = view.ReviewedSubmissionId,
            Versions = versions,
            Tables = tables,
            Sections = sections,
            Rows = rows,
            ColumnBlocks = columnBlocks,
            TableRevisions = tableRevisions,
            SectionRevisions = sectionRevisions,
            RowRevisions = rowRevisions,
            ColumnBlockRevisions = columnBlockRevisions,
            Values = shownValues,
            TemplateSections = await TemplateSectionQueries.ListInVersionsAsync(db, versionIds, cancellationToken),
            TemplateColumnBlocks = await TemplateColumnBlockQueries.ListInVersionsAsync(db, versionIds, cancellationToken),
            TemplateRows = templateRows,
            CellTypes = await CellTypeQueries.GetAllWithOptionsByIdAsync(db, cancellationToken),
            AvailableTemplates = availableTemplates,
            LatestTemplateVersions = await TableTemplateQueries.GetLatestVersionNumbersAsync(
                db,
                availableTemplates.Select(template => template.Id).ToList(),
                cancellationToken),
            UserNames = await UserQueries.GetAllDisplayNamesAsync(db, cancellationToken),
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

    private static Dictionary<int, RevisionResolution<TRevision>> Resolve<TRevision>(
        List<TRevision> revisions,
        Func<TRevision, int> itemId,
        SheetViewPoint view,
        int currentUserId)
        where TRevision : class, ISheetRevision
    {
        // Reviewing a submission shows its drafts and nobody else's, the reviewer's own included.
        return view.ReviewedSubmissionId is { } submissionId
            ? RevisionResolver.Resolve(revisions, itemId, draft => draft.SheetSubmissionId == submissionId)
            : RevisionResolver.Resolve(revisions, itemId, currentUserId);
    }

    /// <summary>
    /// Gives every cell its template cell from the template rows already read, and reads only those that
    /// aren't among them in one more query. Joining them into the rows' query instead would read and build
    /// each template cell again for every cell made from it.
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
            foreach (var templateCell in await TemplateCellQueries.ListAsync(db, missingIds, cancellationToken))
            {
                templateCells[templateCell.Id] = templateCell;
            }
        }

        foreach (var cell in cells)
        {
            cell.TemplateCell = templateCells[cell.TemplateCellId];
        }
    }

    /// <summary>
    /// The values of the row revisions shown. They are read for every revision the view could show, found
    /// by the sheet in the database rather than sent as a list of ids, and other people's drafts are then
    /// left out.
    /// </summary>
    private async Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadShownValuesAsync(
        int sheetId,
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

        var loaded = await RowValueQueries.LoadInViewAsync(db, sheetId, moment, cancellationToken);
        foreach (var revisionId in loaded.Keys.Where(revisionId => !shownRowRevisionIds.Contains(revisionId)).ToList())
        {
            loaded.Remove(revisionId);
        }

        return loaded;
    }
}
