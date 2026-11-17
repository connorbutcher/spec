namespace PUSpecSheet.CheckInOut.Api.Models;

public sealed record CheckoutRequest
{
    public required Guid RequestContextId { get; init; }

    public required int Id { get; init; }

    public required ConnectedUser RequestingUser { get; init; }

    public required ConnectedUser RespondingUser { get; init; }

    public required DateTime RequestedDateTime { get; init; }
}
