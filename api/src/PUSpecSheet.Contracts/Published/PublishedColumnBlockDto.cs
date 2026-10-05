using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// A set of repeated columns on a published table, such as one part's columns, left to right. A cell in
/// the set carries this <see cref="Id"/> as its <c>column</c>.
/// </summary>
public sealed record PublishedColumnBlockDto(
    Guid Id,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Name);
