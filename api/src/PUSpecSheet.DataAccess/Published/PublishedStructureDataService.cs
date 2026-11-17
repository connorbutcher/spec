using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Published.Queries;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Published;

public sealed class PublishedStructureDataService : IPublishedStructureDataService
{
    private readonly PuSpecSheetDbContext _db;

    public PublishedStructureDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<PublishedStructure> ReadAsync(ResolvedSheetVersion version, CancellationToken cancellationToken)
    {
        var tables = await Tables(version).ToListAsync(cancellationToken);
        var sections = await Sections(version).ToListAsync(cancellationToken);
        var columnBlocks = await ColumnBlocks(version).ToListAsync(cancellationToken);

        return new PublishedStructure(tables, sections, columnBlocks);
    }

    public IQueryable<PublishedTableRecord> Tables(ResolvedSheetVersion version)
    {
        return _db.SheetTableRevisions
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

    public IQueryable<PublishedSectionRecord> Sections(ResolvedSheetVersion version)
    {
        return _db.SheetSectionRevisions
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

    public IQueryable<PublishedColumnBlockRecord> ColumnBlocks(ResolvedSheetVersion version)
    {
        return _db.SheetColumnBlockRevisions
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
