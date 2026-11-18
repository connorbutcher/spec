using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Requests.Sheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

/// <summary>
/// Reads sheets in the shape of the revision schema. These are read from the revisions alone and have the ids of the revision schema's items.
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
    /// The cells with the reference ids, of the sheet of the phase and type as it stood at its latest version published at
    /// or before the moment (its latest version when none is given). Null when there is no such version.
    /// </summary>
    Task<SheetCells?> GetLatestPublishedCellsByPhaseAndTypeAsync(int phaseId, int sheetTypeId, IReadOnlyCollection<Guid> referenceIds, DateTime? publishedBy, CancellationToken cancellationToken);

    /// <summary>
    /// The cells with the reference ids, of the sheet as it stood at the version (its latest when none is given).
    /// </summary>
    Task<SheetCells?> GetPublishedCellsAsync(int sheetId, int? versionNumber, IReadOnlyCollection<Guid> referenceIds, CancellationToken cancellationToken);

    /// <summary>
    /// The tables that have been published, one for each reference id, that the current user has permission to.
    /// </summary>
    Task<IReadOnlyList<SheetTable>> GetUniqueTablesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Every published version of the sheets of the type (of the phase too when one is given), without their tables.
    /// </summary>
    Task<IReadOnlyList<SheetVersionSummary>> GetPublishedVersionSummariesAsync(int sheetTypeId, int? phaseId, CancellationToken cancellationToken);

    /// <summary>
    /// The sheets of the phase, and of its child phases when asked, without their tables.
    /// </summary>
    Task<IReadOnlyList<SheetSummary>> GetSheetSummariesByPhaseAsync(int phaseId, bool includeChildren, CancellationToken cancellationToken);

    /// <summary>
    /// The sheets of the type, without their tables.
    /// </summary>
    Task<IReadOnlyList<SheetSummary>> GetSheetSummariesByTypeAsync(int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>
    /// The sheet for the phase and sheet type without its tables. Null when there is none.
    /// </summary>
    Task<SheetSummary?> GetSheetSummaryByPhaseAndTypeAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>
    /// Every published version of the sheet of the phase and type, with the sheet and its draft's id.
    /// </summary>
    Task<SheetListing> GetSheetListingAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>
    /// The sheet the spec sheet is, as it is opened: the draft with its drafts, or the published version, with its
    /// submissions. Null when the spec sheet isn't one of the sheets, or has no such sheet.
    /// </summary>
    Task<SheetDetail?> GetSheetDetailAsync(int specSheetId, CancellationToken cancellationToken);

    /// <summary>
    /// Where the spec sheet with a SpecSheets id is among the sheets. Null when it isn't: it hasn't been migrated to a
    /// version, or it is an unpublished one with no sheet.
    /// </summary>
    Task<SheetLocation?> FindSheetAsync(int specSheetId, CancellationToken cancellationToken);

    /// <summary>
    /// Starts the draft for the phase and sheet type. A sheet type must have been migrated to the revisions first.
    /// </summary>
    Task<SheetLocation> CreateDraftAsync(CreateSheetRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Applies the changes to the sheet's draft.
    /// </summary>
    Task ApplyChangesAsync(int sheetId, SheetChanges changes, CancellationToken cancellationToken);

    /// <summary>
    /// Publishes the sheet's draft, or submits it for approval when its type needs it.
    /// </summary>
    Task PublishAsync(int sheetId, PublishSheetRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Throws away the sheet's draft. Published versions can't be deleted.
    /// </summary>
    Task DeleteDraftAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>
    /// Records the current user's decision on the sheet's pending submission. A rejection hands the draft back; the
    /// last approval publishes it.
    /// </summary>
    Task DecideSubmissionAsync(int sheetId, DecideSubmissionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Changes who may approve the sheet's pending submission.
    /// </summary>
    Task ReassignSubmissionApproversAsync(int sheetId, ReassignSubmissionApproversRequest request, CancellationToken cancellationToken);
}
