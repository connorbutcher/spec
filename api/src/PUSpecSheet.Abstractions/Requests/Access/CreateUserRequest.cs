namespace PUSpecSheet.Abstractions.Requests.Access;

public sealed record CreateUserRequest
{
    public required Guid ObjectId { get; init; }

    public required string DisplayName { get; init; }

    public required string Email { get; init; }
}
