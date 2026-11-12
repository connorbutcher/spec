using PUSpecSheet.Application.Published.Lookups;
using PUSpecSheet.Application.Published.Queries;
using PUSpecSheet.Application.Published.Versions;
using PUSpecSheet.Contracts.Published;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Published.Rows;

public sealed class PublishedRowsService : IPublishedRowsService
{
    private readonly PuSpecSheetDbContext db;
    private readonly PublishedRowsCache cache;

    public PublishedRowsService(
        PuSpecSheetDbContext db,
        PublishedRowsCache cache)
    {
        this.db = db;
        this.cache = cache;
    }

    public async Task<PublishedRowsDto> ReadAsync(ResolvedSheetVersion version, PublishedRowsSelection selection, CancellationToken cancellationToken)
    {
        var key = $"{version.SheetPublicId:N}-v{version.VersionNumber}-{selection.Key}";
        if (cache.TryGet(key, out var cached))
        {
            return cached;
        }

        var structure = await PublishedStructureQueries.ReadAsync(db, version, cancellationToken);
        PublishedRowsDto rows;
        int cellCount;

        if (selection.IsEverything)
        {
            var all = await PublishedLookupCellQueries.ListOnSheetAsync(db, version, cancellationToken);
            var allValues = await PublishedKindValueQueries.ReadAsync(db, all, wholeSheet: version, cancellationToken);
            rows = PublishedRowsAssembler.Assemble(version, selection, structure, all, all, allValues);
            cellCount = all.Count;
        }
        else
        {
            var cells = await PublishedLookupCellQueries.ListInRowsAsync(db, version, [.. selection.Rows], cancellationToken);
            var headers = await HeadersAsync(version, structure, cells, cancellationToken);
            var read = cells
                .Concat(headers.Where(cell => cell.ColumnBlockId is not null))
                .DistinctBy(cell => cell.CellId)
                .ToList();
            var values = await PublishedKindValueQueries.ReadAsync(db, read, wholeSheet: null, cancellationToken);
            rows = PublishedRowsAssembler.Assemble(version, selection, structure, cells, headers, values);
            cellCount = read.Count;
        }

        cache.Set(key, rows, cellCount);
        return rows;
    }

    /// <summary>
    /// The header cells of the tables the rows are in. Only a table with repeated columns needs its header
    /// read, to name each set of columns.
    /// </summary>
    private async Task<List<PublishedLookupCellRecord>> HeadersAsync(
        ResolvedSheetVersion version,
        PublishedStructure structure,
        List<PublishedLookupCellRecord> cells,
        CancellationToken cancellationToken)
    {
        var sectionTables = structure.Sections.ToDictionary(section => section.Id, section => section.TableId);
        var tableIds = cells
            .Where(cell => cell.ColumnBlockId is not null && sectionTables.ContainsKey(cell.SectionId))
            .Select(cell => sectionTables[cell.SectionId])
            .Distinct()
            .ToList();

        return tableIds.Count == 0
            ? []
            : await PublishedLookupCellQueries.ListHeadersAsync(db, version, tableIds, cancellationToken);
    }
}
