namespace PUSpecSheet.Application.Sheets.Removal;

/// <summary>A draft that removes a section or table, which nobody has published yet.</summary>
public sealed record PendingRemoval(RemovedContainer Container, int AuthorUserId, bool AwaitingApproval);
