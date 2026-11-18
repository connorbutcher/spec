using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// The base of the handlers that apply an operation to a sheet's drafts in the revisions, rather than to its copy. The ids
/// in the operations are those the draft is read with: its items' own, and a value's is its cell's.
/// </summary>
internal abstract class DraftOperationHandlerBase : OperationHandlerBase
{
    protected DraftOperationHandlerBase(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user)
    {
        SheetId = sheetId;
        Tables = new TableDrafts(dbContext, user, sheetId);
        Groups = new GroupDrafts(dbContext, user, sheetId);
        Rows = new RowDrafts(dbContext, user, sheetId);
        Values = new RowDraftValues(dbContext);
        Owners = new DraftItemOwners(dbContext, sheetId);
    }

    protected int SheetId { get; }

    protected TableDrafts Tables { get; }

    protected GroupDrafts Groups { get; }

    protected RowDrafts Rows { get; }

    protected RowDraftValues Values { get; }

    protected DraftItemOwners Owners { get; }

    protected async Task EnsureTableOnSheetAsync(int tableId, CancellationToken ct)
    {
        var isOnSheet = await DbContext.SheetTables.AnyAsync(t => t.Id == tableId && t.SheetId == SheetId, ct);
        if (!isOnSheet)
        {
            throw new KeyNotFoundException($"SheetTable with Id {tableId} not found on Sheet with Id {SheetId}.");
        }
    }

    protected async Task EnsureGroupOnSheetAsync(int groupId, CancellationToken ct)
    {
        var isOnSheet = await DbContext.SheetTableGroups
            .AnyAsync(g => g.Id == groupId && DbContext.SheetTables.Any(t => t.Id == g.SheetTableId && t.SheetId == SheetId), ct);
        if (!isOnSheet)
        {
            throw new KeyNotFoundException($"SheetTableGroup with Id {groupId} not found on Sheet with Id {SheetId}.");
        }
    }
}