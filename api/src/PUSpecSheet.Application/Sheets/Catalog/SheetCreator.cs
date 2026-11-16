using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Phases.Queries;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.SheetTypes.Queries;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Domain.SheetTypes;

namespace PUSpecSheet.Application.Sheets.Catalog;

/// <summary>
/// Creates sheets. A sheet is told apart from the phase's other sheets of its type by its identifier values,
/// which the database holds unique through the sheet's identity key; they never change afterwards.
/// </summary>
public sealed class SheetCreator
{
    private readonly PuSpecSheetDbContext db;
    private readonly SheetCopier copier;

    public SheetCreator(PuSpecSheetDbContext db, SheetCopier copier)
    {
        this.db = db;
        this.copier = copier;
    }

    /// <summary>
    /// Creates a sheet with the identifier values asked for, optionally starting as a copy of another sheet
    /// of the same type, and returns its id.
    /// </summary>
    public async Task<int> CreateAsync(int phaseId, CreateSheetRequest request, CancellationToken cancellationToken)
    {
        await EnsureAvailableAsync(phaseId, request.SheetTypeId, cancellationToken);
        var definitions = await SheetTypeIdentifierQueries.ListDefinitionsAsync(db, request.SheetTypeId, cancellationToken);
        var values = CheckedValues(definitions, request.Identifiers);
        var copyFrom = await FindCopySourceAsync(request, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var sheet = new DbSheet
        {
            PhaseId = phaseId,
            SheetTypeId = request.SheetTypeId,
            IdentityKey = SheetIdentifierRules.IdentityKey(values),
        };
        foreach (var value in values)
        {
            sheet.IdentifierValues.Add(new DbSheetIdentifierValue { SheetTypeIdentifierId = value.Definition.Id, Value = value.Value });
        }

        db.Sheets.Add(sheet);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException(values.Count == 0
                ? "This phase already has a sheet of this type with no identifiers."
                : $"This phase already has a sheet of this type with {Describe(values)}.");
        }

        if (copyFrom is { } sourceId)
        {
            await copier.CopyAsync(sourceId, sheet.Id, cancellationToken);
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
        var definitions = await SheetTypeIdentifierQueries.ListDefinitionsAsync(db, sheetTypeId, cancellationToken);
        if (definitions.Any(definition => definition.IsRequired))
        {
            throw new InvalidRequestException("Sheets of this type need their identifiers. Create one from the phase's list of sheets.");
        }

        var sheet = new DbSheet { PhaseId = phaseId, SheetTypeId = sheetTypeId };
        db.Sheets.Add(sheet);
        if (await db.TrySaveChangesAsync(cancellationToken))
        {
            return sheet.Id;
        }

        db.Detach(sheet);
        return await SheetQueries.FindIdAsync(db, phaseId, sheetTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The sheet could not be created, and nobody else created it either.");
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
                values.Add(new SheetIdentifierValue(definition, stored));
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
        if (!await PhaseQueries.HasSheetTypeAsync(db, phaseId, sheetTypeId, cancellationToken))
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

        var source = await SheetQueries.FindAsync(db, sourceId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sourceId} was not found.");
        if (source.SheetTypeId != request.SheetTypeId)
        {
            throw new InvalidRequestException("A sheet can only start as a copy of a sheet of the same type.");
        }

        return sourceId;
    }
}
