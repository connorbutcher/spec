namespace PUSpecSheet.DataAccess.Seeding.Sheets;

/// <summary>An assembly stage on an as-built record and the measurements taken at it: name, measured value, min and max.</summary>
public sealed record BuildStage
{
    public BuildStage(string name, (string Name, decimal Measured, decimal Min, decimal Max)[] measurements)
    {
        Name = name;
        Measurements = measurements;
    }

    public string Name { get; init; }

    public (string Name, decimal Measured, decimal Min, decimal Max)[] Measurements { get; init; }
}
