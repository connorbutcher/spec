using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Api.DAL.Caching;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Query.Requests;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Updates the external references of sheets of a type edited on the revisions. References are only revised by
/// versions, but other systems change them without a publish, so a change is made to the sheet's latest version: a
/// reference new or changed in it has a revision of that version (changed in place if its current revision is of that
/// version already, otherwise superseding it as of the version's moment), and one removed from it has a removing
/// revision, or none at all if it was only added in that version.
/// </summary>
internal class SheetExternalReferences
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly SheetEditSwitch _editSwitch;
    private readonly ISpecSheetCache _specSheetCache;
    private readonly ILogger<SheetExternalReferences> _logger;

    public SheetExternalReferences(PUSpecSheetDbContext dbContext, SheetEditSwitch editSwitch, ISpecSheetCache specSheetCache, ILogger<SheetExternalReferences> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(editSwitch, nameof(editSwitch));
        ArgumentNullException.ThrowIfNull(specSheetCache, nameof(specSheetCache));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbContext = dbContext;
        _editSwitch = editSwitch;
        _specSheetCache = specSheetCache;
        _logger = logger;
    }

    /// <summary>
    /// Updates the references of the spec sheets that are the latest version of a sheet whose type is edited on the
    /// revisions, as the copies' update does: a reference not given for a spec sheet and system is removed. Returns the
    /// references for any other spec sheet, which are the copies' to update.
    /// </summary>
    public async Task<List<ExternalSystemReferenceRequest>> UpdateAsync(ICollection<ExternalSystemReferenceRequest> references, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(references, nameof(references));

        var latestVersions = await FindLatestVersionsAsync(references.Select(r => r.PUSpecSheetId).Distinct().ToList(), cancellationToken);
        var rest = references.Where(r => !latestVersions.ContainsKey(r.PUSpecSheetId)).ToList();
        if (latestVersions.Count == 0)
        {
            return rest;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var sheet in references.Where(r => latestVersions.ContainsKey(r.PUSpecSheetId)).GroupBy(r => latestVersions[r.PUSpecSheetId]))
        {
            await UpdateSheetAsync(sheet.Key, [.. sheet], cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        foreach (var specSheetId in latestVersions.Keys)
        {
            await _specSheetCache.RemoveAsync(specSheetId, cancellationToken);
        }

        _logger.LogInformation("Updated the system references of {SpecSheetCount} sheet(s) on the revisions", latestVersions.Count);
        return rest;
    }

    /// <summary>
    /// Removes the references with the external id from the sheets that have them, as of their latest versions.
    /// </summary>
    public async Task DeleteByExternalIdAsync(int externalId, CancellationToken cancellationToken)
    {
        var items = await _dbContext.SheetExternalReferences
            .Where(item => item.Revisions.Any(r => r.SupersededDateTime == null && !r.IsDeleted && r.ExternalId == externalId))
            .Include(item => item.Revisions.Where(r => r.SupersededDateTime == null))
            .ToListAsync(cancellationToken);
        if (items.Count == 0)
        {
            return;
        }

        var sheetIds = items.Select(item => item.SheetId).Distinct().ToList();
        var versions = await _dbContext.SheetVersions
            .AsNoTracking()
            .Where(version => sheetIds.Contains(version.SheetId))
            .ToListAsync(cancellationToken);
        var latest = versions
            .GroupBy(version => version.SheetId)
            .ToDictionary(group => group.Key, group => group.MaxBy(version => version.VersionNumber)!);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        foreach (var item in items)
        {
            await RemoveAsync(item, latest[item.SheetId], cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        foreach (var specSheetId in latest.Values.Select(version => version.SourceSpecSheetId).OfType<int>())
        {
            await _specSheetCache.RemoveAsync(specSheetId, cancellationToken);
        }

        _logger.LogInformation("Deleted {ReferenceCount} system references for ExternalId {ExternalId} on the revisions", items.Count, externalId);
    }

    // For each spec sheet that is a version of a sheet of a type edited on the revisions: the sheet's latest version. A
    // spec sheet that is an earlier version isn't one to change, as its references are shared with the later ones.
    private async Task<Dictionary<int, DbSheetVersion>> FindLatestVersionsAsync(List<int> specSheetIds, CancellationToken cancellationToken)
    {
        var versions = await _dbContext.SheetVersions
            .AsNoTracking()
            .Where(version => version.SourceSpecSheetId != null && specSheetIds.Contains(version.SourceSpecSheetId.Value))
            .ToListAsync(cancellationToken);
        if (versions.Count == 0)
        {
            return [];
        }

        var sheetIds = versions.Select(version => version.SheetId).Distinct().ToList();
        var sheetTypeIds = await _dbContext.Sheets
            .AsNoTracking()
            .Where(sheet => sheetIds.Contains(sheet.Id))
            .ToDictionaryAsync(sheet => sheet.Id, sheet => sheet.SpecSheetTypeId, cancellationToken);
        var latestNumbers = await _dbContext.SheetVersions
            .AsNoTracking()
            .Where(version => sheetIds.Contains(version.SheetId))
            .GroupBy(version => version.SheetId)
            .Select(group => new { SheetId = group.Key, VersionNumber = group.Max(version => version.VersionNumber) })
            .ToDictionaryAsync(latest => latest.SheetId, latest => latest.VersionNumber, cancellationToken);

        var result = new Dictionary<int, DbSheetVersion>();
        foreach (var version in versions.Where(version => _editSwitch.IsOnRevisions(sheetTypeIds[version.SheetId])))
        {
            if (version.VersionNumber != latestNumbers[version.SheetId])
            {
                throw new NotSupportedException($"Spec Sheet with Id {version.SourceSpecSheetId} isn't the latest version of its sheet, so its system references can't be changed.");
            }

            result[version.SourceSpecSheetId!.Value] = version;
        }

        return result;
    }

    private async Task UpdateSheetAsync(DbSheetVersion latest, IReadOnlyCollection<ExternalSystemReferenceRequest> references, CancellationToken cancellationToken)
    {
        var items = await _dbContext.SheetExternalReferences
            .Where(item => item.SheetId == latest.SheetId)
            .Include(item => item.Revisions.Where(r => r.SupersededDateTime == null))
            .ToListAsync(cancellationToken);

        foreach (var system in references.GroupBy(r => r.SystemName))
        {
            var requestedIds = system.Select(r => r.ReferenceId).ToHashSet();

            foreach (var reference in system)
            {
                await SetAsync(items, latest, reference, cancellationToken);
            }

            var gone = items
                .Where(item => !requestedIds.Contains(item.ReferenceId)
                    && Current(item) is { IsDeleted: false } current
                    && current.SystemName == system.Key)
                .ToList();
            foreach (var item in gone)
            {
                await RemoveAsync(item, latest, cancellationToken);
                if (!_dbContext.Entry(item).State.HasFlag(EntityState.Deleted))
                {
                    continue;
                }

                items.Remove(item);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SetAsync(List<DbSheetExternalReference> items, DbSheetVersion latest, ExternalSystemReferenceRequest reference, CancellationToken cancellationToken)
    {
        var type = Enum.GetName(reference.Type)?.ToLowerInvariant() ?? string.Empty;
        var item = items.FirstOrDefault(i => i.ReferenceId == reference.ReferenceId);

        if (item is null)
        {
            item = new DbSheetExternalReference
            {
                Id = 0,
                SheetId = latest.SheetId,
                ReferenceId = reference.ReferenceId,
                CreatedDateTime = DateTime.UtcNow,
                Revisions = [NewRevision(latest, null, 1, reference, type, false)]
            };

            await _dbContext.SheetExternalReferences.AddAsync(item, cancellationToken);
            items.Add(item);
            return;
        }

        var current = Current(item);
        if (current.SheetVersionId == latest.Id)
        {
            _dbContext.Entry(current).CurrentValues.SetValues(new
            {
                reference.ExternalId,
                reference.SystemName,
                reference.OwnerName,
                reference.Url,
                Type = type,
                reference.Grouping,
                reference.SystemIconUrl,
                reference.Description,
                IsDeleted = false
            });
            return;
        }

        // Saved before the new revision, as an item has one current revision at a time.
        current.SupersededDateTime = latest.PublishedDateTime;
        await _dbContext.SaveChangesAsync(cancellationToken);
        item.Revisions.Add(NewRevision(latest, item, current.RevisionNumber + 1, reference, type, false));
    }

    private async Task RemoveAsync(DbSheetExternalReference item, DbSheetVersion latest, CancellationToken cancellationToken)
    {
        var current = Current(item);
        if (current.SheetVersionId == latest.Id)
        {
            if (current.RevisionNumber == 1)
            {
                _dbContext.SheetExternalReferences.Remove(item);
            }
            else
            {
                _dbContext.Entry(current).CurrentValues.SetValues(new { IsDeleted = true });
            }

            return;
        }

        current.SupersededDateTime = latest.PublishedDateTime;
        await _dbContext.SaveChangesAsync(cancellationToken);
        item.Revisions.Add(new DbSheetExternalReferenceRevision
        {
            Id = 0,
            SheetExternalReferenceId = item.Id,
            SheetId = item.SheetId,
            SheetVersionId = latest.Id,
            RevisionNumber = current.RevisionNumber + 1,
            ExternalId = current.ExternalId,
            SystemName = current.SystemName,
            OwnerName = current.OwnerName,
            Url = current.Url,
            Type = current.Type,
            Grouping = current.Grouping,
            SystemIconUrl = current.SystemIconUrl,
            Description = current.Description,
            IsDeleted = true,
            CreatedDateTime = DateTime.UtcNow,
            PublishedDateTime = latest.PublishedDateTime,
            SupersededDateTime = null
        });
    }

    private static DbSheetExternalReferenceRevision Current(DbSheetExternalReference item)
    {
        return item.Revisions.Single(revision => revision.SupersededDateTime == null);
    }

    private static DbSheetExternalReferenceRevision NewRevision(DbSheetVersion latest, DbSheetExternalReference? item, int revisionNumber, ExternalSystemReferenceRequest reference, string type, bool isDeleted)
    {
        return new DbSheetExternalReferenceRevision
        {
            Id = 0,
            SheetExternalReferenceId = item?.Id ?? 0,
            SheetId = latest.SheetId,
            SheetVersionId = latest.Id,
            RevisionNumber = revisionNumber,
            ExternalId = reference.ExternalId,
            SystemName = reference.SystemName,
            OwnerName = reference.OwnerName,
            Url = reference.Url,
            Type = type,
            Grouping = reference.Grouping,
            SystemIconUrl = reference.SystemIconUrl,
            Description = reference.Description,
            IsDeleted = isDeleted,
            CreatedDateTime = DateTime.UtcNow,
            PublishedDateTime = latest.PublishedDateTime,
            SupersededDateTime = null
        };
    }
}
