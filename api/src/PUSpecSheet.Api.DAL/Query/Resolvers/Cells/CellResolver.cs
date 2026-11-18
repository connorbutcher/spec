using PUSpecSheet.Abstractions.Domain.Sheets;

namespace PUSpecSheet.Api.DAL.Query.Resolvers.Cells;

internal abstract class CellResolver<T, R> where T : SheetCellValue
{
    protected readonly LinkedSheetLoader _linkedSheetLoader;

    public CellResolver(LinkedSheetLoader linkedSheetLoader)
    {
        _linkedSheetLoader = linkedSheetLoader;
    }

    public abstract Task<R?> ResolveAsync(Sheet sheet, T data, CancellationToken cancellationToken);
}
