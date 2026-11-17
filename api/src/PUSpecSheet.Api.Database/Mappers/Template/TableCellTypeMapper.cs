using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Mappers.Template;

public static class TableCellTypeMapper
{
    public static TableCellType MapToDomain(this DbTableCellType dbTableCellType)
    {
        return new TableCellType
        {
            Id = dbTableCellType.Id,
            Key = dbTableCellType.Key,
            Title = dbTableCellType.Title,
            Description = dbTableCellType.Description,
            Type = dbTableCellType.Type,
            ModelType = dbTableCellType.ModelType,
            RequiresUserConfiguration = dbTableCellType.RequiresUserConfiguration,
            Styling = dbTableCellType.Styling
        };
    }

    public static IEnumerable<TableCellType> MapToDomain(this ICollection<DbTableCellType> cellTypes)
    {
        return cellTypes.Select(MapToDomain);
    }

    public static IQueryable<TableCellType> MapToDomain(this IQueryable<DbTableCellType> cellTypes)
    {
        return cellTypes.Select(ct => ct.MapToDomain());
    }
}
