using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>A column block copy on a published horizontal table, left to right.</summary>
public sealed record PublishedColumnBlockDto(
    Guid Id,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Name);
