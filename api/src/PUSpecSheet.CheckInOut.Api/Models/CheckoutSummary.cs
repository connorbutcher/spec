namespace PUSpecSheet.CheckInOut.Api.Models;

public sealed record CheckoutSummary(IReadOnlyList<PUSpecSheetCheckout> PUSpecSheetCheckouts);