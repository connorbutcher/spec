namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>A heading in the documentation's sidebar and the tags listed under it, in order.</summary>
public sealed record ApiTagGroup
{
    public required string Name { get; init; }

    public required IReadOnlyList<ApiTagDescription> Tags { get; init; }
}
