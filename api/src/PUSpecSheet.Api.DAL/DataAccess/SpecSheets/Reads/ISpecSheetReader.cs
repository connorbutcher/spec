using PUSpecSheet.Abstractions.Domain.SpecSheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// Reads a whole spec sheet, from where <see cref="SheetReadOptions.Source"/> says.
/// </summary>
internal interface ISpecSheetReader
{
    Task<SpecSheet> ReadAsync(int id, CancellationToken cancellationToken);
}
