using System.Globalization;
using Microsoft.Data.SqlClient;

namespace PUSpecSheet.Migrator.Legacy;

// Reads legacy columns by name. The legacy ids are tinyint, smallint and int, so ints are converted rather than read
// as int, which would fail on the narrower types.
internal static class LegacyColumns
{
    public static int Int(this SqlDataReader reader, string column)
    {
        return Convert.ToInt32(reader[column], CultureInfo.InvariantCulture);
    }

    public static int? NullableInt(this SqlDataReader reader, string column)
    {
        var value = reader[column];
        return value is DBNull ? null : Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    public static string? NullableString(this SqlDataReader reader, string column)
    {
        var value = reader[column];
        return value is DBNull ? null : (string)value;
    }

    public static DateTime? NullableDateTime(this SqlDataReader reader, string column)
    {
        var value = reader[column];
        return value is DBNull ? null : DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
    }

    public static DateTime Utc(this SqlDataReader reader, string column)
    {
        return DateTime.SpecifyKind((DateTime)reader[column], DateTimeKind.Utc);
    }

    public static Guid? NullableGuid(this SqlDataReader reader, string column)
    {
        var value = reader[column];
        return value is DBNull ? null : (Guid)value;
    }

    public static bool Bool(this SqlDataReader reader, string column)
    {
        return (bool)reader[column];
    }
}
