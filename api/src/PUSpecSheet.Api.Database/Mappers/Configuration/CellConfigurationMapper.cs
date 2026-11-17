using PUSpecSheet.Abstractions.Domain.Configuration;
using PUSpecSheet.Api.Database.Schema.Configuration;

namespace PUSpecSheet.Api.Database.Mappers.Configuration;

public static class CellConfigurationMapper
{
    public static CellConfiguration MapToDomain(this DbCellConfiguration dbCellConfiguration)
    {
        return new CellConfiguration
        {
            Id = dbCellConfiguration.Id,
            Styling = dbCellConfiguration.Styling,
            Properties = dbCellConfiguration.Properties,
            Validators = dbCellConfiguration.Validators,
            IsIdentifier = dbCellConfiguration.IsIdentifier,
        };
    }
}
