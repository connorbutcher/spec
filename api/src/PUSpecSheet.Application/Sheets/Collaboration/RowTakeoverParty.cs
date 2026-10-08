namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>The two people in a takeover request.</summary>
public enum RowTakeoverParty
{
    /// <summary>The person the row is checked out to, who approves or denies.</summary>
    Holder,

    /// <summary>The person asking for the row, who can withdraw.</summary>
    Requester,
}
