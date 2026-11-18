using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// Reads a whole spec sheet, from where <see cref="SheetReadOptions.Source"/> says, or a draft of a sheet type edited on
/// the revisions from its drafts.
/// </summary>
internal interface ISpecSheetReader
{
    Task<SpecSheet> ReadAsync(int id, CancellationToken cancellationToken);
}
