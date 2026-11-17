namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>A tag and the text shown under its heading in the API documentation.</summary>
public sealed record ApiTagDescription
{
    public required string Name { get; init; }

    public required string Description { get; init; }
}
