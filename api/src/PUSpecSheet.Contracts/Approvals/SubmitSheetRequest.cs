using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Contracts.Approvals;

/// <summary>
/// Submits drafts on a sheet for publishing. Nothing is published until every person in
/// <see cref="ApproverUserIds"/> approves; <see cref="Note"/> becomes the version note and
/// <see cref="Scope"/> says whose drafts go in: the submitter's own (the default) or everyone's.
/// </summary>
public sealed record SubmitSheetRequest
{
    public SubmitSheetRequest(
        [StringLength(1000)] string? note,
        [Required, MinLength(1), MaxLength(20)] IReadOnlyList<int> approverUserIds,
        [EnumDataType(typeof(PublishScope))] PublishScope scope = PublishScope.Mine)
    {
        this.Note = note;
        this.ApproverUserIds = approverUserIds;
        this.Scope = scope;
    }

    public string? Note { get; init; }

    public IReadOnlyList<int> ApproverUserIds { get; init; }

    public PublishScope Scope { get; init; }
}
