using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Mappers.Types;

public static class WidgetTypeMapper
{
    public static WidgetType MapToDomain(this DbWidgetType dbWidgetType)
    {
        return new WidgetType
        {
            Id = dbWidgetType.Id,
            Key = dbWidgetType.Key,
            Title = dbWidgetType.Title,
            Description = dbWidgetType.Description
        };
    }
}
