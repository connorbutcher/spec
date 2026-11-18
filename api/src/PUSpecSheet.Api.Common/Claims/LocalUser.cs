namespace PUSpecSheet.Api.Common.Claims;

/// <summary>
/// One of the users a request can act as while auth is disabled for local running.
/// </summary>
public sealed record LocalUser
{
    public required Guid ObjectId { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }
}
