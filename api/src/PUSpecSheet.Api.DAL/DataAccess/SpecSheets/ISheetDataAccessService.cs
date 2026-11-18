using PUSpecSheet.Abstractions.Domain.Sheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

/// <summary>
/// Reads sheets in the shape of the revision schema. These are read from the revisions alone, whatever
/// <c>SheetReads:Source</c> says, and have the ids of the revision schema's items rather than the copies'.
/// </summary>
public interface ISheetDataAccessService
{
    /// <summary>
    /// The sheet as it stood when one of its versions was published: the latest version when none is given. Null when
    /// the sheet has no such version. Tables the current user has no permission to are left out.
    /// </summary>
    Task<Sheet?> GetPublishedSheetAsync(int sheetId, int? versionNumber, CancellationToken cancellationToken);

    /// <summary>
    /// The sheet as it stands now with its drafts laid over its published revisions. Null when there is no such sheet.
    /// Tables the current user has no permission to are left out.
    /// </summary>
    Task<Sheet?> GetDraftSheetAsync(int sheetId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SheetVersion>> GetSheetVersionsAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>
    /// The sheet's submissions and who approved them, oldest first: only those that published the version when one is
    /// given. Null when the sheet has no such version.
    /// </summary>
    Task<IReadOnlyList<SheetSubmission>?> GetSheetSubmissionsAsync(int sheetId, int? versionNumber, CancellationToken cancellationToken);

    /// <summary>
    /// The sheet for the phase and sheet type as it stood at its latest version published at or before the moment: its
    /// latest version when no moment is given. Null when there is no such sheet or version.
    /// </summary>
    Task<Sheet?> GetLatestPublishedSheetByPhaseAndTypeAsync(int phaseId, int sheetTypeId, DateTime? publishedBy, CancellationToken cancellationToken);

    /// <summary>
    /// The sheets of the phase, and of its child phases when asked, without their tables.
    /// </summary>
    Task<IReadOnlyList<SheetSummary>> GetSheetSummariesByPhaseAsync(int phaseId, bool includeChildren, CancellationToken cancellationToken);

    Task<IReadOnlyList<SheetSummary>> GetSheetSummariesByTypeAsync(int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>
    /// The sheet for the phase and sheet type without its tables. Null when there is none.
    /// </summary>
    Task<SheetSummary?> GetSheetSummaryByPhaseAndTypeAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>
    /// Where the spec sheet with a SpecSheets id is among the sheets. Null when it isn't: it hasn't been migrated to a
    /// version, or it is an unpublished one whose type isn't edited on the revisions.
    /// </summary>
    Task<SheetLocation?> FindSheetAsync(int specSheetId, CancellationToken cancellationToken);
}
