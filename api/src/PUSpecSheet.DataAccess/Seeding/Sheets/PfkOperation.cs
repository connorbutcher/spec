namespace PUSpecSheet.DataAccess.Seeding.Sheets;

public sealed record PfkOperation
{
    public required string Name { get; init; }

    public required (string Number, string Feature, string Class, decimal Lower, decimal Upper, int Every, string Gauge, string Result, string? ReactionPlan)[] Features { get; init; }
}
