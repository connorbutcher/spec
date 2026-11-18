namespace PUSpecSheet.CheckInOut.Api.Models;

/// <summary>
/// Tells the others viewing a sheet that a user changed what is checked out on it: saved, undid, submitted or decided
/// changes. Checkouts are kept with the sheet's drafts, so the others read the sheet again to see them.
/// </summary>
public record SheetChangedNotice
{
    public required int SheetId { get; init; }

    public required ConnectedUser User { get; init; }

    /// <summary>
    /// What the user did, in a few words to show (for example "saved changes").
    /// </summary>
    public required string Change { get; init; }
}
