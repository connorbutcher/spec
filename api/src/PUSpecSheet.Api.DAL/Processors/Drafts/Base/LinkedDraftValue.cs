namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// What a linked sheet cell points at: the other sheet's type and phase, and the table and item in it.
/// </summary>
internal record LinkedDraftValue
{
    public required int? LinkedSpecSheetTypeId { get; init; }

    public required int? LinkedSpecSheetPhaseId { get; init; }

    public required Guid? TableReferenceId { get; init; }

    public required Guid? LinkedReferenceId { get; init; }

    public bool IsEmpty => !LinkedSpecSheetTypeId.HasValue
        && !LinkedSpecSheetPhaseId.HasValue
        && !TableReferenceId.HasValue
        && !LinkedReferenceId.HasValue;
}