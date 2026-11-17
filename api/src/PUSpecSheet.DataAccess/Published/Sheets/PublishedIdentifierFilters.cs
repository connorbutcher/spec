using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.DataAccess.Published.Sheets;

// Values are normalised as stored, so a number or date written another way still matches.
public static class PublishedIdentifierFilters
{
    public static List<PublishedIdentifierFilter> For(
        int sheetTypeId,
        IReadOnlyList<DbSheetTypeIdentifier> identifiers,
        IReadOnlyDictionary<string, string> given)
    {
        var filters = new List<PublishedIdentifierFilter>();
        foreach (var (key, raw) in given)
        {
            var identifier = identifiers.FirstOrDefault(candidate => string.Equals(candidate.Key, key, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidRequestException($"Sheet type {sheetTypeId} has no identifier \"{key}\".");
            var definition = new SheetIdentifierDefinition
            {
                Id = identifier.Id,
                Name = identifier.Key,
                Kind = identifier.Kind,
                IsRequired = false,
                Options = identifier.Options ?? [],
            };
            if (!SheetIdentifierRules.TryNormalise(definition, raw, out var stored, out var error))
            {
                throw new InvalidRequestException(error!);
            }

            filters.Add(new PublishedIdentifierFilter { IdentifierId = identifier.Id, Value = stored });
        }

        return filters;
    }
}
