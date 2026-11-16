using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Database;
using PUSpecSheet.Domain.SheetTypes;

namespace PUSpecSheet.Application.Published.Sheets;

/// <summary>
/// Turns the identifier values a caller gives by key (<c>?serialNumber=ESN-1</c>) into filters on stored
/// values, so a number or date written another way still matches.
/// </summary>
public static class PublishedIdentifierFilters
{
    public static async Task<List<PublishedIdentifierFilter>> ResolveAsync(
        PuSpecSheetDbContext db,
        int sheetTypeId,
        IReadOnlyDictionary<string, string> given,
        CancellationToken cancellationToken)
    {
        if (given.Count == 0)
        {
            return [];
        }

        var identifiers = await db.SheetTypeIdentifiers
            .AsNoTracking()
            .Where(identifier => identifier.SheetTypeId == sheetTypeId)
            .ToListAsync(cancellationToken);

        var filters = new List<PublishedIdentifierFilter>();
        foreach (var (key, raw) in given)
        {
            var identifier = identifiers.FirstOrDefault(candidate => string.Equals(candidate.Key, key, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidRequestException($"Sheet type {sheetTypeId} has no identifier \"{key}\".");
            var definition = new SheetIdentifierDefinition(identifier.Id, identifier.Key, identifier.Kind, false, identifier.Options ?? []);
            if (!SheetIdentifierRules.TryNormalise(definition, raw, out var stored, out var error))
            {
                throw new InvalidRequestException(error!);
            }

            filters.Add(new PublishedIdentifierFilter(identifier.Id, stored));
        }

        return filters;
    }
}
