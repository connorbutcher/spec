namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>A heading in the documentation's sidebar and the tags listed under it, in order.</summary>
public sealed record ApiTagGroup(string Name, IReadOnlyList<ApiTagDescription> Tags);
