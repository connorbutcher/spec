using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Mappers.Template;

public static class TableRowTemplateMapper
{
    public static TableRowTemplate MapToDomain(this DbTableRowTemplate dbTableRowTemplate)
    {
        return new TableRowTemplate
        {
            Id = dbTableRowTemplate.Id,
            TableGroupTemplateId = dbTableRowTemplate.TableGroupTemplateId,
            RowNumber = dbTableRowTemplate.RowNumber,
            ShowContextMenu = dbTableRowTemplate.ShowContextMenu,
            CanBeReordered = dbTableRowTemplate.CanBeReordered,
            CellTemplates = [.. dbTableRowTemplate.CellTemplates.MapToDomain()],
            Validators = dbTableRowTemplate.Validators,
        };
    }

    public static IEnumerable<TableRowTemplate> MapToDomain(this ICollection<DbTableRowTemplate> dbTableRowTemplates)
    {
        return dbTableRowTemplates.Select(MapToDomain);
    }

    public static IQueryable<TableRowTemplate> MapToDomain(this IQueryable<DbTableRowTemplate> dbTableRowTemplates)
    {
        return dbTableRowTemplates.Select(tr => tr.MapToDomain());
    }
}
