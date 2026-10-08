using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Api.Collaboration;

/// <summary>What the server sends to browsers that have a sheet open.</summary>
public interface ISheetHubClient
{
    /// <summary>Who has the sheet open now.</summary>
    Task PresenceChanged(IReadOnlyList<SheetPresenceUserDto> users);

    /// <summary>Which rows people are in without having changed them yet. Replaces the list before.</summary>
    Task CheckoutsChanged(IReadOnlyList<RowCheckoutDto> checkouts);

    /// <summary>A row was checked out or released, or a version was published: read the sheet again.</summary>
    Task SheetChanged();

    /// <summary>A takeover request the receiver made or has to answer was opened or settled.</summary>
    Task TakeoverChanged(RowTakeoverDto takeover);
}
