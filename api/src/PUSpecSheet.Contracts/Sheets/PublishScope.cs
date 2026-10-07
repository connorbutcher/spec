namespace PUSpecSheet.Contracts.Sheets;

/// <summary>Whose drafts a publish takes.</summary>
public enum PublishScope
{
    /// <summary>Only the drafts of the person publishing. Other people's drafts stay as they are.</summary>
    Mine = 0,

    /// <summary>Every draft on the sheet at that moment, whoever made it.</summary>
    All = 1,
}
