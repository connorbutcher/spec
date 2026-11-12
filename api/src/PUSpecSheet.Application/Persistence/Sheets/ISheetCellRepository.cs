using PUSpecSheet.Application.Sheets.Values;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The cells of sheet rows.</summary>
public interface ISheetCellRepository
{
    /// <summary>The cells of the rows, with what each copies and its kind.</summary>
    Task<List<NewCell>> ListNewCellsAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken);

    /// <summary>The cells of the rows that must be filled in.</summary>
    Task<List<RequiredCell>> ListRequiredAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken);
}
