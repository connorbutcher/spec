namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>A tag and the text shown under its heading in the API documentation.</summary>
public sealed record ApiTagDescription
{
    public ApiTagDescription(string name, string description)
    {
        this.Name = name;
        this.Description = description;
    }

    public string Name { get; init; }

    public string Description { get; init; }
}
