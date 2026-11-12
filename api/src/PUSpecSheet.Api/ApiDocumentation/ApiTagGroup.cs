namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>A heading in the documentation's sidebar and the tags listed under it, in order.</summary>
public sealed record ApiTagGroup
{
    public ApiTagGroup(string name, IReadOnlyList<ApiTagDescription> tags)
    {
        this.Name = name;
        this.Tags = tags;
    }

    public string Name { get; init; }

    public IReadOnlyList<ApiTagDescription> Tags { get; init; }
}
