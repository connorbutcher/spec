using System.Linq.Expressions;
using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Database.Mappers.SheetTypes;

/// <summary>
/// Sheet types and their identifiers as DTOs. <see cref="Projection"/> is an expression, so a query can
/// select straight into the DTO; <see cref="ToDto(DbSheetType)"/> maps an entity already loaded.
/// </summary>
public static class SheetTypeMapper
{
    public static readonly Expression<Func<DbSheetType, SheetTypeDto>> Projection = sheetType => new SheetTypeDto(sheetType.Id, sheetType.Name, sheetType.DisplayOrder)
    {
        Identifiers = sheetType.Identifiers
            .OrderBy(identifier => identifier.DisplayOrder)
            .ThenBy(identifier => identifier.Id)
            .Select(identifier => new SheetTypeIdentifierDto(
                identifier.Id,
                identifier.Key,
                identifier.Name,
                identifier.Kind,
                identifier.IsRequired,
                identifier.DisplayOrder,
                identifier.Options ?? new List<string>()))
            .ToList(),
    };

    private static readonly Func<DbSheetType, SheetTypeDto> _compiled = Projection.Compile();

    /// <summary>Expects <see cref="DbSheetType.Identifiers"/> to be loaded, or to be empty for a new sheet type.</summary>
    public static SheetTypeDto ToDto(this DbSheetType sheetType)
    {
        return _compiled(sheetType);
    }
}
