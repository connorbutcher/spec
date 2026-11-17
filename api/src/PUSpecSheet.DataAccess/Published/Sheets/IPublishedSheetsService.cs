using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published.Sheets;

/// <summary>
/// Read-only access to published sheet data for other applications. It never returns drafts, locks,
/// templates or anything that depends on who is asking.
/// </summary>
public interface IPublishedSheetsService
{
    /// <summary>
    /// The one sheet of a sheet type for a phase, found by the phase's code and narrowed by identifier values
    /// (by identifier key). Fails when no sheet, or more than one, matches.
    /// </summary>
    Task<PublishedSheetReferenceDto> FindAsync(string phaseCode, int sheetTypeId, IReadOnlyDictionary<string, string> identifiers, CancellationToken cancellationToken);

    /// <summary>Every sheet of a sheet type for a phase that has the identifier values given (by identifier key), oldest first.</summary>
    Task<IReadOnlyList<PublishedSheetReferenceDto>> ListAsync(string phaseCode, int sheetTypeId, IReadOnlyDictionary<string, string> identifiers, CancellationToken cancellationToken);

    /// <summary>The sheet's published versions, oldest first.</summary>
    Task<IReadOnlyList<PublishedVersionDto>> VersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken);

    /// <summary>
    /// Finds the version a caller means, in one small query. Callers do this first so an unchanged answer
    /// can be recognised before any data is read.
    /// </summary>
    Task<ResolvedSheetVersion> ResolveAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken);

    /// <summary>The selected values of the sheet at a resolved version.</summary>
    Task<PublishedSheetDto> ReadAsync(ResolvedSheetVersion version, PublishedSheetSelection selection, CancellationToken cancellationToken);
}
