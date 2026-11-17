using System.Linq.Expressions;
using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Application.SheetTypes;

/// <summary>Sheet types and their identifiers as contracts, written as expressions so queries project straight to them.</summary>
public static class SheetTypeMappings
{
    public static readonly Expression<Func<DbSheetType, SheetTypeDto>> ToDto = sheetType => new SheetTypeDto(sheetType.Id, sheetType.Name, sheetType.DisplayOrder)
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
}
