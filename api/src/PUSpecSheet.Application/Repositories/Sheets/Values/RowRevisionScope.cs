using PUSpecSheet.Database.Entities.Values;

namespace PUSpecSheet.Application.Repositories.Sheets.Values;

/// <summary>
/// Which row revisions to read values for: either a list of ids, or a query over the revisions that SQL
/// Server runs as part of each value query.
/// </summary>
/// <remarks>
/// A whole sheet's revisions should be given as a query. As a list, up to 2,100 ids are sent as as many
/// parameters, which costs 30 to 50 ms a value table at 2,000 rows; above that they go as one JSON
/// parameter, which SQL Server can't estimate, and it then reads the JSON again for every row of a small
/// value table. A query over the sheet's revisions is found through the (sheet, status) and (sheet,
/// published) indexes and costs a few milliseconds whatever the sheet's size.
/// </remarks>
public sealed class RowRevisionScope
{
    private readonly IReadOnlyCollection<int>? ids;

    private readonly IQueryable<int>? query;

    private RowRevisionScope(IReadOnlyCollection<int>? ids, IQueryable<int>? query)
    {
        this.ids = ids;
        this.query = query;
    }

    /// <summary>True when there is certainly nothing to read.</summary>
    public bool IsEmpty => ids is { Count: 0 };

    public static RowRevisionScope Of(IReadOnlyCollection<int> ids)
    {
        return new RowRevisionScope(ids, null);
    }

    public static RowRevisionScope Of(IQueryable<int> query)
    {
        return new RowRevisionScope(null, query);
    }

    /// <summary>The values held by these revisions.</summary>
    public IQueryable<TValue> Apply<TValue>(IQueryable<TValue> values)
        where TValue : class, ICellValue
    {
        if (query is not null)
        {
            var revisionIds = query;
            return values.Where(value => revisionIds.Contains(value.SheetRowRevisionId));
        }

        var listed = ids!;
        return values.Where(value => listed.Contains(value.SheetRowRevisionId));
    }
}
