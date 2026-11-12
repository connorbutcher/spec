using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Published.Versions;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Published.Queries;

/// <summary>
/// Reads a sheet's tables, sections and column blocks as they stood at a version: three small queries
/// that return only the columns needed to place each item and tell whether it had been removed.
/// </summary>
public static class PublishedStructureQueries
{
    public static async Task<PublishedStructure> ReadAsync(PuSpecSheetDbContext db, ResolvedSheetVersion version, CancellationToken cancellationToken)
    {
        var tables = await Tables(db, version).ToListAsync(cancellationToken);
        var sections = await Sections(db, version).ToListAsync(cancellationToken);
        var columnBlocks = await ColumnBlocks(db, version).ToListAsync(cancellationToken);

        return new PublishedStructure(tables, sections, columnBlocks);
    }

    public static IQueryable<PublishedTableRecord> Tables(PuSpecSheetDbContext db, ResolvedSheetVersion version)
    {
        return db.SheetTableRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == version.SheetId)
            .PublishedAt(version.PublishedAtUtc)
            .Select(revision => new PublishedTableRecord(
                revision.SheetTableId,
                revision.SheetTable.PublicId,
                revision.DisplayOrder,
                revision.IsDeleted,
                revision.Title));
    }

    public static IQueryable<PublishedSectionRecord> Sections(PuSpecSheetDbContext db, ResolvedSheetVersion version)
    {
        return db.SheetSectionRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == version.SheetId)
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

    public static IQueryable<PublishedColumnBlockRecord> ColumnBlocks(PuSpecSheetDbContext db, ResolvedSheetVersion version)
    {
        return db.SheetColumnBlockRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == version.SheetId)
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
