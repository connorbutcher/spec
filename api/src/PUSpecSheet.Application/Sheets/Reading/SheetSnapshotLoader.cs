using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence.CellTypes;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Application.Sheets.History;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
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
    private readonly SheetStructureRepositories sheet;
    private readonly SheetRevisionRepositories revisions;
    private readonly TemplateRepositories templates;
    private readonly ICellTypeRepository cellTypes;
    private readonly IUserRepository users;
    private readonly IRowValueRepository values;
    private readonly SheetChangeHistoryLoader changeLoader;

    public SheetSnapshotLoader(
        SheetStructureRepositories sheet,
        SheetRevisionRepositories revisions,
        TemplateRepositories templates,
        ICellTypeRepository cellTypes,
        IUserRepository users,
        IRowValueRepository values,
        SheetChangeHistoryLoader changeLoader)
    {
        this.sheet = sheet;
        this.revisions = revisions;
        this.templates = templates;
        this.cellTypes = cellTypes;
        this.users = users;
        this.values = values;
        this.changeLoader = changeLoader;
    }

    public async Task<SheetSnapshot> LoadAsync(int sheetId, SheetViewPoint view, int currentUserId, CancellationToken cancellationToken)
    {
        var found = await sheet.Sheets.FindAsync(sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");
        var versions = await sheet.Sheets.ListVersionsAsync(sheetId, cancellationToken);
        var (moment, versionNumber) = ResolveMoment(view, versions);
        var tables = await sheet.Tables.ListOnSheetAsync(sheetId, cancellationToken);
        var sections = await sheet.Sections.ListOnSheetAsync(sheetId, cancellationToken);
        var rows = await sheet.Rows.ListOnSheetAsync(sheetId, cancellationToken);
        var columnBlocks = await sheet.ColumnBlocks.ListOnSheetAsync(sheetId, cancellationToken);

        var tableRevisions = Resolve(await revisions.Tables.ListInViewAsync(sheetId, moment, cancellationToken), revision => revision.SheetTableId, view, currentUserId);
        var sectionRevisions = Resolve(await revisions.Sections.ListInViewAsync(sheetId, moment, cancellationToken), revision => revision.SheetSectionId, view, currentUserId);
        var rowRevisions = Resolve(await revisions.Rows.ListInViewAsync(sheetId, moment, cancellationToken), revision => revision.SheetRowId, view, currentUserId);
        var columnBlockRevisions = Resolve(await revisions.ColumnBlocks.ListInViewAsync(sheetId, moment, cancellationToken), revision => revision.SheetColumnBlockId, view, currentUserId);
        var shownValues = await LoadShownValuesAsync(sheetId, rowRevisions, moment, cancellationToken);

        var versionIds = tables.Select(table => table.TableTemplateVersionId).Distinct().ToList();
        var templateRows = await templates.Rows.ListWithCellsInVersionsAsync(versionIds, cancellationToken);
        await AttachTemplateCellsAsync(rows, templateRows, cancellationToken);
        var availableTemplates = await templates.Templates.ListForSheetTypeAsync(found.SheetTypeId, cancellationToken);

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
            TemplateSections = await templates.Sections.ListInVersionsAsync(versionIds, cancellationToken),
            TemplateColumnBlocks = await templates.ColumnBlocks.ListInVersionsAsync(versionIds, cancellationToken),
            TemplateRows = templateRows,
            CellTypes = await cellTypes.GetAllWithOptionsByIdAsync(cancellationToken),
            AvailableTemplates = availableTemplates,
            LatestTemplateVersions = await templates.Templates.GetLatestVersionNumbersAsync(
                availableTemplates.Select(template => template.Id).ToList(),
                cancellationToken),
            UserNames = await users.GetAllDisplayNamesAsync(cancellationToken),
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
            foreach (var templateCell in await templates.Cells.ListAsync(missingIds, cancellationToken))
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

        var loaded = await values.LoadInViewAsync(sheetId, moment, cancellationToken);
        foreach (var revisionId in loaded.Keys.Where(revisionId => !shownRowRevisionIds.Contains(revisionId)).ToList())
        {
            loaded.Remove(revisionId);
        }

        return loaded;
    }
}
