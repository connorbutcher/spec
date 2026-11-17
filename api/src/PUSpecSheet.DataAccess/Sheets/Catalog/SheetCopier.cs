using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.DataAccess.Sheets.Catalog;

// Starts a new sheet as a copy of another: the tables, column blocks, sections, rows and values the source has at its
// latest published version, built again on the new sheet as the current user's first drafts. So the copy is theirs to
// change, and reaches everyone else when it is approved, like any other change. Each table keeps the template version
// it was built from. A linked dropdown pointed at a table or row of the source is pointed at the copy of it.
public sealed class SheetCopier
{
    private readonly PuSpecSheetDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ISheetTableDataService _sheetTableData;
    private readonly ISheetColumnBlockDataService _sheetColumnBlockData;
    private readonly ISheetSectionDataService _sheetSectionData;
    private readonly ISheetRowDataService _sheetRowData;
    private readonly ISheetCellDataService _sheetCellData;
    private readonly ICellValueDataService _cellValueData;

    public SheetCopier(
        PuSpecSheetDbContext db,
        ICurrentUser currentUser,
        ISheetTableDataService sheetTableData,
        ISheetColumnBlockDataService sheetColumnBlockData,
        ISheetSectionDataService sheetSectionData,
        ISheetRowDataService sheetRowData,
        ISheetCellDataService sheetCellData,
        ICellValueDataService cellValueData)
    {
        _db = db;
        _currentUser = currentUser;
        _sheetTableData = sheetTableData;
        _sheetColumnBlockData = sheetColumnBlockData;
        _sheetSectionData = sheetSectionData;
        _sheetRowData = sheetRowData;
        _sheetCellData = sheetCellData;
        _cellValueData = cellValueData;
    }

