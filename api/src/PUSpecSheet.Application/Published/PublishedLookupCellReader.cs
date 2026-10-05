using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Reads the cells that belong to a lookup match, in one query, each joined to its row's revision at the
/// version. A match in a column block reads that block's cells, the rows' own cells and the header, and
/// leaves the table's other blocks alone. A match in a row's own cell reads its sections' rows and the header.
/// </summary>
public sealed class PublishedLookupCellReader(PuSpecSheetDbContext db)
{
    /// <param name="version">The version of the sheet the match is on.</param>
    /// <param name="hit">The cell that was found.</param>
    /// <param name="sectionIds">The sections whose rows belong to the match, or null for the whole table.</param>
    public IQueryable<PublishedLookupCellRecord> Query(
        ResolvedSheetVersion version,
        PublishedLookupHit hit,
        IReadOnlyList<int>? sectionIds)
    {
        var tableId = hit.TableId;
        var cells = db.SheetCells
            .AsNoTracking()
            .Where(cell => cell.SheetRow.SheetSection.SheetTableId == tableId);

        if (hit.ColumnBlockId is { } blockId)
        {
            // The whole header comes too, so the block is named the same way as when rows are read by id.
            cells = cells.Where(cell => cell.SheetColumnBlockId == null
                || cell.SheetColumnBlockId == blockId
                || cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header);
        }
        else if (sectionIds is not null)
        {
            // The header comes too: it holds the headings and the cells that say which part a block is.
            cells = cells.Where(cell => sectionIds.Contains(cell.SheetRow.SheetSectionId)
                || cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header);
        }

        return Project(cells, version.PublishedAtUtc);
    }

    /// <summary>Every cell of the sheet's rows at the version, headings included.</summary>
    public IQueryable<PublishedLookupCellRecord> QuerySheet(ResolvedSheetVersion version)
    {
        var sheetId = version.SheetId;
        var cells = db.SheetCells
            .AsNoTracking()
            .Where(cell => cell.SheetRow.SheetSection.SheetTable.SheetId == sheetId);

        return Project(cells, version.PublishedAtUtc);
    }

    /// <summary>The cells of some rows of the sheet, found by the rows' public identifiers.</summary>
    public IQueryable<PublishedLookupCellRecord> QueryRows(ResolvedSheetVersion version, IReadOnlyList<Guid> rowPublicIds)
    {
        var sheetId = version.SheetId;
        var cells = db.SheetCells
            .AsNoTracking()
            .Where(cell => rowPublicIds.Contains(cell.SheetRow.PublicId)
                && cell.SheetRow.SheetSection.SheetTable.SheetId == sheetId);

        return Project(cells, version.PublishedAtUtc);
    }

    /// <summary>The cells of some tables' headers: their headings, and the cells that name each column block.</summary>
    public IQueryable<PublishedLookupCellRecord> QueryHeaders(ResolvedSheetVersion version, IReadOnlyList<int> tableIds)
    {
        var cells = db.SheetCells
            .AsNoTracking()
            .Where(cell => tableIds.Contains(cell.SheetRow.SheetSection.SheetTableId)
                && cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header);

        return Project(cells, version.PublishedAtUtc);
    }

    private static IQueryable<PublishedLookupCellRecord> Project(IQueryable<SheetCell> cells, DateTime moment)
    {
        return cells.SelectMany(
            cell => cell.SheetRow.Revisions.Where(revision => revision.Status == RevisionStatus.Published
                && revision.PublishedAtUtc <= moment
                && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > moment)
                && !revision.IsDeleted),
            (cell, revision) => new PublishedLookupCellRecord(
                revision.Id,
                cell.SheetRowId,
                cell.SheetRow.PublicId,
                cell.SheetRow.SheetSectionId,
                revision.DisplayOrder,
                cell.Id,
                cell.PublicId,
                cell.SheetColumnBlockId,
                cell.TemplateCell.Column,
                cell.TemplateCell.CellType.Kind,
                cell.TemplateCell.Caption,
                cell.TemplateCell.LookupKey,
                cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header));
    }
}
