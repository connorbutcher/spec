using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.SheetTypes.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Mappers.SheetTypes;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Application.SheetTypes;

public sealed class SheetTypesService : ISheetTypesService
{
    private readonly PuSpecSheetDbContext db;

    public SheetTypesService(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<IReadOnlyList<SheetTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await SheetTypeQueries.ListAsync(db, cancellationToken);
    }

    public async Task<SheetTypeDto> CreateAsync(CreateSheetTypeRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await SheetTypeQueries.NameExistsAsync(db, name, cancellationToken))
        {
            throw new ConflictException($"A sheet type called \"{name}\" already exists.");
        }

        var lastOrder = await SheetTypeQueries.GetLastDisplayOrderAsync(db, cancellationToken);
        var sheetType = new DbSheetType { Name = name, DisplayOrder = (lastOrder ?? 0) + 1 };

        db.SheetTypes.Add(sheetType);
        await db.SaveChangesAsync(cancellationToken);

        return sheetType.ToDto();
    }

    public async Task<SheetTypeDto> AddIdentifierAsync(int sheetTypeId, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken)
    {
        var siblings = await ListIdentifiersForEditAsync(sheetTypeId, cancellationToken);
        if (siblings.Count >= SheetIdentifierRules.MaxIdentifiers)
        {
            throw new InvalidRequestException($"A sheet type can have at most {SheetIdentifierRules.MaxIdentifiers} identifiers.");
        }

        var name = CheckedName(request, siblings, null);
        var identifier = new DbSheetTypeIdentifier
        {
            SheetTypeId = sheetTypeId,
            Key = UniqueKey(SheetIdentifierRules.KeyFromName(name), siblings),
            Name = name,
            Kind = request.Kind,
            IsRequired = request.IsRequired,
            DisplayOrder = siblings.Count == 0 ? 1 : siblings.Max(sibling => sibling.DisplayOrder) + 1,
            Options = CheckedOptions(request),
        };

        db.SheetTypeIdentifiers.Add(identifier);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(sheetTypeId, cancellationToken);
    }

    public async Task<SheetTypeDto> UpdateIdentifierAsync(int sheetTypeId, int identifierId, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken)
    {
        var siblings = await ListIdentifiersForEditAsync(sheetTypeId, cancellationToken);
        var identifier = siblings.SingleOrDefault(candidate => candidate.Id == identifierId)
            ?? throw new NotFoundException($"Sheet type {sheetTypeId} has no identifier {identifierId}.");

        if (identifier.Kind != request.Kind && await SheetTypeIdentifierQueries.IsInUseAsync(db, identifierId, cancellationToken))
        {
            throw new ConflictException($"Sheets already have a value for {identifier.Name}, so its kind can't change.");
        }

        identifier.Name = CheckedName(request, siblings, identifier);
        identifier.Kind = request.Kind;
        identifier.IsRequired = request.IsRequired;
        identifier.Options = CheckedOptions(request);

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(sheetTypeId, cancellationToken);
    }

    public async Task<SheetTypeDto> DeleteIdentifierAsync(int sheetTypeId, int identifierId, CancellationToken cancellationToken)
    {
        var identifier = await db.SheetTypeIdentifiers
            .SingleOrDefaultAsync(candidate => candidate.Id == identifierId && candidate.SheetTypeId == sheetTypeId, cancellationToken)
            ?? throw new NotFoundException($"Sheet type {sheetTypeId} has no identifier {identifierId}.");

        if (await SheetTypeIdentifierQueries.IsInUseAsync(db, identifierId, cancellationToken))
        {
            throw new ConflictException($"Sheets have a value for {identifier.Name}, so it can't be removed.");
        }

        db.SheetTypeIdentifiers.Remove(identifier);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(sheetTypeId, cancellationToken);
    }

    private static string CheckedName(SaveSheetTypeIdentifierRequest request, List<DbSheetTypeIdentifier> siblings, DbSheetTypeIdentifier? self)
    {
        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            throw new InvalidRequestException("An identifier needs a name.");
        }

        if (siblings.Any(sibling => sibling != self && string.Equals(sibling.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"This sheet type already has an identifier called \"{name}\".");
        }

        return name;
    }

    /// <summary>The options of an option identifier, trimmed and without blanks; null for the other kinds.</summary>
    private static List<string>? CheckedOptions(SaveSheetTypeIdentifierRequest request)
    {
        if (request.Kind != SheetIdentifierKind.Option)
        {
            return null;
        }

        var options = (request.Options ?? [])
            .Select(option => option.Trim())
            .Where(option => option.Length > 0)
            .ToList();
        if (options.Count == 0)
        {
            throw new InvalidRequestException("An option identifier needs at least one option.");
        }

        if (options.Any(option => option.Length > SheetIdentifierRules.MaxValueLength))
        {
            throw new InvalidRequestException($"An option can be at most {SheetIdentifierRules.MaxValueLength} characters.");
        }

        if (options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Count)
        {
            throw new InvalidRequestException("Each option can be listed only once.");
        }

        return options;
    }

    private static string UniqueKey(string key, List<DbSheetTypeIdentifier> siblings)
    {
        var candidate = key;
        for (var suffix = 2; siblings.Any(sibling => string.Equals(sibling.Key, candidate, StringComparison.OrdinalIgnoreCase)); suffix++)
        {
            var ending = suffix.ToString(CultureInfo.InvariantCulture);
            candidate = key.Length + ending.Length > SheetIdentifierRules.MaxKeyLength
                ? key[..(SheetIdentifierRules.MaxKeyLength - ending.Length)] + ending
                : key + ending;
        }

        return candidate;
    }

    private async Task<List<DbSheetTypeIdentifier>> ListIdentifiersForEditAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        if (!await SheetTypeQueries.ExistsAsync(db, sheetTypeId, cancellationToken))
        {
            throw new NotFoundException($"Sheet type {sheetTypeId} was not found.");
        }

        return await db.SheetTypeIdentifiers
            .Where(identifier => identifier.SheetTypeId == sheetTypeId)
            .ToListAsync(cancellationToken);
    }

    private async Task<SheetTypeDto> GetAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await SheetTypeQueries.FindAsync(db, sheetTypeId, cancellationToken)
            ?? throw new NotFoundException($"Sheet type {sheetTypeId} was not found.");
    }
}
