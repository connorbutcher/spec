using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// Publishes drafts on a sheet as its next version, with an optional note. <paramref name="Scope"/> says
/// whose: the viewer's own (the default) or everyone's.
/// </summary>
public sealed record PublishSheetRequest(
    [StringLength(1000)] string? Note,
    [EnumDataType(typeof(PublishScope))] PublishScope Scope = PublishScope.Mine);
