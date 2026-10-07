namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>Where a request to take over a row's checkout has got to.</summary>
public enum RowTakeoverStatus
{
    /// <summary>Waiting for the person the row is checked out to.</summary>
    Pending,

    /// <summary>They agreed, and the row is now checked out to the person who asked.</summary>
    Approved,

    /// <summary>They refused, and the row stays with them.</summary>
    Denied,

    /// <summary>The person who asked withdrew the request.</summary>
    Cancelled,

    /// <summary>They didn't answer in time, so the row went to the person who asked.</summary>
    GrantedOnTimeout,

    /// <summary>They didn't have the sheet open, so the row went to the person who asked straight away.</summary>
    GrantedHolderAway,

    /// <summary>The row stopped being checked out to them (published or discarded) before the request was answered.</summary>
    Released,
}
