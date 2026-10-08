namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Settles the takeover requests that nobody is going to answer. Nothing here runs as a user: it is
/// called on a timer and after changes to a sheet.
/// </summary>
public interface IRowTakeoverSettler
{
    /// <summary>Grants every request whose holder has not answered in time.</summary>
    Task GrantOverdueAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Closes the requests on a sheet whose row is no longer checked out to the holder, because they
    /// published or discarded it while the request was waiting.
    /// </summary>
    Task ReleaseSettledAsync(int sheetId, CancellationToken cancellationToken);
}
