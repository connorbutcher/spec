using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PUSpecSheet.Api.Database.Conventions;

public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeUtcConverter() : base(
        v => v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
