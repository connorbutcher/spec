using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Mappers.Legacy.References;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Mappers.Legacy.Workflow;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.SpecSheets;

public static class SpecSheetMapper
{
    public static SpecSheet MapToDomain(this DbSpecSheet dbSpecSheet)
    {
        return new SpecSheet
        {
            Id = dbSpecSheet.Id,
            LegacySpecSheetId = dbSpecSheet.LegacySpecSheetId,
            SpecSheetPhaseId = dbSpecSheet.SpecSheetPhaseId,
            SpecSheetTypeId = dbSpecSheet.SpecSheetTypeId,
            IsPublished = dbSpecSheet.IsPublished,
            ChangeLog = dbSpecSheet.ChangeLog,
            CreatedByUserId = dbSpecSheet.CreatedByUserId,
            CreatedDateTime = dbSpecSheet.CreatedDateTime,
            CreatedByUser = dbSpecSheet.CreatedByUser!.MapToDomain(),
            PublishedByUserId = dbSpecSheet.PublishedByUserId,
            PublishedDateTime = dbSpecSheet.PublishedDateTime,
            HasBeenMigrated = dbSpecSheet.HasBeenMigrated,
            PublishedByUser = dbSpecSheet.PublishedByUser?.MapToDomain(),
            SpecSheetPhase = dbSpecSheet.SpecSheetPhase!.MapToDomain(),
            SpecSheetType = dbSpecSheet.SpecSheetType!.MapToDomain(),
            Tables = [.. dbSpecSheet.Tables.MapToDomain()],
            PropertyValues = [.. dbSpecSheet.PropertyValues.MapToDomain()],
            WorkflowSteps = [.. dbSpecSheet.WorkflowSteps.MapToDomain()],
            ExternalReferences = [.. dbSpecSheet.ExternalReferences.MapToDomain()]
        };
    }

    public static SpecSheetSummary MapToSummaryDomain(this DbSpecSheet dbSpecSheet)
    {
        return new SpecSheetSummary
        {
            Id = dbSpecSheet.Id,
            LegacySpecSheetId = dbSpecSheet.LegacySpecSheetId,
            SpecSheetPhaseId = dbSpecSheet.SpecSheetPhaseId,
            SpecSheetTypeId = dbSpecSheet.SpecSheetTypeId,
            IsPublished = dbSpecSheet.IsPublished,
            CreatedByUserId = dbSpecSheet.CreatedByUserId,
            CreatedDateTime = dbSpecSheet.CreatedDateTime,
            CreatedByUser = dbSpecSheet.CreatedByUser!.MapToDomain(),
            HasBeenMigrated = dbSpecSheet.HasBeenMigrated,
            PublishedByUserId = dbSpecSheet.PublishedByUserId,
            PublishedDateTime = dbSpecSheet.PublishedDateTime,
            PublishedByUser = dbSpecSheet.PublishedByUser?.MapToDomain(),
            ChangeLog = dbSpecSheet.ChangeLog,
            SpecSheetType = dbSpecSheet.SpecSheetType!.MapToDomain(),
            SpecSheetPhase = dbSpecSheet.SpecSheetPhase!.MapToDomain(),
            PropertyValues = [.. dbSpecSheet.PropertyValues.MapToSummaryDomain()]
        };
    }

    public static IQueryable<SpecSheet> MapToDomain(this IQueryable<DbSpecSheet> dbSpecSheets)
    {
        return dbSpecSheets
            .Select(db => db.MapToDomain());
    }

    public static IQueryable<SpecSheetSummary> MapToSummaryDomain(this IQueryable<DbSpecSheet> dbSpecSheets)
    {
        return dbSpecSheets
            .Select(db => db.MapToSummaryDomain());
    }
}
