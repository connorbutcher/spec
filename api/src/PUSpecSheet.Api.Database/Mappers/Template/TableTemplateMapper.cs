using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Mappers.Template;

public static class TableTemplateMapper
{
    public static TableTemplate MapToDomain(this DbTableTemplate dbTableTemplate)
    {
        return new TableTemplate
        {
            Id = dbTableTemplate.Id,
            SpecSheetTypeId = dbTableTemplate.SpecSheetTypeId,
            Type = dbTableTemplate.Type,
            Description = dbTableTemplate.Description,
            RequiresUserConfiguration = dbTableTemplate.RequiresUserConfiguration ?? false,
            ShowNotes = dbTableTemplate.ShowNotes,
            IsLegacy = dbTableTemplate.IsLegacy,
            DefaultConfiguration = dbTableTemplate.DefaultConfiguration,
            TableGroupTemplates = [.. dbTableTemplate.TableGroupTemplates.MapToDomain()],
            TableTemplateWidgets = [.. dbTableTemplate.TableTemplateWidgets.MapToDomain()]
        };
    }

    public static IEnumerable<TableTemplate> MapToDomain(this ICollection<DbTableTemplate> tableTemplates)
    {
        return tableTemplates.Select(MapToDomain);
    }

    public static IQueryable<TableTemplate> MapToDomain(this IQueryable<DbTableTemplate> tableTemplates)
    {
        return tableTemplates.Select(tt => tt.MapToDomain());
    }
}
