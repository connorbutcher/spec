using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Layout;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Templates;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

public sealed class SheetTablesService : ISheetTablesService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly TableDrafts _drafts;
    private readonly SheetInstantiator _instantiator;
    private readonly ISheetCellFiller _filler;
    private readonly SheetReader _reader;
    private readonly RemovalContentsGuard _contents;
    private readonly ICurrentUser _currentUser;
    private readonly ISheetSectionsService _sections;
    private readonly ITableTemplateDataService _tableTemplateData;
    private readonly ITemplateSectionDataService _templateSectionData;
    private readonly ITemplateColumnBlockDataService _templateColumnBlockData;

    public SheetTablesService(
        PuSpecSheetDbContext db,
        TableDrafts drafts,
        SheetInstantiator instantiator,
        ISheetCellFiller filler,
        SheetReader reader,
        RemovalContentsGuard contents,
        ICurrentUser currentUser,
        ISheetSectionsService sections,
        ITableTemplateDataService tableTemplateData,
        ITemplateSectionDataService templateSectionData,
        ITemplateColumnBlockDataService templateColumnBlockData)
    {
        _db = db;
        _drafts = drafts;
        _instantiator = instantiator;
        _filler = filler;
        _reader = reader;
        _contents = contents;
        _currentUser = currentUser;
        _sections = sections;
        _tableTemplateData = tableTemplateData;
        _templateSectionData = templateSectionData;
        _templateColumnBlockData = templateColumnBlockData;
    }

    public async Task<Sheet> AddAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken)
    {
        var sheet = await SheetQueries.FindAsync(_db, sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");

        var template = await _tableTemplateData.FindForSheetTypeAsync(request.TableTemplateId, sheet.SheetTypeId, cancellationToken)
            ?? throw new InvalidRequestException("That table template isn't for this kind of sheet.");

        var version = await _tableTemplateData.GetLatestVersionAsync(template.Id, cancellationToken);
        var tree = new TemplateTree(
            await _templateSectionData.ListForLayoutAsync(version.Id, cancellationToken),
            await _templateColumnBlockData.ListAsync(version.Id, cancellationToken));

        var orders = await VisibleTableOrdersAsync(sheetId, excludingTableId: null, cancellationToken);
        var table = new DbSheetTable { SheetId = sheetId, TableTemplateVersionId = version.Id };
        table.Revisions.Add(new DbSheetTableRevision
        {
            SheetId = sheetId,
            RevisionNumber = 1,
            Status = RevisionStatus.Draft,
            DisplayOrder = OrderGaps.Next(orders),
            AuthorUserId = _currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });
        _instantiator.AddStartingSections(table, tree);
        _instantiator.AddStartingColumnBlocks(table, tree);

        _db.SheetTables.Add(table);
        await _db.SaveSheetChangesAsync(cancellationToken);
        await _filler.FillAsync(table.Id, cancellationToken);
        return await _reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<Sheet> SetTitleAsync(int tableId, SetSheetTableTitleRequest request, CancellationToken cancellationToken)
    {
        var state = await _drafts.LoadAsync(tableId, cancellationToken);
        var draft = await MyDraftAsync(tableId, state, cancellationToken);
        draft.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();

        await _db.SaveSheetChangesAsync(cancellationToken);
        await _drafts.ReleaseIfUnchangedAsync(tableId, cancellationToken);
        return await _reader.ReadLiveAsync(await SheetIdOfAsync(tableId, cancellationToken), cancellationToken);
    }

    public async Task<Sheet> MoveAsync(int tableId, MoveRequest request, CancellationToken cancellationToken)
    {
        var sheetId = await SheetIdOfAsync(tableId, cancellationToken);
        var state = await _drafts.LoadAsync(tableId, cancellationToken);

        var siblingOrders = await VisibleTableOrdersAsync(sheetId, tableId, cancellationToken);
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder, state.Current?.DisplayOrder);

        var draft = await MyDraftAsync(tableId, state, cancellationToken);
        draft.DisplayOrder = order;

        await _db.SaveSheetChangesAsync(cancellationToken);
        await _drafts.ReleaseIfUnchangedAsync(tableId, cancellationToken);
        await _drafts.ReleaseRestoredOrderAsync(
            await SheetTableQueries.ListIdsOnSheetAsync(_db, sheetId, cancellationToken),
            cancellationToken);
        return await _reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<Sheet> RemoveAsync(int tableId, CancellationToken cancellationToken)
    {
        var sheetId = await SheetIdOfAsync(tableId, cancellationToken);
        var state = await _drafts.LoadAsync(tableId, cancellationToken);
        await _drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);
        await _contents.EnsureTableFreeAsync(tableId, cancellationToken);

        if (state.IsNew)
        {
            // Only its author ever saw it, so there's no history to keep.
            await SheetTableQueries.DeleteAsync(_db, tableId, cancellationToken);
        }
        else
        {
            var draft = await MyDraftAsync(tableId, state, cancellationToken);
            draft.IsDeleted = true;
            await _db.SaveSheetChangesAsync(cancellationToken);
        }

        return await _reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public Task<Sheet> AddSectionAsync(int tableId, AddSheetSectionRequest request, CancellationToken cancellationToken)
    {
        return _sections.AddAsync(tableId, request, cancellationToken);
    }

    /// <summary>The user's draft on the table, carrying over the published title when it's newly started.</summary>
    private async Task<DbSheetTableRevision> MyDraftAsync(int tableId, DraftState<DbSheetTableRevision> state, CancellationToken cancellationToken)
    {
        var (draft, created) = await _drafts.EnsureMineAsync(tableId, state, cancellationToken);
        if (created)
        {
            draft.Title = state.Current?.Title;
        }

        return draft;
    }

    private async Task<int> SheetIdOfAsync(int tableId, CancellationToken cancellationToken)
    {
        return await SheetTableQueries.FindSheetIdAsync(_db, tableId, cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");
    }

    /// <summary>The display orders of the tables the user can see, ascending, optionally leaving one out.</summary>
    private async Task<IReadOnlyList<int>> VisibleTableOrdersAsync(int sheetId, int? excludingTableId, CancellationToken cancellationToken)
    {
        var me = _currentUser.UserId;
        var revisions = await SheetTableRevisionQueries.ListVisibleOnSheetAsync(_db, sheetId, me, cancellationToken);

        return RevisionResolver.Resolve(revisions, revision => revision.SheetTableId, me)
            .Where(entry => entry.Key != excludingTableId && RevisionResolver.IsVisible(entry.Value))
            .Select(entry => entry.Value.Shown!.DisplayOrder)
            .Order()
            .ToList();
    }
}
