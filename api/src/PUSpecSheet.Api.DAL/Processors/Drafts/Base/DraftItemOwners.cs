using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// Finds the table or group a widget or note belongs to, on the sheet being edited, for the operations that name the widget or note by its id.
/// </summary>
internal class DraftItemOwners
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly int _sheetId;

    public DraftItemOwners(PUSpecSheetDbContext dbContext, int sheetId)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
        _sheetId = sheetId;
    }

    public async Task<int> TableOfWidgetAsync(int widgetId, CancellationToken ct)
    {
        return await _dbContext.SheetTableWidgets
            .Where(w => w.Id == widgetId && _dbContext.SheetTables.Any(t => t.Id == w.SheetTableId && t.SheetId == _sheetId))
            .Select(w => (int?)w.SheetTableId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"SheetTableWidget with Id {widgetId} not found on Sheet with Id {_sheetId}.");
    }

    public async Task<int> TableOfNoteAsync(int noteId, CancellationToken ct)
    {
        return await _dbContext.SheetTableNotes
            .Where(n => n.Id == noteId && _dbContext.SheetTables.Any(t => t.Id == n.SheetTableId && t.SheetId == _sheetId))
            .Select(n => (int?)n.SheetTableId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"SheetTableNote with Id {noteId} not found on Sheet with Id {_sheetId}.");
    }

    public async Task<int> GroupOfWidgetAsync(int widgetId, CancellationToken ct)
    {
        return await _dbContext.SheetTableGroupWidgets
            .Where(w => w.Id == widgetId && _dbContext.SheetTableGroups.Any(g => g.Id == w.SheetTableGroupId && _dbContext.SheetTables.Any(t => t.Id == g.SheetTableId && t.SheetId == _sheetId)))
            .Select(w => (int?)w.SheetTableGroupId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"SheetTableGroupWidget with Id {widgetId} not found on Sheet with Id {_sheetId}.");
    }
}
