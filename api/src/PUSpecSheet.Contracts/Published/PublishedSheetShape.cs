namespace PUSpecSheet.Contracts.Published;

/// <summary>How a published sheet's data is laid out in the response.</summary>
public enum PublishedSheetShape
{
    /// <summary>Tables, their sections, rows and cells, nested and in display order.</summary>
    Tree = 0,

    /// <summary>One map of cell identifier to value, for a caller that knows the cells it wants.</summary>
    Flat = 1,
}
