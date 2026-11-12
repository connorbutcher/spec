using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Layout;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Editing;

public sealed class SheetTableService : ISheetTableService
{
    private readonly ISheetRepository sheets;
    private readonly ISheetTableRepository tables;
    private readonly ISheetTableRevisionRepository tableRevisions;
    private readonly TemplateRepositories templates;
    private readonly IUnitOfWork unitOfWork;
    private readonly TableDrafts drafts;
    private readonly SheetInstantiator instantiator;
    private readonly ISheetCellFiller filler;
    private readonly SheetReader reader;
    private readonly RemovalContentsGuard contents;
    private readonly ICurrentUser currentUser;

    public SheetTableService(
        ISheetRepository sheets,
        ISheetTableRepository tables,
        ISheetTableRevisionRepository tableRevisions,
        TemplateRepositories templates,
        IUnitOfWork unitOfWork,
        TableDrafts drafts,
        SheetInstantiator instantiator,
        ISheetCellFiller filler,
        SheetReader reader,
        RemovalContentsGuard contents,
        ICurrentUser currentUser)
    {
        this.sheets = sheets;
        this.tables = tables;
        this.tableRevisions = tableRevisions;
        this.templates = templates;
        this.unitOfWork = unitOfWork;
        this.drafts = drafts;
        this.instantiator = instantiator;
        this.filler = filler;
        this.reader = reader;
        this.contents = contents;
        this.currentUser = currentUser;
    }

    public async Task<SheetDto> AddAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken)
    {
        var sheet = await sheets.FindAsync(sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");

        var template = await templates.Templates.FindForSheetTypeAsync(request.TableTemplateId, sheet.SheetTypeId, cancellationToken)
            ?? throw new InvalidRequestException("That table template isn't for this kind of sheet.");

        var version = await templates.Templates.GetLatestVersionAsync(template.Id, cancellationToken);
        var tree = await TemplateTree.LoadAsync(templates.Sections, templates.ColumnBlocks, version.Id, cancellationToken);

        var orders = await VisibleTableOrdersAsync(sheetId, excludingTableId: null, cancellationToken);
        var table = new SheetTable { SheetId = sheetId, TableTemplateVersionId = version.Id };
        table.Revisions.Add(new SheetTableRevision
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

        tables.Add(table);
        await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        await filler.FillAsync(table.Id, cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> SetTitleAsync(int tableId, UpdateSheetTableRequest request, CancellationToken cancellationToken)
    {
        var state = await drafts.LoadAsync(tableId, cancellationToken);
        var draft = await MyDraftAsync(tableId, state, cancellationToken);
        draft.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();

        await unitOfWork.SaveSheetChangesAsync(cancellationToken);
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

        await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        await drafts.ReleaseIfUnchangedAsync(tableId, cancellationToken);
        await drafts.ReleaseRestoredOrderAsync(
            await tables.ListIdsOnSheetAsync(sheetId, cancellationToken),
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
            await tables.DeleteAsync(tableId, cancellationToken);
        }
        else
        {
            var draft = await MyDraftAsync(tableId, state, cancellationToken);
            draft.IsDeleted = true;
            await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    /// <summary>The user's draft on the table, carrying over the published title when it's newly started.</summary>
    private async Task<SheetTableRevision> MyDraftAsync(int tableId, DraftState<SheetTableRevision> state, CancellationToken cancellationToken)
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
        return await tables.FindSheetIdAsync(tableId, cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");
    }

    /// <summary>The display orders of the tables the user can see, ascending, optionally leaving one out.</summary>
    private async Task<IReadOnlyList<int>> VisibleTableOrdersAsync(int sheetId, int? excludingTableId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var revisions = await tableRevisions.ListVisibleOnSheetAsync(sheetId, me, cancellationToken);

        return RevisionResolver.Resolve(revisions, revision => revision.SheetTableId, me)
            .Where(entry => entry.Key != excludingTableId && RevisionResolver.IsVisible(entry.Value))
            .Select(entry => entry.Value.Shown!.DisplayOrder)
            .Order()
            .ToList();
    }
}
