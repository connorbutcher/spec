using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets;

public interface ISheetItemLocator
{
    /// <summary>
    /// Finds the sheet, table, section, row or cell with this public identifier, and says whether its
    /// latest published revision removes it. Fails if nothing has the identifier.
    /// </summary>
    Task<SheetItemReferenceDto> FindAsync(Guid publicId, CancellationToken cancellationToken);
}
