namespace PUSpecSheet.Contracts.Approvals;

/// <summary>Someone who can be asked to approve a submission.</summary>
public sealed record ApproverDto(int UserId, string UserName);
