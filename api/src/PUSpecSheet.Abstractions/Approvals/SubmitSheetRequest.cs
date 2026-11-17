using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.Abstractions.Approvals;

/// <summary>
/// Submits drafts on a sheet for publishing. Nothing is published until every person in
/// <see cref="ApproverUserIds"/> approves; <see cref="Note"/> becomes the version note and
/// <see cref="Scope"/> says whose drafts go in: the submitter's own (the default) or everyone's.
/// </summary>
public sealed record SubmitSheetRequest
{
    [StringLength(1000)]
    public string? Note { get; init; }

    [Required, MinLength(1), MaxLength(20)]
    public required IReadOnlyList<int> ApproverUserIds { get; init; }

    [EnumDataType(typeof(PublishScope))]
    public PublishScope Scope { get; init; } = PublishScope.Mine;
}
