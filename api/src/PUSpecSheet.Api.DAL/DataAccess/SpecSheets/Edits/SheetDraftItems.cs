using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.SpecSheets;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Makes the widget and note rows of a table or group draft hold what a spec sheet's table or group holds. A widget is
/// matched to the table's or group's by its template widget, and a note by who wrote it, when and its source system. One
/// the table or group hasn't got is added as an item, and a row for one the spec sheet hasn't got is dropped from the draft.
/// </summary>
internal class SheetDraftItems
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly TableDrafts _tables;
    private readonly GroupDrafts _groups;

    public SheetDraftItems(PUSpecSheetDbContext dbContext, TableDrafts tables, GroupDrafts groups)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(tables, nameof(tables));
        ArgumentNullException.ThrowIfNull(groups, nameof(groups));

        _dbContext = dbContext;
        _tables = tables;
        _groups = groups;
    }

    /// <param name="draft">The table's draft.</param>
    /// <param name="newTable">The table, when it is added in the draft and not saved yet, otherwise null.</param>
    /// <param name="source">The table to copy the widgets and notes of.</param>
    public async Task SetTableItemsAsync(DbSheetTableRevision draft, DbSheetTable? newTable, SpecSheetTable source, CancellationToken ct)
    {
        await SetTableWidgetsAsync(draft, newTable, source, ct);
        await SetNotesAsync(draft, newTable, source, ct);
    }

    /// <param name="draft">The group's draft.</param>
    /// <param name="newGroup">The group, when it is added in the draft and not saved yet, otherwise null.</param>
    /// <param name="source">The group to copy the widgets of.</param>
    public async Task SetGroupItemsAsync(DbSheetTableGroupRevision draft, DbSheetTableGroup? newGroup, SpecSheetTableGroup source, CancellationToken ct)
    {
        var items = newGroup is null
            ? await _dbContext.SheetTableGroupWidgets.Where(w => w.SheetTableGroupId == draft.SheetTableGroupId).ToListAsync(ct)
            : [];
        var values = await _groups.LoadWidgetValuesAsync(draft, ct);
        var wanted = new HashSet<DbSheetTableGroupWidgetValue>(ReferenceEqualityComparer.Instance);

        foreach (var sourceWidget in source.Widgets)
        {
            var item = items.FirstOrDefault(w => w.TableGroupTemplateWidgetId == sourceWidget.TableGroupTemplateWidgetId);
            if (item is null)
            {
                item = new DbSheetTableGroupWidget
                {
                    Id = 0,
                    SheetTableGroupId = draft.SheetTableGroupId,
                    TableGroupTemplateWidgetId = sourceWidget.TableGroupTemplateWidgetId,
                    CreatedDateTime = DateTime.UtcNow
                };
                items.Add(item);
                if (newGroup is null)
                {
                    await _dbContext.SheetTableGroupWidgets.AddAsync(item, ct);
                }
                else
                {
                    newGroup.Widgets.Add(item);
                }
            }

            var value = values.FirstOrDefault(v => ReferenceEquals(v.SheetTableGroupWidget, item) || (item.Id != 0 && v.SheetTableGroupWidgetId == item.Id));
            if (value is null)
            {
                value = new DbSheetTableGroupWidgetValue
                {
                    SheetTableGroupRevisionId = 0,
                    SheetTableGroupWidgetId = item.Id,
                    Value = null,
                    SheetTableGroupWidget = item
                };
                values.Add(value);
            }

            value.Value = sourceWidget.Value.Value;
            wanted.Add(value);
        }

        Drop(values, wanted);
    }

    private async Task SetTableWidgetsAsync(DbSheetTableRevision draft, DbSheetTable? newTable, SpecSheetTable source, CancellationToken ct)
    {
        var items = newTable is null
            ? await _dbContext.SheetTableWidgets.Where(w => w.SheetTableId == draft.SheetTableId).ToListAsync(ct)
            : [];
        var values = await _tables.LoadWidgetValuesAsync(draft, ct);
        var wanted = new HashSet<DbSheetTableWidgetValue>(ReferenceEqualityComparer.Instance);

        foreach (var sourceWidget in source.Widgets)
        {
            var item = items.FirstOrDefault(w => w.TableTemplateWidgetId == sourceWidget.TableTemplateWidgetId);
            if (item is null)
            {
                item = new DbSheetTableWidget
                {
                    Id = 0,
                    SheetTableId = draft.SheetTableId,
                    TableTemplateWidgetId = sourceWidget.TableTemplateWidgetId,
                    CreatedDateTime = DateTime.UtcNow
                };
                items.Add(item);
                if (newTable is null)
                {
                    await _dbContext.SheetTableWidgets.AddAsync(item, ct);
                }
                else
                {
                    newTable.Widgets.Add(item);
                }
            }

            var value = values.FirstOrDefault(v => ReferenceEquals(v.SheetTableWidget, item) || (item.Id != 0 && v.SheetTableWidgetId == item.Id));
            if (value is null)
            {
                value = new DbSheetTableWidgetValue
                {
                    SheetTableRevisionId = 0,
                    SheetTableWidgetId = item.Id,
                    Value = null,
                    SheetTableWidget = item
                };
                values.Add(value);
            }

            value.Value = sourceWidget.Value.Value;
            wanted.Add(value);
        }

        Drop(values, wanted);
    }

    private async Task SetNotesAsync(DbSheetTableRevision draft, DbSheetTable? newTable, SpecSheetTable source, CancellationToken ct)
    {
        var items = newTable is null
            ? await _dbContext.SheetTableNotes.Where(n => n.SheetTableId == draft.SheetTableId).ToListAsync(ct)
            : [];
        var values = await _tables.LoadNoteValuesAsync(draft, ct);
        var wanted = new HashSet<DbSheetTableNoteValue>(ReferenceEqualityComparer.Instance);

        foreach (var sourceNote in source.Notes.OrderBy(n => n.Id))
        {
            // A note is told apart from another by who wrote it and when, so two identical ones are matched in order.
            var item = items.FirstOrDefault(n => n.CreatedByUserId == sourceNote.CreatedByUserId
                && n.CreatedDateTime == sourceNote.CreatedDateTime
                && n.SourceSystem == sourceNote.SourceSystem
                && !values.Any(v => wanted.Contains(v) && (ReferenceEquals(v.SheetTableNote, n) || (n.Id != 0 && v.SheetTableNoteId == n.Id))));
            if (item is null)
            {
                item = new DbSheetTableNote
                {
                    Id = 0,
                    SheetTableId = draft.SheetTableId,
                    SourceSystem = sourceNote.SourceSystem,
                    CreatedByUserId = sourceNote.CreatedByUserId,
                    CreatedDateTime = sourceNote.CreatedDateTime
                };
                items.Add(item);
                if (newTable is null)
                {
                    await _dbContext.SheetTableNotes.AddAsync(item, ct);
                }
                else
                {
                    newTable.Notes.Add(item);
                }
            }

            var value = values.FirstOrDefault(v => ReferenceEquals(v.SheetTableNote, item) || (item.Id != 0 && v.SheetTableNoteId == item.Id));
            if (value is null)
            {
                value = new DbSheetTableNoteValue
                {
                    SheetTableRevisionId = 0,
                    SheetTableNoteId = item.Id,
                    Note = sourceNote.Note,
                    IsObsolete = sourceNote.IsObsolete,
                    SheetTableNote = item
                };
                values.Add(value);
            }
            else
            {
                _dbContext.Entry(value).Property(v => v.Note).CurrentValue = sourceNote.Note;
                value.IsObsolete = sourceNote.IsObsolete;
            }

            wanted.Add(value);
        }

        Drop(values, wanted);
    }

    // What the draft has beyond what is wanted isn't in the spec sheet.
    private void Drop<TValue>(ICollection<TValue> values, HashSet<TValue> wanted)
        where TValue : class
    {
        foreach (var stale in values.Where(value => !wanted.Contains(value)).ToList())
        {
            values.Remove(stale);
            _dbContext.Remove(stale);
        }
    }
}
