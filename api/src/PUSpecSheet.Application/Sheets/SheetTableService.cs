using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

public sealed class SheetTableService(
    PuSpecSheetDbContext db,
    TableDrafts drafts,
    SheetInstantiator instantiator,
    SheetReader reader,
    ICurrentUser currentUser) : ISheetTableService
{
    public async Task<SheetDto> AddAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken)
    {
        var sheet = await db.Sheets
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");

        var template = await db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == request.TableTemplateId && candidate.SheetTypeId == sheet.SheetTypeId, cancellationToken)
            ?? throw new InvalidRequestException("That table template isn't for this kind of sheet.");

        var version = await db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.TableTemplateId == template.Id)
            .OrderByDescending(candidate => candidate.VersionNumber)
            .FirstAsync(cancellationToken);
        var tree = await TemplateTree.LoadAsync(db, version.Id, cancellationToken);

        var orders = await VisibleTableOrdersAsync(sheetId, excludingTableId: null, cancellationToken);
        var table = new SheetTable { SheetId = sheetId, TableTemplateVersionId = version.Id };
        table.Revisions.Add(new SheetTableRevision
        {
            RevisionNumber = 1,
            Status = RevisionStatus.Draft,
            DisplayOrder = OrderGaps.Next(orders),
            AuthorUserId = currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });
        instantiator.AddStartingSections(table, tree);

        db.SheetTables.Add(table);
        await db.SaveSheetChangesAsync(cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> SetTitleAsync(int tableId, UpdateSheetTableRequest request, CancellationToken cancellationToken)
    {
        var state = await drafts.LoadAsync(tableId, cancellationToken);
        var draft = await MyDraftAsync(tableId, state, cancellationToken);
        draft.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();

        await db.SaveSheetChangesAsync(cancellationToken);
        return await reader.ReadLiveAsync(await SheetIdOfAsync(tableId, cancellationToken), cancellationToken);
    }

    public async Task<SheetDto> MoveAsync(int tableId, MoveRequest request, CancellationToken cancellationToken)
    {
        var sheetId = await SheetIdOfAsync(tableId, cancellationToken);
        var state = await drafts.LoadAsync(tableId, cancellationToken);

        var siblingOrders = await VisibleTableOrdersAsync(sheetId, tableId, cancellationToken);
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder);

        var draft = await MyDraftAsync(tableId, state, cancellationToken);
        draft.DisplayOrder = order;

        await db.SaveSheetChangesAsync(cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> RemoveAsync(int tableId, CancellationToken cancellationToken)
    {
        var sheetId = await SheetIdOfAsync(tableId, cancellationToken);
        var state = await drafts.LoadAsync(tableId, cancellationToken);
        await drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);

        var me = currentUser.UserId;
        var othersInside = await db.SheetSectionRevisions
            .AnyAsync(revision => revision.SheetSection.SheetTableId == tableId && revision.Status == RevisionStatus.Draft && revision.AuthorUserId != me, cancellationToken)
            || await db.SheetRowRevisions
                .AnyAsync(revision => revision.SheetRow.SheetSection.SheetTableId == tableId && revision.Status == RevisionStatus.Draft && revision.AuthorUserId != me, cancellationToken);
        if (othersInside)
        {
            throw new ConflictException("Someone else is editing inside this table, so it can't be removed yet.");
        }

        if (state.IsNew)
        {
            // Only its author ever saw it, so there's no history to keep.
            await db.SheetTables.Where(table => table.Id == tableId).ExecuteDeleteAsync(cancellationToken);
        }
        else
        {
            var draft = await MyDraftAsync(tableId, state, cancellationToken);
            draft.IsDeleted = true;
            await db.SaveSheetChangesAsync(cancellationToken);
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
        return await db.SheetTables
            .Where(table => table.Id == tableId)
            .Select(table => (int?)table.SheetId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");
    }

    /// <summary>The display orders of the tables the user can see, ascending, optionally leaving one out.</summary>
    private async Task<IReadOnlyList<int>> VisibleTableOrdersAsync(int sheetId, int? excludingTableId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var revisions = await db.SheetTableRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetTable.SheetId == sheetId
                && revision.SupersededAtUtc == null
                && (revision.Status == RevisionStatus.Published || revision.AuthorUserId == me))
            .ToListAsync(cancellationToken);

        return RevisionResolver.Resolve(revisions, revision => revision.SheetTableId, me)
            .Where(entry => entry.Key != excludingTableId && RevisionResolver.IsVisible(entry.Value))
            .Select(entry => entry.Value.Shown!.DisplayOrder)
            .Order()
            .ToList();
    }
}
