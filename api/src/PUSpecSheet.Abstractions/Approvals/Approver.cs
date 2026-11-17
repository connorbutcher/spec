namespace PUSpecSheet.Abstractions.Approvals;

/// <summary>Someone who can be asked to approve a submission.</summary>
public sealed record Approver
{
    public required int UserId { get; init; }

    public required string UserName { get; init; }
}
