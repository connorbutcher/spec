namespace PUSpecSheet.Application.Seeding;

/// <summary>One machining operation of the sample PFK register and the product features checked at it.</summary>
public sealed record PfkOperation(
    string Name,
    (string Number, string Feature, string Class, decimal Lower, decimal Upper, int Every, string Gauge, string Result, string? ReactionPlan)[] Features);
