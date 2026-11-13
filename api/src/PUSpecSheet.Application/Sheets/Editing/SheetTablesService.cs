using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Layout;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Templates.Queries;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Editing;

public sealed class SheetTablesService : ISheetTablesService
{
    private readonly PuSpecSheetDbContext db;
    private readonly TableDrafts drafts;
    private readonly SheetInstantiator instantiator;
    private readonly ISheetCellFiller filler;
    private readonly SheetReader reader;
    private readonly RemovalContentsGuard contents;
    private readonly ICurrentUser currentUser;
    private readonly ISheetSectionsService sections;

    public SheetTablesService(
        PuSpecSheetDbContext db,
        TableDrafts drafts,
        SheetInstantiator instantiator,
        ISheetCellFiller filler,
        SheetReader reader,
        RemovalContentsGuard contents,
        ICurrentUser currentUser,
        ISheetSectionsService sections)
    {
        this.db = db;
        this.drafts = drafts;
        this.instantiator = instantiator;
        this.filler = filler;
        this.reader = reader;
        this.contents = contents;
        this.currentUser = currentUser;
        this.sections = sections;
    }

    public async Task<SheetDto> AddAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken)
    {
        var sheet = await SheetQueries.FindAsync(db, sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");

        var template = await TableTemplateQueries.FindForSheetTypeAsync(db, request.TableTemplateId, sheet.SheetTypeId, cancellationToken)
            ?? throw new InvalidRequestException("That table template isn't for this kind of sheet.");

        var version = await TableTemplateQueries.GetLatestVersionAsync(db, template.Id, cancellationToken);
        var tree = await TemplateTree.LoadAsync(db, version.Id, cancellationToken);

        var orders = await VisibleTableOrdersAsync(sheetId, excludingTableId: null, cancellationToken);
        var table = new DbSheetTable { SheetId = sheetId, TableTemplateVersionId = version.Id };
        table.Revisions.Add(new DbSheetTableRevision
        {
            SheetId = sheetId,
            RevisionNumber = 1,
            Status = RevisionStatus.Draft,
            DisplayOrder = OrderGaps.Next(orders),
            AuthorUserId = currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });
        instantiator.AddStartingSections(table, tree);
        instantiator.AddStartingColumnBlocks(table, tree);

        db.SheetTables.Add(table);
        await db.SaveSheetChangesAsync(cancellationToken);
        await filler.FillAsync(table.Id, cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> SetTitleAsync(int tableId, UpdateSheetTableRequest request, CancellationToken cancellationToken)
    {
        var state = await drafts.LoadAsync(tableId, cancellationToken);
        var draft = await MyDraftAsync(tableId, state, cancellationToken);
        draft.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();

        await db.SaveSheetChangesAsync(cancellationToken);
        await drafts.ReleaseIfUnchangedAsync(tableId, cancellationToken);
        return await reader.ReadLiveAsync(await SheetIdOfAsync(tableId, cancellationToken), cancellationToken);
    }

    public async Task<SheetDto> MoveAsync(int tableId, MoveRequest request, CancellationToken cancellationToken)
    {
        var sheetId = await SheetIdOfAsync(tableId, cancellationToken);
        var state = await drafts.LoadAsync(tableId, cancellationToken);

        var siblingOrders = await VisibleTableOrdersAsync(sheetId, tableId, cancellationToken);
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder, state.Current?.DisplayOrder);

        var draft = await MyDraftAsync(tableId, state, cancellationToken);
        draft.DisplayOrder = order;

        await db.SaveSheetChangesAsync(cancellationToken);
        await drafts.ReleaseIfUnchangedAsync(tableId, cancellationToken);
        await drafts.ReleaseRestoredOrderAsync(
            await SheetTableQueries.ListIdsOnSheetAsync(db, sheetId, cancellationToken),
            cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> RemoveAsync(int tableId, CancellationToken cancellationToken)
    {
        var sheetId = await SheetIdOfAsync(tableId, cancellationToken);
        var state = await drafts.LoadAsync(tableId, cancellationToken);
        await drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);
        await contents.EnsureTableFreeAsync(tableId, cancellationToken);

        if (state.IsNew)
        {
            // Only its author ever saw it, so there's no history to keep.
            await SheetTableQueries.DeleteAsync(db, tableId, cancellationToken);
        }
        else
        {
            var draft = await MyDraftAsync(tableId, state, cancellationToken);
            draft.IsDeleted = true;
            await db.SaveSheetChangesAsync(cancellationToken);
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public Task<SheetDto> AddSectionAsync(int tableId, AddSheetSectionRequest request, CancellationToken cancellationToken)
    {
        return sections.AddAsync(tableId, request, cancellationToken);
    }

    /// <summary>The user's draft on the table, carrying over the published title when it's newly started.</summary>
    private async Task<DbSheetTableRevision> MyDraftAsync(int tableId, DraftState<DbSheetTableRevision> state, CancellationToken cancellationToken)
    {
        var (draft, created) = await drafts.EnsureMineAsync(tableId, state, cancellationToken);
        if (created)
        {
            draft.Title = state.Current?.Title;
        }

        return draft;
    }

    private async Task<int> SheetIdOfAsync(int tableId, CancellationToken cancellationToken)
    {
        return await SheetTableQueries.FindSheetIdAsync(db, tableId, cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");
    }

    /// <summary>The display orders of the tables the user can see, ascending, optionally leaving one out.</summary>
    private async Task<IReadOnlyList<int>> VisibleTableOrdersAsync(int sheetId, int? excludingTableId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var revisions = await SheetTableRevisionQueries.ListVisibleOnSheetAsync(db, sheetId, me, cancellationToken);

        return RevisionResolver.Resolve(revisions, revision => revision.SheetTableId, me)
            .Where(entry => entry.Key != excludingTableId && RevisionResolver.IsVisible(entry.Value))
            .Select(entry => entry.Value.Shown!.DisplayOrder)
            .Order()
            .ToList();
    }
}
