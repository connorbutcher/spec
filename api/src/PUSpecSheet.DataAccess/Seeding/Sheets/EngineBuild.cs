namespace PUSpecSheet.DataAccess.Seeding.Sheets;

public sealed record EngineBuild
{
    public required string SerialNumber { get; init; }

    public DateOnly? BuildDate { get; init; }

    public string? Configuration { get; init; }
}
