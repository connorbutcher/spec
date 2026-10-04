using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Reads a sheet's tables, sections and column blocks as they stood at a version: three small queries
/// that return only the columns needed to place each item and tell whether it had been removed.
/// </summary>
public sealed class PublishedStructureReader(PuSpecSheetDbContext db)
{
    public async Task<PublishedStructure> ReadAsync(ResolvedSheetVersion version, CancellationToken cancellationToken)
    {
        var tables = await Tables(version).ToListAsync(cancellationToken);
        var sections = await Sections(version).ToListAsync(cancellationToken);
        var columnBlocks = await ColumnBlocks(version).ToListAsync(cancellationToken);

        return new PublishedStructure(tables, sections, columnBlocks);
    }

    public IQueryable<PublishedTableRecord> Tables(ResolvedSheetVersion version)
    {
        return db.SheetTableRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetTable.SheetId == version.SheetId)
            .PublishedAt(version.PublishedAtUtc)
            .Select(revision => new PublishedTableRecord(
                revision.SheetTableId,
                revision.SheetTable.PublicId,
                revision.DisplayOrder,
                revision.IsDeleted,
                revision.Title));
    }

    public IQueryable<PublishedSectionRecord> Sections(ResolvedSheetVersion version)
    {
        return db.SheetSectionRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetSection.SheetTable.SheetId == version.SheetId)
            .PublishedAt(version.PublishedAtUtc)
            .Select(revision => new PublishedSectionRecord(
                revision.SheetSectionId,
                revision.SheetSection.PublicId,
                revision.SheetSection.SheetTableId,
                revision.SheetSection.ParentSheetSectionId,
                revision.DisplayOrder,
                revision.IsDeleted,
                revision.SheetSection.TemplateSection.Name));
    }

    public IQueryable<PublishedColumnBlockRecord> ColumnBlocks(ResolvedSheetVersion version)
    {
        return db.SheetColumnBlockRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetColumnBlock.SheetTable.SheetId == version.SheetId)
            .PublishedAt(version.PublishedAtUtc)
            .Select(revision => new PublishedColumnBlockRecord(
                revision.SheetColumnBlockId,
                revision.SheetColumnBlock.PublicId,
                revision.SheetColumnBlock.SheetTableId,
                revision.DisplayOrder,
                revision.IsDeleted,
                revision.SheetColumnBlock.TemplateColumnBlock.Name));
    }
}
