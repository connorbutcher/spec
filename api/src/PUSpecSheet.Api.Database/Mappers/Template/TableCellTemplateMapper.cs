using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Api.Database.Mappers.Configuration;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Mappers.Template;

public static class TableCellTemplateMapper
{
    public static TableCellTemplate MapToDomain(this DbTableCellTemplate dbTableCellTemplate)
    {
        return new TableCellTemplate
        {
            Id = dbTableCellTemplate.Id,
            CellConfigurationId = dbTableCellTemplate.CellConfigurationId,
            TableCellTypeId = dbTableCellTemplate.TableCellTypeId,
            TableRowTemplateId = dbTableCellTemplate.TableRowTemplateId,
            ColumnNumber = dbTableCellTemplate.ColumnNumber,
            RowSpan = dbTableCellTemplate.RowSpan,
            ColSpan = dbTableCellTemplate.ColSpan,
            GroupKey = dbTableCellTemplate.GroupKey,
            CellConfiguration = dbTableCellTemplate.CellConfiguration?.MapToDomain(),
            TableCellType = dbTableCellTemplate.TableCellType?.MapToDomain()
        };
    }

    public static IEnumerable<TableCellTemplate> MapToDomain(this ICollection<DbTableCellTemplate> dbTableCellTemplates)
    {
        return dbTableCellTemplates.Select(MapToDomain);
    }

    public static IQueryable<TableCellTemplate> MapToDomain(this IQueryable<DbTableCellTemplate> dbTableCellTemplates)
    {
        return dbTableCellTemplates.Select(tc => tc.MapToDomain());
    }
}
