using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Contracts.Approvals;

/// <summary>
/// Submits drafts on a sheet for publishing. Nothing is published until every person in
/// <paramref name="ApproverUserIds"/> approves; <paramref name="Note"/> becomes the version note and
/// <paramref name="Scope"/> says whose drafts go in: the submitter's own (the default) or everyone's.
/// </summary>
public sealed record SubmitSheetRequest(
    [StringLength(1000)] string? Note,
    [Required, MinLength(1), MaxLength(20)] IReadOnlyList<int> ApproverUserIds,
    [EnumDataType(typeof(PublishScope))] PublishScope Scope = PublishScope.Mine);
