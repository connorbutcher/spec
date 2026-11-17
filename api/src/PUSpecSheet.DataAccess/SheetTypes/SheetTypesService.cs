using System.Globalization;
using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database.Mappers.SheetTypes;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.DataAccess.SheetTypes;

public sealed class SheetTypesService : ISheetTypesService
{
    private readonly ISheetTypeDataService _sheetTypes;

    public SheetTypesService(ISheetTypeDataService sheetTypes)
    {
        _sheetTypes = sheetTypes;
    }

    public async Task<IReadOnlyList<SheetTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _sheetTypes.ListAsync(cancellationToken);
    }

    public async Task<SheetTypeDto> CreateAsync(CreateSheetTypeRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await _sheetTypes.NameExistsAsync(name, cancellationToken))
        {
            throw new ConflictException($"A sheet type called \"{name}\" already exists.");
        }

        var lastOrder = await _sheetTypes.GetLastDisplayOrderAsync(cancellationToken) ?? 0;
        var sheetType = new DbSheetType { Name = name, DisplayOrder = lastOrder + 1 };

        _sheetTypes.Add(sheetType);
        await _sheetTypes.SaveAsync(cancellationToken);

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

        _sheetTypes.AddIdentifier(identifier);
        await _sheetTypes.SaveAsync(cancellationToken);
        return await GetAsync(sheetTypeId, cancellationToken);
    }

    public async Task<SheetTypeDto> UpdateIdentifierAsync(int sheetTypeId, int identifierId, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken)
    {
        var siblings = await ListIdentifiersForEditAsync(sheetTypeId, cancellationToken);
        var identifier = siblings.SingleOrDefault(candidate => candidate.Id == identifierId)
            ?? throw new NotFoundException($"Sheet type {sheetTypeId} has no identifier {identifierId}.");

        if (identifier.Kind != request.Kind && await _sheetTypes.IsIdentifierInUseAsync(identifierId, cancellationToken))
        {
            throw new ConflictException($"Sheets already have a value for {identifier.Name}, so its kind can't change.");
        }

        identifier.Name = CheckedName(request, siblings, identifier);
        identifier.Kind = request.Kind;
        identifier.IsRequired = request.IsRequired;
        identifier.Options = CheckedOptions(request);

        await _sheetTypes.SaveAsync(cancellationToken);
        return await GetAsync(sheetTypeId, cancellationToken);
    }

    public async Task<SheetTypeDto> DeleteIdentifierAsync(int sheetTypeId, int identifierId, CancellationToken cancellationToken)
    {
        var siblings = await ListIdentifiersForEditAsync(sheetTypeId, cancellationToken);
        var identifier = siblings.SingleOrDefault(candidate => candidate.Id == identifierId)
            ?? throw new NotFoundException($"Sheet type {sheetTypeId} has no identifier {identifierId}.");

        if (await _sheetTypes.IsIdentifierInUseAsync(identifierId, cancellationToken))
        {
            throw new ConflictException($"Sheets have a value for {identifier.Name}, so it can't be removed.");
        }

        _sheetTypes.RemoveIdentifier(identifier);
        await _sheetTypes.SaveAsync(cancellationToken);
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
        if (!await _sheetTypes.ExistsAsync(sheetTypeId, cancellationToken))
        {
            throw new NotFoundException($"Sheet type {sheetTypeId} was not found.");
        }

        return await _sheetTypes.ListIdentifiersForEditAsync(sheetTypeId, cancellationToken);
    }

    private async Task<SheetTypeDto> GetAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await _sheetTypes.FindAsync(sheetTypeId, cancellationToken)
            ?? throw new NotFoundException($"Sheet type {sheetTypeId} was not found.");
    }
}
