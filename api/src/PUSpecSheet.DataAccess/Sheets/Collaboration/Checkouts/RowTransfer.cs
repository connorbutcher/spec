namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

public enum RowTransfer
{
    // The person moved from no longer held the row, so nothing changed.
    Nothing,

    Live,

    // The row's draft moved, so the sheet shows it locked to someone else.
    Draft,
}
