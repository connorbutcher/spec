using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Changes to the rows of a sheet. A row is the unit that's locked: saving values, moving or removing a
/// row starts the current user's draft on it, which locks it to them until they publish or discard.
/// Every action returns the refreshed live view of the sheet.
/// </summary>
public interface ISheetRowService
{
    /// <summary>Adds another row built from one of the section's template rows. Not allowed in the header.</summary>
    Task<SheetDto> AddAsync(int sectionId, AddSheetRowRequest request, CancellationToken cancellationToken);

    /// <summary>Locks the row to the current user without changing it yet.</summary>
    Task<SheetDto> LockAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>Sets or clears cell values in the row, locking it to the current user.</summary>
    Task<SheetDto> SaveValuesAsync(int rowId, SaveRowValuesRequest request, CancellationToken cancellationToken);

    Task<SheetDto> MoveAsync(int rowId, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Removes a row. A row nobody has published is simply deleted. Not allowed in the header.</summary>
    Task<SheetDto> RemoveAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>Throws away the current user's draft on the row, releasing its lock.</summary>
    Task<SheetDto> DiscardAsync(int rowId, CancellationToken cancellationToken);
}
