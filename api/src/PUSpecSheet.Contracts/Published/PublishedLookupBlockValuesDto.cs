namespace PUSpecSheet.Contracts.Published;

/// <summary>A row's values in one column block: one entry per cell, left to right, null for an empty cell.</summary>
public sealed record PublishedLookupBlockValuesDto(
    Guid Block,
    IReadOnlyList<object?> Values);
