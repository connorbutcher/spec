using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Domain.Values.PropertyValues;
using PUSpecSheet.Abstractions.Requests.Sheets;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Sheets;
using PUSpecSheet.Api.DAL.DataAccess.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Creates the sheet for a phase and sheet type that has none: its drafts are made to show the sheet copied from, or
/// nothing, with the property values given, all checked out to the user creating it until they submit or undo them. A
/// sheet that exists already is edited in place rather than created again.
/// </summary>
internal class SheetDraftCreator
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly SheetReader _sheetReader;
    private readonly ITypeDataAccessService _typeDataAccessService;

    public SheetDraftCreator(PUSpecSheetDbContext dbContext,
        SheetReader sheetReader,
        ITypeDataAccessService typeDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(sheetReader, nameof(sheetReader));
        ArgumentNullException.ThrowIfNull(typeDataAccessService, nameof(typeDataAccessService));

        _dbContext = dbContext;
        _sheetReader = sheetReader;
        _typeDataAccessService = typeDataAccessService;
    }

    /// <summary>
    /// Creates the sheet and returns its id.
    /// </summary>
    public async Task<int> CreateAsync(CreateSheetRequest request, User creatingUser, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        ArgumentNullException.ThrowIfNull(creatingUser, nameof(creatingUser));

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var sheet = await AddSheetAsync(request, cancellationToken);

        var sourceTables = await ReadCopyFromTablesAsync(request, cancellationToken);
        var target = await _sheetReader.ReadDraftAsync(sheet.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Sheet with Id {sheet.Id} not found after adding it.");
        await new SheetDraftCopier(_dbContext, creatingUser, sheet.Id).CopyAsync(target, sourceTables, cancellationToken);

        var propertyValues = request.PropertyValues.ToDictionary(pv => pv.SpecSheetTypePropertyId, pv => pv.PropertyValue?.ToString());
        await new SheetPropertyDrafts(_dbContext, creatingUser, sheet.Id).SetAsync(propertyValues, cancellationToken);

        sheet.LegacySourceHash = null;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return sheet.Id;
    }

    private async Task<DbSheet> AddSheetAsync(CreateSheetRequest request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Sheets
            .AnyAsync(s => s.SpecSheetPhaseId == request.SpecSheetPhaseId && s.SpecSheetTypeId == request.SpecSheetTypeId, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"A sheet already exists for SpecSheetPhaseId {request.SpecSheetPhaseId} and SpecSheetTypeId {request.SpecSheetTypeId}. Edit it rather than creating it again.");
        }

        var sheet = new DbSheet
        {
            Id = 0,
            SpecSheetPhaseId = request.SpecSheetPhaseId,
            SpecSheetTypeId = request.SpecSheetTypeId,
            ReferenceId = Guid.NewGuid(),
            CreatedDateTime = DateTime.UtcNow,
            LegacySourceHash = null,
            MigratedDateTime = null
        };

        await _dbContext.Sheets.AddAsync(sheet, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return sheet;
    }
    private async Task<IReadOnlyCollection<SheetTable>> ReadCopyFromTablesAsync(CreateSheetRequest request, CancellationToken cancellationToken)
    {
        var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(request.SpecSheetTypeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Spec Sheet Type with Id {request.SpecSheetTypeId} not found.");

        var copyFromProperty = specSheetType.Properties.FirstOrDefault(p => p.ValueControlType == "Copy From");
        var copyFromValue = copyFromProperty is null
            ? null
            : request.PropertyValues.FirstOrDefault(pv => pv.SpecSheetTypePropertyId == copyFromProperty.Id)?.PropertyValue;
        if (copyFromValue is not IntPropertyValue { Value: { } copyFromId })
        {
            return [];
        }

        var location = await _sheetReader.FindLocationAsync(copyFromId, cancellationToken);
        var source = location is { IsDraft: false }
            ? await _sheetReader.ReadPublishedAsync(location.SheetId, location.VersionNumber, cancellationToken)
            : null;
        if (source is null)
        {
            throw new InvalidOperationException($"Spec Sheet with Id {copyFromId} isn't a published version, so it can't be copied from.");
        }

        return [.. source.Tables];
    }
}