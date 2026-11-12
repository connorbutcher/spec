using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// Publishes drafts on a sheet as its next version, with an optional note. <see cref="Scope"/> says
/// whose: the viewer's own (the default) or everyone's.
/// </summary>
public sealed record PublishSheetRequest
{
    public PublishSheetRequest(
        [StringLength(1000)] string? note,
        [EnumDataType(typeof(PublishScope))] PublishScope scope = PublishScope.Mine)
    {
        this.Note = note;
        this.Scope = scope;
    }

    public string? Note { get; init; }

    public PublishScope Scope { get; init; }
}
