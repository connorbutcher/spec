namespace PUSpecSheet.Domain.Sheets;

/// <summary>Where a row revision is in its life.</summary>
public enum RevisionStatus
{
    /// <summary>Being edited. A row's draft locks the row to its author until published or discarded.</summary>
    Draft = 0,

    /// <summary>Part of the sheet's history, visible to everyone from its publish time.</summary>
    Published = 1,
}
