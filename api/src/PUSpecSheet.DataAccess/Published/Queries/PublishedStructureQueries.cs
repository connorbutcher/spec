using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Published.Queries;

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
            .Select(revision => new PublishedTableRecord
            {
                Id = revision.SheetTableId,
                PublicId = revision.SheetTable.PublicId,
                DisplayOrder = revision.DisplayOrder,
                IsDeleted = revision.IsDeleted,
                Title = revision.Title,
            });
    }

    public static IQueryable<PublishedSectionRecord> Sections(PuSpecSheetDbContext db, ResolvedSheetVersion version)
    {
        return db.SheetSectionRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == version.SheetId)
            .PublishedAt(version.PublishedAtUtc)
            .Select(revision => new PublishedSectionRecord
            {
                Id = revision.SheetSectionId,
                PublicId = revision.SheetSection.PublicId,
                TableId = revision.SheetSection.SheetTableId,
                ParentSectionId = revision.SheetSection.ParentSheetSectionId,
                DisplayOrder = revision.DisplayOrder,
                IsDeleted = revision.IsDeleted,
                Name = revision.SheetSection.TemplateSection.Name,
            });
    }

    public static IQueryable<PublishedColumnBlockRecord> ColumnBlocks(PuSpecSheetDbContext db, ResolvedSheetVersion version)
    {
        return db.SheetColumnBlockRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == version.SheetId)
            .PublishedAt(version.PublishedAtUtc)
            .Select(revision => new PublishedColumnBlockRecord
            {
                Id = revision.SheetColumnBlockId,
                PublicId = revision.SheetColumnBlock.PublicId,
                TableId = revision.SheetColumnBlock.SheetTableId,
                DisplayOrder = revision.DisplayOrder,
                IsDeleted = revision.IsDeleted,
                Name = revision.SheetColumnBlock.TemplateColumnBlock.Name,
            });
    }
}
