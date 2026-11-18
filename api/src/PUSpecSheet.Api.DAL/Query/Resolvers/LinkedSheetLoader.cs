using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.DAL.Caching;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Sheets;

namespace PUSpecSheet.Api.DAL.Query.Resolvers;

/// <summary>
/// Loads the published sheet that a link points at, as it stood when the sheet linking to it was published, from the
/// cache when it is there. It isn't trimmed to the tables the current user has permission to, as the link is the
/// linking sheet's to resolve.
/// </summary>
internal class LinkedSheetLoader
{
    private readonly SheetReader _sheetReader;
    private readonly ISpecSheetCache _specSheetCache;
    private readonly Dictionary<(int SheetTypeId, int PhaseId, DateTime PublishedBy), Task<Sheet?>> _loaded = [];

    public LinkedSheetLoader(SheetReader sheetReader, ISpecSheetCache specSheetCache)
    {
        ArgumentNullException.ThrowIfNull(sheetReader, nameof(sheetReader));
        ArgumentNullException.ThrowIfNull(specSheetCache, nameof(specSheetCache));

        _sheetReader = sheetReader;
        _specSheetCache = specSheetCache;
    }

    // A sheet is loaded once per request, however many of its cells and tables are linked to.
    public Task<Sheet?> LoadAsync(int sheetTypeId, int phaseId, DateTime publishedBy, CancellationToken cancellationToken)
    {
        var key = (sheetTypeId, phaseId, publishedBy);
        if (!_loaded.TryGetValue(key, out var loading))
        {
            _loaded[key] = loading = ReadAsync(sheetTypeId, phaseId, publishedBy, cancellationToken);
        }

        return loading;
    }

    private async Task<Sheet?> ReadAsync(int sheetTypeId, int phaseId, DateTime publishedBy, CancellationToken cancellationToken)
    {
        var location = await _sheetReader.FindVersionAtAsync(phaseId, sheetTypeId, publishedBy, cancellationToken);
        if (location is null)
        {
            return null;
        }

        if (location.SpecSheetId != 0 && await _specSheetCache.GetAsync(location.SpecSheetId, cancellationToken) is { } cached)
        {
            return cached;
        }

        var sheet = await _sheetReader.ReadPublishedAsync(location.SheetId, location.VersionNumber, cancellationToken);
        if (sheet is not null)
        {
            await _specSheetCache.SetAsync(sheet, cancellationToken);
        }

        return sheet;
    }
}
