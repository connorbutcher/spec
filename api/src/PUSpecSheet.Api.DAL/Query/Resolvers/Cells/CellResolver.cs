using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Abstractions.Domain.Legacy.Values;
using PUSpecSheet.Api.DAL.Caching;

namespace PUSpecSheet.Api.DAL.Query.Resolvers.Cells;

internal abstract class CellResolver<T, R> where T : CellValue
{
    protected readonly ISpecSheetCache _loadedSpecSheetsService;

    public CellResolver(ISpecSheetCache loadedSpecSheetsService)
    {
        _loadedSpecSheetsService = loadedSpecSheetsService;
    }

    public abstract Task<R?> ResolveAsync(SpecSheet specSheet, T data, CancellationToken cancellationToken);
}
