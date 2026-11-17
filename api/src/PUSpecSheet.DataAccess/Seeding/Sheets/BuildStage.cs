namespace PUSpecSheet.DataAccess.Seeding.Sheets;

/// <summary>An assembly stage on an as-built record and the measurements taken at it: name, measured value, min and max.</summary>
public sealed record BuildStage
{
    public required string Name { get; init; }

    public required (string Name, decimal Measured, decimal Min, decimal Max)[] Measurements { get; init; }
}
