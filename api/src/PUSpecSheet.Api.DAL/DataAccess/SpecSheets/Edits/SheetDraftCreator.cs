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
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Creates a sheet's draft. The SpecSheets row is the draft's header, and the sheet's
/// drafts are made to show the sheet copied from, or nothing, with the property values given, as a new copy did. The
/// property values are kept on the header too, where the lists of spec sheets read them; they aren't changed afterwards.
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

    public async Task<int> CreateAsync(CreateSheetRequest request, User creatingUser, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        ArgumentNullException.ThrowIfNull(creatingUser, nameof(creatingUser));

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await EnsureNoDraftHeaderAsync(request, cancellationToken);
        var sheet = await FindOrAddSheetAsync(request, cancellationToken);
        await EnsureNoDraftsAsync(sheet.Id, cancellationToken);

        var header = new DbSpecSheet
        {
            Id = 0,
            SpecSheetPhaseId = request.SpecSheetPhaseId,
            SpecSheetTypeId = request.SpecSheetTypeId,
            CreatedByUserId = creatingUser.Id,
            CreatedDateTime = DateTime.UtcNow,
            IsPublished = false,
            HasBeenMigrated = false,
            PropertyValues = [.. request.PropertyValues.Select(pv => new DbSpecSheetPropertyValue
            {
                Id = 0,
                SpecSheetId = 0,
                SpecSheetTypePropertyId = pv.SpecSheetTypePropertyId,
                PropertyValue = pv.PropertyValue?.ToString()
            })]
        };

        await _dbContext.SpecSheets.AddAsync(header, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var sourceTables = await ReadCopyFromTablesAsync(request, cancellationToken);
        var target = await _sheetReader.ReadDraftAsync(sheet.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Sheet with Id {sheet.Id} not found after adding it.");
        await new SheetDraftCopier(_dbContext, creatingUser, sheet.Id).CopyAsync(target, sourceTables, cancellationToken);

        var propertyValues = request.PropertyValues.ToDictionary(pv => pv.SpecSheetTypePropertyId, pv => pv.PropertyValue?.ToString());
        await new SheetPropertyDrafts(_dbContext, creatingUser, sheet.Id).SetAsync(propertyValues, cancellationToken);

        sheet.LegacySourceHash = null;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return header.Id;
    }

    private async Task EnsureNoDraftHeaderAsync(CreateSheetRequest request, CancellationToken cancellationToken)
    {
        var existingDraftId = await _dbContext.SpecSheets
            .AsNoTracking()
            .Where(ss => ss.SpecSheetPhaseId == request.SpecSheetPhaseId && ss.SpecSheetTypeId == request.SpecSheetTypeId && !ss.IsPublished)
            .Select(ss => (int?)ss.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existingDraftId.HasValue)
        {
            throw new InvalidOperationException($"A draft spec sheet with Id {existingDraftId} already exists for SpecSheetPhaseId {request.SpecSheetPhaseId} and SpecSheetTypeId {request.SpecSheetTypeId}. Please publish or delete the existing draft before creating a new one.");
        }
    }

    private async Task<DbSheet> FindOrAddSheetAsync(CreateSheetRequest request, CancellationToken cancellationToken)
    {
        var sheet = await _dbContext.Sheets
            .FirstOrDefaultAsync(s => s.SpecSheetPhaseId == request.SpecSheetPhaseId && s.SpecSheetTypeId == request.SpecSheetTypeId, cancellationToken);
        if (sheet is not null)
        {
            return sheet;
        }

        sheet = new DbSheet
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

    // Drafts without a header are left over from somewhere else; they aren't the new draft's to take over.
    private async Task EnsureNoDraftsAsync(int sheetId, CancellationToken cancellationToken)
    {
        var hasDrafts = await _dbContext.SheetTableRevisions.AnyAsync(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft, cancellationToken)
            || await _dbContext.SheetTableGroupRevisions.AnyAsync(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft, cancellationToken)
            || await _dbContext.SheetTableRowRevisions.AnyAsync(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft, cancellationToken)
            || await _dbContext.SheetPropertyRevisions.AnyAsync(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft, cancellationToken);
        if (hasDrafts)
        {
            throw new InvalidOperationException($"Sheet with Id {sheetId} already has drafts without a draft spec sheet.");
        }
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