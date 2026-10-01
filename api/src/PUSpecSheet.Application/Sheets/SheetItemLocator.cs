using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Looks a sheet item up by its public identifier. Items keep that identifier through every revision, so
/// the same value finds the item in the live sheet, in any version and at any date; only an item that is
/// deleted and created again gets a new one.
/// </summary>
public sealed class SheetItemLocator(PuSpecSheetDbContext db) : ISheetItemLocator
{
    public async Task<SheetItemReferenceDto> FindAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var sheet = await db.Sheets
            .AsNoTracking()
            .Where(candidate => candidate.PublicId == publicId)
            .Select(candidate => new SheetItemReferenceDto(
                SheetItemKind.Sheet,
                publicId,
                candidate.Id,
                candidate.Id,
                candidate.PublicId,
                candidate.PhaseId,
                candidate.SheetTypeId,
                null,
                null,
                null,
                false))
            .SingleOrDefaultAsync(cancellationToken);
        if (sheet is not null)
        {
            return sheet;
        }

        var table = await db.SheetTables
            .AsNoTracking()
            .Where(candidate => candidate.PublicId == publicId)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.Sheet,
                Deleted = candidate.Revisions.Any(revision => revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null && revision.IsDeleted),
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (table is not null)
        {
            return Reference(SheetItemKind.Table, publicId, table.Id, table.Sheet, table.Id, null, null, table.Deleted);
        }

        var section = await db.SheetSections
            .AsNoTracking()
            .Where(candidate => candidate.PublicId == publicId)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.SheetTableId,
                candidate.SheetTable.Sheet,
                Deleted = candidate.Revisions.Any(revision => revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null && revision.IsDeleted),
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (section is not null)
        {
            return Reference(SheetItemKind.Section, publicId, section.Id, section.Sheet, section.SheetTableId, section.Id, null, section.Deleted);
        }

        var row = await db.SheetRows
            .AsNoTracking()
            .Where(candidate => candidate.PublicId == publicId)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.SheetSectionId,
                TableId = candidate.SheetSection.SheetTableId,
                candidate.SheetSection.SheetTable.Sheet,
                Deleted = candidate.Revisions.Any(revision => revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null && revision.IsDeleted),
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (row is not null)
        {
            return Reference(SheetItemKind.Row, publicId, row.Id, row.Sheet, row.TableId, row.SheetSectionId, row.Id, row.Deleted);
        }

        var cell = await db.SheetCells
            .AsNoTracking()
            .Where(candidate => candidate.PublicId == publicId)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.SheetRowId,
                SectionId = candidate.SheetRow.SheetSectionId,
                TableId = candidate.SheetRow.SheetSection.SheetTableId,
                candidate.SheetRow.SheetSection.SheetTable.Sheet,
                Deleted = candidate.SheetRow.Revisions.Any(revision => revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null && revision.IsDeleted),
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Nothing on a sheet has the identifier {publicId}.");
        return Reference(SheetItemKind.Cell, publicId, cell.Id, cell.Sheet, cell.TableId, cell.SectionId, cell.SheetRowId, cell.Deleted);
    }

    private static SheetItemReferenceDto Reference(
        SheetItemKind kind,
        Guid publicId,
        int id,
        Sheet sheet,
        int? tableId,
        int? sectionId,
        int? rowId,
        bool isDeleted)
    {
        return new SheetItemReferenceDto(kind, publicId, id, sheet.Id, sheet.PublicId, sheet.PhaseId, sheet.SheetTypeId, tableId, sectionId, rowId, isDeleted);
    }
}
