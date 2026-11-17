using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Phases;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.SheetTypes;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Catalog;

/// <summary>
/// Creates sheets. A sheet is told apart from the phase's other sheets of its type by its identifier values,
/// held only in spec.SheetIdentifierValues; they never change afterwards. Every creation for a phase and sheet
/// type takes the same application lock, then compares the values asked for with the sheets already there,
/// so two people creating the same sheet at once get one sheet and a conflict.
/// </summary>
public sealed class SheetCreator
{
    private const int LockTimeoutMilliseconds = 15000;

    private readonly PuSpecSheetDbContext _db;
    private readonly SheetCopier _copier;
    private readonly IPhaseDataService _phases;
    private readonly ISheetTypeDataService _sheetTypes;

    public SheetCreator(PuSpecSheetDbContext db, SheetCopier copier, IPhaseDataService phases, ISheetTypeDataService sheetTypes)
    {
        _db = db;
        _copier = copier;
        _phases = phases;
        _sheetTypes = sheetTypes;
    }

    /// <summary>
    /// Creates a sheet with the identifier values asked for, optionally starting as a copy of another sheet
    /// of the same type, and returns its id.
    /// </summary>
    public async Task<int> CreateAsync(int phaseId, CreateSheetRequest request, CancellationToken cancellationToken)
    {
        await EnsureAvailableAsync(phaseId, request.SheetTypeId, cancellationToken);
        var definitions = await _sheetTypes.ListIdentifierDefinitionsAsync(request.SheetTypeId, cancellationToken);
        var values = CheckedValues(definitions, request.Identifiers);
        var copyFrom = await FindCopySourceAsync(request, cancellationToken);

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        await TakeIdentityLockAsync(phaseId, request.SheetTypeId, cancellationToken);
        if (await HasSheetWithIdentityAsync(phaseId, request.SheetTypeId, definitions, values, cancellationToken))
        {
            throw new ConflictException(values.Count == 0
                ? "This phase already has a sheet of this type with no identifiers."
                : $"This phase already has a sheet of this type with {Describe(values)}.");
        }

        var sheet = new DbSheet { PhaseId = phaseId, SheetTypeId = request.SheetTypeId };
        foreach (var value in values)
        {
            sheet.IdentifierValues.Add(new DbSheetIdentifierValue { SheetTypeIdentifierId = value.Definition.Id, Value = value.Value });
        }

        _db.Sheets.Add(sheet);
        await _db.SaveChangesAsync(cancellationToken);

        if (copyFrom is { } sourceId)
        {
            await _copier.CopyAsync(sourceId, sheet.Id, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return sheet.Id;
    }

    /// <summary>
    /// Creates the phase's sheet of the type with no identifier values, the way a sheet type without
    /// identifiers has always had one sheet per phase, and returns its id. Someone creating it at the same
    /// moment is fine: their sheet is the one returned.
    /// </summary>
    public async Task<int> CreateUnidentifiedAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        await EnsureAvailableAsync(phaseId, sheetTypeId, cancellationToken);
        var definitions = await _sheetTypes.ListIdentifierDefinitionsAsync(sheetTypeId, cancellationToken);
        if (definitions.Any(definition => definition.IsRequired))
        {
            throw new InvalidRequestException("Sheets of this type need their identifiers. Create one from the phase's list of sheets.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        await TakeIdentityLockAsync(phaseId, sheetTypeId, cancellationToken);
        var existingId = await SheetQueries.FindIdAsync(_db, phaseId, sheetTypeId, cancellationToken);
        if (existingId is { } id)
        {
            return id;
        }

        var sheet = new DbSheet { PhaseId = phaseId, SheetTypeId = sheetTypeId };
        _db.Sheets.Add(sheet);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return sheet.Id;
    }

    private static List<SheetIdentifierValue> CheckedValues(List<SheetIdentifierDefinition> definitions, IReadOnlyList<SheetIdentifierValueRequest> given)
    {
        var unknown = given.FirstOrDefault(value => definitions.All(definition => definition.Id != value.IdentifierId));
        if (unknown is not null)
        {
            throw new InvalidRequestException($"Identifier {unknown.IdentifierId} isn't one of this sheet type's.");
        }

        if (given.GroupBy(value => value.IdentifierId).Any(group => group.Count() > 1))
        {
            throw new InvalidRequestException("Each identifier can be given only once.");
        }

        var values = new List<SheetIdentifierValue>();
        var errors = new List<string>();
        foreach (var definition in definitions)
        {
            var raw = given.FirstOrDefault(value => value.IdentifierId == definition.Id)?.Value;
            if (!SheetIdentifierRules.TryNormalise(definition, raw, out var stored, out var error))
            {
                errors.Add(error!);
            }
            else if (stored is not null)
            {
                values.Add(new SheetIdentifierValue { Definition = definition, Value = stored });
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidRequestException(string.Join(" ", errors));
        }

        return values;
    }

    private static string Describe(List<SheetIdentifierValue> values)
    {
        return string.Join(", ", values.Select(value => $"{value.Definition.Name} {value.Value}"));
    }

    private async Task EnsureAvailableAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        if (!await _phases.HasSheetTypeAsync(phaseId, sheetTypeId, cancellationToken))
        {
            throw new NotFoundException("That sheet type isn't available to that phase.");
        }
    }

    private async Task<int?> FindCopySourceAsync(CreateSheetRequest request, CancellationToken cancellationToken)
    {
        if (request.CopyFromSheetId is not { } sourceId)
        {
            return null;
        }

        var source = await SheetQueries.FindAsync(_db, sourceId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sourceId} was not found.");
        if (source.SheetTypeId != request.SheetTypeId)
        {
            throw new InvalidRequestException("A sheet can only start as a copy of a sheet of the same type.");
        }

        return sourceId;
    }

    /// <summary>
    /// Waits for anyone else creating a sheet of the type in the phase; released when the transaction ends.
    /// </summary>
    private async Task TakeIdentityLockAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var resource = $"SheetIdentity:{phaseId}:{sheetTypeId}";
        await _db.Database.ExecuteSqlAsync(
            $"""
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = {resource},
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = {LockTimeoutMilliseconds};
            IF @result < 0 THROW 50000, 'Someone else is creating a sheet of this type and it is taking too long. Try again.', 1;
            """,
            cancellationToken);
    }

    /// <summary>Whether the phase already has a sheet of the type whose identifier values name the same thing.</summary>
    private async Task<bool> HasSheetWithIdentityAsync(
        int phaseId,
        int sheetTypeId,
        List<SheetIdentifierDefinition> definitions,
        List<SheetIdentifierValue> values,
        CancellationToken cancellationToken)
    {
        var byId = definitions.ToDictionary(definition => definition.Id);
        var wanted = SheetIdentifierRules.ComparisonKey(values);
        var sheets = await SheetQueries.ListWithIdentifiersAsync(_db, phaseId, sheetTypeId, cancellationToken);
        return sheets.Any(sheet => SheetIdentifierRules.ComparisonKey(sheet.IdentifierValues
            .Select(value => new SheetIdentifierValue { Definition = byId[value.SheetTypeIdentifierId], Value = value.Value })) == wanted);
    }
}