    public async Task CopyAsync(int sourceSheetId, int targetSheetId, CancellationToken cancellationToken)
    {
        var map = new SheetCopyMap();
        var now = DateTime.UtcNow;

        var tables = await _sheetTableData.ListCurrentOnSheetAsync(sourceSheetId, cancellationToken);
        foreach (var revision in tables)
        {
            var table = new DbSheetTable { SheetId = targetSheetId, TableTemplateVersionId = revision.SheetTable.TableTemplateVersionId };
            var copy = Draft<DbSheetTableRevision>(targetSheetId, revision.DisplayOrder, now);
            copy.Title = revision.Title;
            table.Revisions.Add(copy);
            _db.SheetTables.Add(table);
            map.Tables[revision.SheetTableId] = table;
        }

        var blocks = await _sheetColumnBlockData.ListCurrentOnSheetAsync(sourceSheetId, cancellationToken);
        foreach (var revision in blocks)
        {
            if (map.Tables.TryGetValue(revision.SheetColumnBlock.SheetTableId, out var table))
            {
                var block = new DbSheetColumnBlock { SheetTable = table, TemplateColumnBlockId = revision.SheetColumnBlock.TemplateColumnBlockId };
                block.Revisions.Add(Draft<DbSheetColumnBlockRevision>(targetSheetId, revision.DisplayOrder, now));
                table.ColumnBlocks.Add(block);
                map.ColumnBlocks[revision.SheetColumnBlockId] = block;
            }
        }

        var sections = await _sheetSectionData.ListCurrentOnSheetAsync(sourceSheetId, cancellationToken);
        CopySections(sections, map, targetSheetId, now);

        var rows = await _sheetRowData.ListCurrentOnSheetAsync(sourceSheetId, cancellationToken);
        foreach (var revision in rows)
        {
            if (map.Sections.TryGetValue(revision.SheetRow.SheetSectionId, out var section))
            {
                var row = new DbSheetRow { SheetSection = section, TemplateRowId = revision.SheetRow.TemplateRowId };
                var copy = Draft<DbSheetRowRevision>(targetSheetId, revision.DisplayOrder, now);
                row.Revisions.Add(copy);
                section.Rows.Add(row);
                map.Rows[revision.SheetRowId] = row;
                map.RowRevisions[revision.Id] = copy;
            }
        }

        var cells = await _sheetCellData.ListOnCurrentRowsAsync(sourceSheetId, cancellationToken);
        foreach (var cell in cells)
        {
            CopyCell(cell, map);
        }

        var values = await _cellValueData.ListCurrentOnSheetAsync(sourceSheetId, cancellationToken);
        CopyValues(values.Texts, map, value => new DbTextValue { Value = value.Value });
        CopyValues(values.Numbers, map, value => new DbNumericValue { Value = value.Value });
        CopyValues(values.Dates, map, value => new DbDateValue { Value = value.Value });
        CopyValues(values.Booleans, map, value => new DbBooleanValue { Value = value.Value });
        CopyValues(values.Options, map, value => new DbOptionValue { CellTypeOptionId = value.CellTypeOptionId });
        CopyValues(values.LabelledTexts, map, value => new DbLabelledTextValue { SlotKey = value.SlotKey, Position = value.Position, Value = value.Value });
        var linked = CopyPlainSettings(values.Settings, map);

        await _db.SaveChangesAsync(cancellationToken);

        // The tables and rows a linked dropdown reads only have ids once saved.
        foreach (var settings in linked)
        {
            AddLinkedSettings(settings, map);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static void CopyCell(DbSheetCell cell, SheetCopyMap map)
    {
        if (!map.Rows.TryGetValue(cell.SheetRowId, out var row))
        {
            return;
        }

        var copy = new DbSheetCell { TemplateCellId = cell.TemplateCellId };
        if (cell.SheetColumnBlockId is { } blockId)
        {
            if (!map.ColumnBlocks.TryGetValue(blockId, out var block))
            {
                return;
            }

            copy.SheetColumnBlock = block;
        }

        row.Cells.Add(copy);
        map.Cells[cell.Id] = copy;
    }

    // Copies sections parents first, so each copy can be given its parent's; a section whose parent isn't copied
    // isn't either.
    private void CopySections(List<DbSheetSectionRevision> revisions, SheetCopyMap map, int targetSheetId, DateTime now)
    {
        var byParent = revisions.ToLookup(revision => revision.SheetSection.ParentSheetSectionId);
        var pending = new Queue<DbSheetSectionRevision>(byParent[null]);
        while (pending.Count > 0)
        {
            var revision = pending.Dequeue();
            var source = revision.SheetSection;
            if (!map.Tables.TryGetValue(source.SheetTableId, out var table))
            {
                continue;
            }

            var section = new DbSheetSection { SheetTable = table, TemplateSectionId = source.TemplateSectionId };
            if (source.ParentSheetSectionId is { } parentId)
            {
                section.ParentSheetSection = map.Sections[parentId];
            }

            section.Revisions.Add(Draft<DbSheetSectionRevision>(targetSheetId, revision.DisplayOrder, now));
            table.Sections.Add(section);
            map.Sections[source.Id] = section;

            foreach (var child in byParent[source.Id])
            {
                pending.Enqueue(child);
            }
        }
    }

    private void CopyValues<TValue>(IReadOnlyList<TValue> source, SheetCopyMap map, Func<TValue, TValue> copy)
        where TValue : class, IDbCellValue
    {
        foreach (var value in source)
        {
            if (map.RowRevisions.TryGetValue(value.SheetRowRevisionId, out var revision) && map.Cells.TryGetValue(value.SheetCellId, out var cell))
            {
                var copied = copy(value);
                copied.SheetRowRevision = revision;
                copied.SheetCell = cell;
                _db.Add(copied);
            }
        }
    }

    private List<DbCellSettingsValue> CopyPlainSettings(IReadOnlyList<DbCellSettingsValue> source, SheetCopyMap map)
    {
        var linked = new List<DbCellSettingsValue>();
        foreach (var value in source)
        {
            if (value.Settings is LinkedDropdownInstanceSettings)
            {
                linked.Add(value);
            }
            else if (map.RowRevisions.TryGetValue(value.SheetRowRevisionId, out var revision) && map.Cells.TryGetValue(value.SheetCellId, out var cell))
            {
                _db.CellSettings.Add(new DbCellSettingsValue { SheetRowRevision = revision, SheetCell = cell, Settings = value.Settings });
            }
        }

        return linked;
    }

    private void AddLinkedSettings(DbCellSettingsValue value, SheetCopyMap map)
    {
        var settings = (LinkedDropdownInstanceSettings)value.Settings;
        if (!map.RowRevisions.TryGetValue(value.SheetRowRevisionId, out var revision) || !map.Cells.TryGetValue(value.SheetCellId, out var cell))
        {
            return;
        }

        int? tableId = null;
        if (settings.SourceSheetTableId is { } sourceTableId)
        {
            if (!map.Tables.TryGetValue(sourceTableId, out var table))
            {
                return;
            }

            tableId = table.Id;
        }

        int? rowId = null;
        if (settings.SourceSheetRowId is { } sourceRowId)
        {
            if (!map.Rows.TryGetValue(sourceRowId, out var row))
            {
                return;
            }

            rowId = row.Id;
        }

        _db.CellSettings.Add(new DbCellSettingsValue
        {
            SheetRowRevisionId = revision.Id,
            SheetCellId = cell.Id,
            Settings = settings with { SourceSheetTableId = tableId, SourceSheetRowId = rowId },
        });
    }

    private TRevision Draft<TRevision>(int sheetId, int displayOrder, DateTime now)
        where TRevision : class, IDbSheetRevision, new()
    {
        return new TRevision
        {
            SheetId = sheetId,
            RevisionNumber = 1,
            Status = RevisionStatus.Draft,
            DisplayOrder = displayOrder,
            AuthorUserId = _currentUser.UserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
    }
}
