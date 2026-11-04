namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>What moving a row from one person to another moved.</summary>
public enum RowTransfer
{
    /// <summary>The person moved from no longer held the row, so nothing changed.</summary>
    Nothing,

    /// <summary>Only a live checkout moved. The sheet itself is as it was; only the checkout list changed.</summary>
    Live,

    /// <summary>The row's draft moved, so the sheet shows it locked to someone else.</summary>
    Draft,
}
