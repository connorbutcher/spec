using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Abstractions.Requests.SpecSheets;

public sealed record PublishSpecSheetRequest
{
    public required int Id { get; init; }

    public required string ChangeLog { get; init; }

    public required ICollection<User> Approvers { get; init; }
}
