namespace PUSpecSheet.Database.Mappers;

/// <summary>
/// The database stores UTC times without a zone, so EF Core reads them back as <see cref="DateTimeKind.Unspecified"/>.
/// Mappers mark them as UTC before they go out, so they are written with a <c>Z</c>.
/// </summary>
internal static class UtcTimes
{
    public static DateTime AsUtc(DateTime value)
    {
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public static DateTime? AsUtc(DateTime? value)
    {
        return value is { } time ? AsUtc(time) : null;
    }
}
