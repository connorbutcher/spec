using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Read-only access to published sheet data for other applications. It never returns drafts, locks,
/// templates or anything that depends on who is asking.
/// </summary>
public interface IPublishedSheetQueryService
{
    /// <summary>The sheet of a sheet type for a phase, found by the phase's code.</summary>
    Task<PublishedSheetReferenceDto> FindAsync(string phaseCode, int sheetTypeId, CancellationToken cancellationToken);

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
