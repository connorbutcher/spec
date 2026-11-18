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
}
