using System.Linq.Expressions;
using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Database.Mappers.SheetTypes;

/// <summary>
/// Sheet types and their identifiers as models. <see cref="Projection"/> is an expression, so a query can
/// select straight into the model; <see cref="ToModel(DbSheetType)"/> maps an entity already loaded.
/// </summary>
public static class SheetTypeMapper
{
    public static readonly Expression<Func<DbSheetType, SheetType>> Projection = sheetType => new SheetType
    {
        Id = sheetType.Id,
        Name = sheetType.Name,
        DisplayOrder = sheetType.DisplayOrder,
        Identifiers = sheetType.Identifiers
            .OrderBy(identifier => identifier.DisplayOrder)
            .ThenBy(identifier => identifier.Id)
            .Select(identifier => new SheetTypeIdentifier
            {
                Id = identifier.Id,
                Key = identifier.Key,
                Name = identifier.Name,
                Kind = identifier.Kind,
                IsRequired = identifier.IsRequired,
                DisplayOrder = identifier.DisplayOrder,
                Options = identifier.Options ?? new List<string>(),
            })
            .ToList(),
    };

    private static readonly Func<DbSheetType, SheetType> _compiled = Projection.Compile();

    /// <summary>Expects <see cref="DbSheetType.Identifiers"/> to be loaded, or to be empty for a new sheet type.</summary>
    public static SheetType ToModel(this DbSheetType sheetType)
    {
        return _compiled(sheetType);
    }
}
