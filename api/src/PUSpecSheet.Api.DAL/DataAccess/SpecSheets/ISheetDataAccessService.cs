using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Requests.Sheets;

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

    /// <summary>
    /// Starts the draft for the phase and sheet type. Only for sheet types edited on the revisions
    /// (<c>SheetEdits:RevisionSheetTypeIds</c>), as are the other writes: a type still edited on its copies has no draft
    /// to change here, and a <see cref="NotSupportedException"/> says so.
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
