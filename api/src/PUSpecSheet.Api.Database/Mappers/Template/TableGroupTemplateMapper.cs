using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Mappers.Template;

public static class TableGroupTemplateMapper
{
    public static TableGroupTemplate MapToDomain(this DbTableGroupTemplate dbTableGroupTemplate)
    {
        return new TableGroupTemplate
        {
            Id = dbTableGroupTemplate.Id,
            ParentTableGroupTemplateId = dbTableGroupTemplate.ParentTableGroupTemplateId,
            TableTemplateId = dbTableGroupTemplate.TableTemplateId,
            Title = dbTableGroupTemplate.Title,
            GroupType = dbTableGroupTemplate.GroupType,
            GroupKey = dbTableGroupTemplate.GroupKey,
            DisplayOrder = dbTableGroupTemplate.DisplayOrder,
            IsDefault = dbTableGroupTemplate.IsDefault,
            CanUserAddToTable = dbTableGroupTemplate.CanUserAddToTable,
            ShowInContextMenu = dbTableGroupTemplate.ShowInContextMenu,
            CanBeReordered = dbTableGroupTemplate.CanBeReordered,
            CopyFromPrevious = dbTableGroupTemplate.CopyFromPrevious,
            ShowContextMenu = dbTableGroupTemplate.ShowContextMenu,
            RowTemplates = [.. dbTableGroupTemplate.RowTemplates.MapToDomain()],
            TableGroupTemplateWidgets = [.. dbTableGroupTemplate.TableGroupTemplateWidgets.MapToDomain()]
        };
    }

    public static IEnumerable<TableGroupTemplate> MapToDomain(this ICollection<DbTableGroupTemplate> tableGroups)
    {
        return tableGroups.Select(MapToDomain);
    }

    public static IQueryable<TableGroupTemplate> MapToDomain(this IQueryable<DbTableGroupTemplate> tableGroups)
    {
        return tableGroups.Select(tg => tg.MapToDomain());
    }
}
