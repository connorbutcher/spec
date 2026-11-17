using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Says whether a spec sheet's draft is edited on its copy or on the revisions, from <see cref="SheetEditOptions"/>.
/// </summary>
internal class SheetEditSwitch
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly IOptionsMonitor<SheetEditOptions> _options;

    public SheetEditSwitch(PUSpecSheetDbContext dbContext, IOptionsMonitor<SheetEditOptions> options)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        _dbContext = dbContext;
        _options = options;
    }

    public bool IsOnRevisions(int specSheetTypeId)
    {
        return _options.CurrentValue.RevisionSheetTypeIds.Contains(specSheetTypeId);
    }

    public async Task<bool> IsOnRevisionsAsync(int specSheetId, CancellationToken cancellationToken)
    {
        var specSheetTypeId = await _dbContext.SpecSheets
            .AsNoTracking()
            .Where(s => s.Id == specSheetId)
            .Select(s => (int?)s.SpecSheetTypeId)
            .FirstOrDefaultAsync(cancellationToken);

        return specSheetTypeId.HasValue && IsOnRevisions(specSheetTypeId.Value);
    }

    /// <summary>
    /// The revisions' sheet that holds the drafts of the unpublished spec sheet with this id, or null when the spec sheet is
    /// published or its type is edited on its copies.
    /// </summary>
    public async Task<int?> FindDraftSheetIdAsync(int specSheetId, CancellationToken cancellationToken)
    {
        var specSheet = await _dbContext.SpecSheets
            .AsNoTracking()
            .Where(s => s.Id == specSheetId)
            .Select(s => new { s.IsPublished, s.SpecSheetTypeId, s.SpecSheetPhaseId })
            .FirstOrDefaultAsync(cancellationToken);
        if (specSheet is null || specSheet.IsPublished || !IsOnRevisions(specSheet.SpecSheetTypeId))
        {
            return null;
        }

        return await _dbContext.Sheets
            .AsNoTracking()
            .Where(s => s.SpecSheetTypeId == specSheet.SpecSheetTypeId && s.SpecSheetPhaseId == specSheet.SpecSheetPhaseId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Spec Sheet with Id {specSheetId} is edited on the revisions, but its sheet hasn't been migrated to them.");
    }
}