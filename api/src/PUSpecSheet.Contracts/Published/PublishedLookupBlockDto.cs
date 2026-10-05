using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// A column block of the table a value was found in, left to right. <see cref="Keys"/> holds the values of
/// the block's own cells that have a lookup key, by key, so a caller can tell which part a block is.
/// </summary>
public sealed record PublishedLookupBlockDto(
    Guid Block,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyDictionary<string, object>? Keys);
