using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Api.DAL.Caching;

namespace PUSpecSheet.Api.DAL.Query.Resolvers.Tables;

internal abstract class TableResolver<T, R> where T : ITableProperties
{
    protected readonly ISpecSheetCache _loadedSpecSheetsService;

    public TableResolver(ISpecSheetCache loadedSpecSheetsService)
    {
        _loadedSpecSheetsService = loadedSpecSheetsService;
    }

    public abstract Task<R?> ResolveAsync(SpecSheet specSheet, T data, CancellationToken cancellationToken);
}
