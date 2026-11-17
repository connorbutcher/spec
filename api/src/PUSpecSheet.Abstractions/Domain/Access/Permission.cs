namespace PUSpecSheet.Abstractions.Domain.Access;

public sealed class Permission
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Key { get; init; }
}
