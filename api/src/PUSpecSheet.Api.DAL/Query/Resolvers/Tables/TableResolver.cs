using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Domain.Sheets;

namespace PUSpecSheet.Api.DAL.Query.Resolvers.Tables;

internal abstract class TableResolver<T, R> where T : ITableProperties
{
    protected readonly LinkedSheetLoader _linkedSheetLoader;

    public TableResolver(LinkedSheetLoader linkedSheetLoader)
    {
        _linkedSheetLoader = linkedSheetLoader;
    }

    public abstract Task<R?> ResolveAsync(Sheet sheet, T data, CancellationToken cancellationToken);
}
