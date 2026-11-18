using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Query.Models;
using PUSpecSheet.Query.Models.Structure;
using PUSpecSheet.Query.Models.Table;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class SlimSheetMapper
{
    public static SlimPUSpecSheetSummary MapToSlimSummary(this SheetVersionSummary summary, SlimStructureLevel displayLevel)
    {
        return new SlimPUSpecSheetSummary
        {
            Id = summary.Version.SpecSheetId(),
            LegacySpecSheetId = summary.Version.LegacySpecSheetId,
            PUSpecSheetPhase = summary.SpecSheetPhase!.MapToSlimPhase(),
            PUSpecSheetType = summary.SpecSheetType!.MapToSlimType(),
            PublishedDateTime = summary.Version.PublishedDateTime,
            ChangeLog = summary.Version.ChangeLog!,
            Properties = [.. summary.Properties.MapToSlimPropertyValueSummaries()],
            DisplayLevel = displayLevel
        };
    }

    public static SlimPUSpecSheet MapToSlimSheet(this Sheet sheet, ICollection<SlimPUSpecSheetTable> tables, SlimStructureLevel displayLevel)
    {
        var version = sheet.Version ?? throw new InvalidOperationException($"Sheet with Id {sheet.Id} isn't published.");
        return new SlimPUSpecSheet
        {
            Id = version.SpecSheetId(),
            LegacySpecSheetId = version.LegacySpecSheetId,
            PUSpecSheetPhase = sheet.SpecSheetPhase!.MapToSlimPhase(),
            PUSpecSheetType = sheet.SpecSheetType!.MapToSlimType(),
            PublishedDateTime = version.PublishedDateTime,
            ChangeLog = version.ChangeLog!,
            Tables = tables,
            Properties = [.. sheet.Properties.MapToSlimPropertyValues()],
            DisplayLevel = displayLevel
        };
    }

    /// <summary>
    /// The id the version is known by to the query API: the id of the SpecSheets row it came from.
    /// </summary>
    public static int SpecSheetId(this SheetVersion version)
    {
        return version.SourceSpecSheetId
            ?? throw new InvalidOperationException($"Version {version.VersionNumber} of Sheet with Id {version.SheetId} has no spec sheet id.");
    }
}
