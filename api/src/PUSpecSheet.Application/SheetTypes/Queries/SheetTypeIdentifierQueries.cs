using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.SheetTypes.Queries;

/// <summary>The identifiers that tell the sheets of a sheet type apart.</summary>
public static class SheetTypeIdentifierQueries
{
    /// <summary>The sheet type's identifiers as the identity rules see them, in display order.</summary>
    public static async Task<List<SheetIdentifierDefinition>> ListDefinitionsAsync(PuSpecSheetDbContext db, int sheetTypeId, CancellationToken cancellationToken)
    {
        var identifiers = await db.SheetTypeIdentifiers
            .AsNoTracking()
            .Where(identifier => identifier.SheetTypeId == sheetTypeId)
            .OrderBy(identifier => identifier.DisplayOrder)
            .ThenBy(identifier => identifier.Id)
            .ToListAsync(cancellationToken);
        return identifiers
            .Select(identifier => new SheetIdentifierDefinition(identifier.Id, identifier.Name, identifier.Kind, identifier.IsRequired, identifier.Options ?? []))
            .ToList();
    }

    /// <summary>Whether any sheet has a value for the identifier.</summary>
    public static async Task<bool> IsInUseAsync(PuSpecSheetDbContext db, int identifierId, CancellationToken cancellationToken)
    {
        return await db.SheetIdentifierValues.AnyAsync(value => value.SheetTypeIdentifierId == identifierId, cancellationToken);
    }
}
