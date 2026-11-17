namespace PUSpecSheet.DataAccess.Seeding.Sheets;

/// <summary>The identifier values of one engine's Engine Specifications sheet: its serial number, and optionally when and how it was built.</summary>
public sealed record EngineBuild
{
    public EngineBuild(string serialNumber, DateOnly? buildDate, string? configuration)
    {
        SerialNumber = serialNumber;
        BuildDate = buildDate;
        Configuration = configuration;
    }

    public string SerialNumber { get; init; }

    public DateOnly? BuildDate { get; init; }

    public string? Configuration { get; init; }
}
