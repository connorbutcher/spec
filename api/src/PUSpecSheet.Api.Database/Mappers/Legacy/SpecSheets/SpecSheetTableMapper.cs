using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.SpecSheets;

public static class SpecSheetTableMapper
{
    public static SpecSheetTable MapToDomain(this DbSpecSheetTable dbSpecSheetTable)
    {
        var groups = dbSpecSheetTable.Groups
            .MapToDomain()
            .OrderBy(r => r.Order)
            .ToList();

        return new SpecSheetTable
        {
            Id = dbSpecSheetTable.Id,
            SpecSheetId = dbSpecSheetTable.SpecSheetId,
            TableTemplateId = dbSpecSheetTable.TableTemplateId,
            Title = dbSpecSheetTable.Title,
            DisplayOrder = dbSpecSheetTable.DisplayOrder,
            ReferenceId = dbSpecSheetTable.ReferenceId,
            IsDeleted = dbSpecSheetTable.IsDeleted,
            Groups = groups,
            Widgets = [.. dbSpecSheetTable.Widgets.MapToDomain()],
            Notes = [.. dbSpecSheetTable.Notes.MapToDomain()],
            CreatedByUserId = dbSpecSheetTable.CreatedByUserId,
            CreatedDateTime = dbSpecSheetTable.CreatedDateTime,
            CreatedByUser = dbSpecSheetTable.CreatedByUser?.MapToDomain(),
            LastModifiedByUserId = dbSpecSheetTable.LastModifiedByUserId,
            LastModifiedDateTime = dbSpecSheetTable.LastModifiedDateTime,
            LastModifiedByUser = dbSpecSheetTable.LastModifiedByUser?.MapToDomain(),
            ConfigurationOverride = dbSpecSheetTable.ConfigurationOverride,
        };
    }

    public static IEnumerable<SpecSheetTable> MapToDomain(this ICollection<DbSpecSheetTable> dbSpecSheetTables)
    {
        return dbSpecSheetTables.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetTable> MapToDomain(this IQueryable<DbSpecSheetTable> dbSpecSheetTables)
    {
        return dbSpecSheetTables
            .Select(db => db.MapToDomain());
    }
}
