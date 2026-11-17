using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Published.Lookups;
using PUSpecSheet.DataAccess.Published.Sheets;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.DataAccess.Published;

public interface IPublishedValueDataService
{
    Task<Dictionary<int, object>> ReadAsync(
        IReadOnlyList<PublishedCellRecord> cells,
        ResolvedSheetVersion? wholeSheet,
        CancellationToken cancellationToken);

    RowRevisionScope RevisionsOf(IEnumerable<int> found, ResolvedSheetVersion? wholeSheet);

    IQueryable<TValue> Scope<TValue>(
        IQueryable<TValue> values,
        RowRevisionScope revisions,
        List<int>? cellIds)
        where TValue : class, IDbCellValue;

    Task ReadTextsAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken);

    Task ReadNumbersAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken);

    Task ReadDatesAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken);

    Task ReadBooleansAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken);

    Task ReadOptionsAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken);

    Task ReadLabelledTextsAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken);

    Task<Dictionary<int, object>> ReadAsync(
        IReadOnlyList<PublishedLookupCellRecord> cells,
        ResolvedSheetVersion? wholeSheet,
        CancellationToken cancellationToken);
}
