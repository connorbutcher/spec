using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Mappers.Sheets;

/// <summary>Sheet versions as DTOs.</summary>
public static class SheetVersionMapper
{
    /// <summary>Expects <see cref="DbSheetVersion.PublishedBy"/> to be loaded.</summary>
    public static SheetVersionSummaryDto ToSummaryDto(this DbSheetVersion version)
    {
        return new SheetVersionSummaryDto
        {
            VersionNumber = version.VersionNumber,
            PublishedAtUtc = UtcTimes.AsUtc(version.PublishedAtUtc),
            PublishedByUserId = version.PublishedByUserId,
            PublishedByName = version.PublishedBy.DisplayName,
            Note = version.Note,
        };
    }
}
