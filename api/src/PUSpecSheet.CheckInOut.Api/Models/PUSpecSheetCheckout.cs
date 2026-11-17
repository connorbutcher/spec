namespace PUSpecSheet.CheckInOut.Api.Models;

public record PUSpecSheetCheckout
{
    public required int Id { get; init; }

    public required ConnectedUser User { get; init; }
}
