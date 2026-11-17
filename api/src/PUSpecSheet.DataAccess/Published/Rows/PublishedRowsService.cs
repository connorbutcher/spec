using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Published.Lookups;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published.Rows;

public sealed class PublishedRowsService : IPublishedRowsService
{
    private readonly PublishedRowsCache _cache;
    private readonly PublishedVersionResolver _resolver;
    private readonly IPublishedValueDataService _publishedValueData;
    private readonly IPublishedLookupCellDataService _publishedLookupCellData;
    private readonly IPublishedStructureDataService _publishedStructureData;

    public PublishedRowsService(
        PublishedRowsCache cache,
        PublishedVersionResolver resolver,
        IPublishedValueDataService publishedValueData,
        IPublishedLookupCellDataService publishedLookupCellData,
        IPublishedStructureDataService publishedStructureData)
    {
        _cache = cache;
        _resolver = resolver;
        _publishedValueData = publishedValueData;
        _publishedLookupCellData = publishedLookupCellData;
        _publishedStructureData = publishedStructureData;
    }

    public async Task<PublishedRows> ReadAsync(ResolvedSheetVersion version, PublishedRowsSelection selection, CancellationToken cancellationToken)
    {
        var key = version.KeyFor(selection);
        if (_cache.TryGet(key, out var cached))
        {
            return cached;
        }

        var structure = await _publishedStructureData.ReadAsync(version, cancellationToken);
        PublishedRows rows;
        int cellCount;

        if (selection.IsEverything)
        {
            var all = await _publishedLookupCellData.ListOnSheetAsync(version, cancellationToken);
            var allValues = await _publishedValueData.ReadAsync(all, wholeSheet: version, cancellationToken);
            rows = PublishedRowsAssembler.Assemble(version, selection, structure, all, all, allValues);
            cellCount = all.Count;
        }
        else
        {
            var cells = await _publishedLookupCellData.ListInRowsAsync(version, [.. selection.Rows], cancellationToken);
            var headers = await HeadersAsync(version, structure, cells, cancellationToken);
            var read = cells
                .Concat(headers.Where(cell => cell.ColumnBlockId is not null))
                .DistinctBy(cell => cell.CellId)
                .ToList();
            var values = await _publishedValueData.ReadAsync(read, wholeSheet: null, cancellationToken);
            rows = PublishedRowsAssembler.Assemble(version, selection, structure, cells, headers, values);
            cellCount = read.Count;
        }

        _cache.Set(key, rows, cellCount);
        return rows;
    }

    public async Task<PublishedSheetRow> ReadRowAsync(ResolvedSheetVersion version, PublishedRowsSelection selection, CancellationToken cancellationToken)
    {
        var rowPublicId = selection.Rows.Single();
        var result = await ReadAsync(version, selection, cancellationToken);
        if (!result.Rows.TryGetValue(rowPublicId, out var row))
        {
            throw new NotFoundException($"Version {version.VersionNumber} of sheet {version.SheetPublicId} has no row {rowPublicId}.");
        }

        return new PublishedSheetRow
        {
            Sheet = result.Sheet,
            Version = result.Version,
            PublishedAtUtc = result.PublishedAtUtc,
            Row = rowPublicId,
            Section = row.Section,
            Values = row.Values,
            Columns = row.Columns,
        };
    }

    public Task<ResolvedSheetVersion> ResolveAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken)
    {
        return _resolver.ResolveAsync(sheetPublicId, point, cancellationToken);
    }

    // Only a table with column blocks needs its header read, to name each set of columns.
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
            : await _publishedLookupCellData.ListHeadersAsync(version, tableIds, cancellationToken);
    }
}
