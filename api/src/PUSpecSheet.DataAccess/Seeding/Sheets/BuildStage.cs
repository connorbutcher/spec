namespace PUSpecSheet.DataAccess.Seeding.Sheets;

public sealed record BuildStage
{
    public required string Name { get; init; }

    public required (string Name, decimal Measured, decimal Min, decimal Max)[] Measurements { get; init; }
}
