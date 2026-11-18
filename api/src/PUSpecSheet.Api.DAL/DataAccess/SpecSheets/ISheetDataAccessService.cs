using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Requests.Sheets;
using PUSpecSheet.Abstractions.Types;

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
    /// The list of versions of the sheet of the phase and type: every published version, oldest first, then the draft when
    /// the sheet has one.
    /// </summary>
    Task<IReadOnlyList<SheetListItem>> GetSheetListAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>
    /// Every published version of the sheets of the type, as rows of a list.
    /// </summary>
    Task<IReadOnlyList<SheetListItem>> GetPublishedSheetListAsync(int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>
    /// The published sheet as it stood at the version (the latest when none is given), with the submissions that published
    /// it. Null when the sheet has no such version. Tables the current user has no permission to are left out.
    /// </summary>
    Task<SheetDetail?> GetPublishedSheetDetailAsync(int sheetId, int? versionNumber, CancellationToken cancellationToken);

    /// <summary>
    /// The sheet as it stands now with its drafts, with all its submissions. Null when there is no such sheet. Tables the
    /// current user has no permission to are left out.
    /// </summary>
    Task<SheetDetail?> GetDraftSheetDetailAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>
    /// Where the spec sheet with a SpecSheets id is among the sheets. Null when it isn't: it hasn't been migrated to a
    /// version, or it is an unpublished one with no sheet.
    /// </summary>
    Task<SheetLocation?> FindSheetAsync(int specSheetId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates the sheet for a phase and sheet type that has none, its drafts checked out to the current user.
    /// </summary>
    Task<SheetLocation> CreateDraftAsync(CreateSheetRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Applies the changes to the sheet's drafts, checking out each item changed to the current user. Items checked out to
    /// someone else or awaiting approval are refused.
    /// </summary>
    Task ApplyChangesAsync(int sheetId, SheetChanges changes, CancellationToken cancellationToken);

    /// <summary>
    /// Submits the current user's checked-out changes: publishes them as the next version, or sends them for approval
    /// when the sheet type needs it.
    /// </summary>
    Task PublishAsync(int sheetId, PublishSheetRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Undoes every change the current user has checked out on the sheet that isn't awaiting approval, which frees those
    /// items for anyone to edit.
    /// </summary>
    Task UndoMyChangesAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>
    /// Undoes the current user's checked-out changes to one table, group or row of the sheet and to what is inside it.
    /// </summary>
    Task UndoMyItemChangesAsync(int sheetId, SheetItemKind kind, int itemId, CancellationToken cancellationToken);

    /// <summary>
    /// Throws away every user's checked-out changes to the sheet that aren't awaiting approval (an admin's way to clear
    /// its checkouts). Published versions can't be deleted.
    /// </summary>
    Task DeleteDraftAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>
    /// Records the current user's decision on one of the sheet's pending submissions. A rejection hands its changes back
    /// to their author; the last approval publishes them as the next version.
    /// </summary>
    Task DecideSubmissionAsync(int sheetId, int submissionId, DecideSubmissionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Takes back the current user's pending submission: its changes go back to being checked out to them.
    /// </summary>
    Task WithdrawSubmissionAsync(int sheetId, int submissionId, CancellationToken cancellationToken);

    /// <summary>
    /// Changes who may approve one of the sheet's pending submissions.
    /// </summary>
    Task ReassignSubmissionApproversAsync(int sheetId, int submissionId, ReassignSubmissionApproversRequest request, CancellationToken cancellationToken);
}
