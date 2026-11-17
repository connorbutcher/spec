namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

public enum RowTakeoverParty
{
    /// <summary>The person the row is checked out to, who approves or denies.</summary>
    Holder,

    /// <summary>The person asking for the row, who can withdraw.</summary>
    Requester,
}
