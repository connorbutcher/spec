using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.Application.Sheets.Catalog;

/// <summary>
/// Starts a new sheet as a copy of another: the tables, column blocks, sections, rows and values the source
/// has at its latest published version, built again on the new sheet as the current user's first drafts.
/// So the copy is theirs to change, and reaches everyone else when it is approved, like any other change.
/// Each table keeps the template version it was built from. A linked dropdown pointed at a table or row of
/// the source is pointed at the copy of it.
/// </summary>
public sealed class SheetCopier
{
    private readonly PuSpecSheetDbContext db;
    private readonly ICurrentUser currentUser;

    public SheetCopier(PuSpecSheetDbContext db, ICurrentUser currentUser)
    {
        this.db = db;
        this.currentUser = currentUser;
    }

    /// <summary>Copies the published content of one sheet onto another, empty, sheet. The caller holds the transaction.</summary>
    public async Task CopyAsync(int sourceSheetId, int targetSheetId, CancellationToken cancellationToken)
    {
        var map = new SheetCopyMap();
        var now = DateTime.UtcNow;

        var tables = await ListCurrent(db.SheetTableRevisions, sourceSheetId).Include(revision => revision.SheetTable).ToListAsync(cancellationToken);
        foreach (var revision in tables)
        {
            var table = new DbSheetTable { SheetId = targetSheetId, TableTemplateVersionId = revision.SheetTable.TableTemplateVersionId };
            var copy = Draft<DbSheetTableRevision>(targetSheetId, revision.DisplayOrder, now);
            copy.Title = revision.Title;
            table.Revisions.Add(copy);
            db.SheetTables.Add(table);
            map.Tables[revision.SheetTableId] = table;
        }

        var blocks = await ListCurrent(db.SheetColumnBlockRevisions, sourceSheetId).Include(revision => revision.SheetColumnBlock).ToListAsync(cancellationToken);
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

        var sections = await ListCurrent(db.SheetSectionRevisions, sourceSheetId).Include(revision => revision.SheetSection).ToListAsync(cancellationToken);
        CopySections(sections, map, targetSheetId, now);

        var rows = await ListCurrent(db.SheetRowRevisions, sourceSheetId).Include(revision => revision.SheetRow).ToListAsync(cancellationToken);
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

        var rowIds = ListCurrent(db.SheetRowRevisions, sourceSheetId).Select(revision => revision.SheetRowId);
        var cells = await db.SheetCells.AsNoTracking().Where(cell => rowIds.Contains(cell.SheetRowId)).ToListAsync(cancellationToken);
        foreach (var cell in cells)
        {
            CopyCell(cell, map);
        }

        var revisionIds = ListCurrent(db.SheetRowRevisions, sourceSheetId).Select(revision => revision.Id);
        await CopyValuesAsync(db.TextValues, revisionIds, map, value => new DbTextValue { Value = value.Value }, cancellationToken);
        await CopyValuesAsync(db.NumericValues, revisionIds, map, value => new DbNumericValue { Value = value.Value }, cancellationToken);
        await CopyValuesAsync(db.DateValues, revisionIds, map, value => new DbDateValue { Value = value.Value }, cancellationToken);
        await CopyValuesAsync(db.BooleanValues, revisionIds, map, value => new DbBooleanValue { Value = value.Value }, cancellationToken);
        await CopyValuesAsync(db.OptionValues, revisionIds, map, value => new DbOptionValue { CellTypeOptionId = value.CellTypeOptionId }, cancellationToken);
        await CopyValuesAsync(
            db.LabelledTextValues,
            revisionIds,
            map,
            value => new DbLabelledTextValue { SlotKey = value.SlotKey, Position = value.Position, Value = value.Value },
            cancellationToken);
        var linked = await CopyPlainSettingsAsync(revisionIds, map, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        // The tables and rows a linked dropdown reads only have ids once saved.
        foreach (var settings in linked)
        {
            AddLinkedSettings(settings, map);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<TRevision> ListCurrent<TRevision>(DbSet<TRevision> revisions, int sheetId)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.AsNoTracking().Where(revision => revision.SheetId == sheetId).Current().Where(revision => !revision.IsDeleted);
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

    /// <summary>Copies sections parents first, so each copy can be given its parent's; a section whose parent isn't copied isn't either.</summary>
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

    private static async Task CopyValuesAsync<TValue>(
        DbSet<TValue> values,
        IQueryable<int> revisionIds,
        SheetCopyMap map,
        Func<TValue, TValue> copy,
        CancellationToken cancellationToken)
        where TValue : class, IDbCellValue
    {
        var source = await values.AsNoTracking().Where(value => revisionIds.Contains(value.SheetRowRevisionId)).ToListAsync(cancellationToken);
        foreach (var value in source)
        {
            if (map.RowRevisions.TryGetValue(value.SheetRowRevisionId, out var revision) && map.Cells.TryGetValue(value.SheetCellId, out var cell))
            {
                var copied = copy(value);
                copied.SheetRowRevision = revision;
                copied.SheetCell = cell;
                values.Add(copied);
            }
        }
    }

    /// <summary>Copies the cell settings that point at nothing on the sheet, and returns the linked dropdowns' for later.</summary>
    private async Task<List<DbCellSettingsValue>> CopyPlainSettingsAsync(IQueryable<int> revisionIds, SheetCopyMap map, CancellationToken cancellationToken)
    {
        var source = await db.CellSettings.AsNoTracking().Where(value => revisionIds.Contains(value.SheetRowRevisionId)).ToListAsync(cancellationToken);
        var linked = new List<DbCellSettingsValue>();
        foreach (var value in source)
        {
            if (value.Settings is LinkedDropdownInstanceSettings)
            {
                linked.Add(value);
            }
            else if (map.RowRevisions.TryGetValue(value.SheetRowRevisionId, out var revision) && map.Cells.TryGetValue(value.SheetCellId, out var cell))
            {
                db.CellSettings.Add(new DbCellSettingsValue { SheetRowRevision = revision, SheetCell = cell, Settings = value.Settings });
            }
        }

        return linked;
    }

    /// <summary>Adds a linked dropdown's settings pointed at the copies of what it read, or nothing when that wasn't copied.</summary>
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

        db.CellSettings.Add(new DbCellSettingsValue
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
            AuthorUserId = currentUser.UserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
    }
}
