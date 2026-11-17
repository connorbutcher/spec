namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// Where a widget or note is: the table or group whose revision holds it, and its place in that revision's list.
/// </summary>
internal record DraftItemNumber
{
    public required int ItemId { get; init; }

    public required int Index { get; init; }
}