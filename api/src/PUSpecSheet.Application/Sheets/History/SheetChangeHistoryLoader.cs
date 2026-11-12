using System.Globalization;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Sheets.Reading;

namespace PUSpecSheet.Application.Sheets.History;

/// <summary>
/// Reads what <see cref="SheetChangeHistoryCalculator"/> needs: every published revision of the sheet's
/// rows, sections and column blocks up to the moment viewed, and the values of those row revisions.
/// </summary>
/// <remarks>
/// Revisions are only published with a version, so what was published up to a moment is what was
/// published up to the last version at or before it, and the live view sees everything up to the newest
/// version. A history is therefore kept by sheet and version (see <see cref="SheetChangeHistoryCache"/>),
/// and every view that lands on the same version, live, by number or by date, shares it.
/// </remarks>
public sealed class SheetChangeHistoryLoader
{
    private readonly SheetRevisionRepositories revisions;
    private readonly IRowValueRepository values;
    private readonly SheetChangeHistoryCache cache;

    public SheetChangeHistoryLoader(SheetRevisionRepositories revisions, IRowValueRepository values, SheetChangeHistoryCache cache)
    {
        this.revisions = revisions;
        this.values = values;
        this.cache = cache;
    }

    public async Task<SheetChangeHistory> LoadAsync(
        SheetSnapshot sheet,
        DateTime? moment,
        CancellationToken cancellationToken)
    {
        // Changes come from published revisions, and those only exist once a version has been published.
        if (sheet.Versions.Count == 0)
        {
            return SheetChangeHistory.Empty;
        }

        var version = moment is { } at
            ? sheet.Versions.LastOrDefault(candidate => candidate.PublishedAtUtc <= at)
            : sheet.Versions[^1];
        if (version is null)
        {
            return await BuildAsync(sheet, moment, cancellationToken);
        }

        var key = string.Create(CultureInfo.InvariantCulture, $"{sheet.Sheet.Id}:{version.Id}");
        if (cache.TryGet(key, out var kept))
        {
            return kept;
        }

        var history = await BuildAsync(sheet, moment, cancellationToken);
        cache.Set(key, history, history.Count);
        return history;
    }

    private async Task<SheetChangeHistory> BuildAsync(SheetSnapshot sheet, DateTime? moment, CancellationToken cancellationToken)
    {
        // Found by the sheet they carry, like the sheet's own revisions (see SheetSnapshotLoader).
        var sheetId = sheet.Sheet.Id;

        var rowRevisions = await revisions.Rows.ListPublishedUpToAsync(sheetId, moment, cancellationToken);
        var sectionRevisions = await revisions.Sections.ListPublishedUpToAsync(sheetId, moment, cancellationToken);
        var columnBlockRevisions = await revisions.ColumnBlocks.ListPublishedUpToAsync(sheetId, moment, cancellationToken);
        var publishedValues = await values.LoadPublishedUpToAsync(sheetId, moment, cancellationToken);

        return SheetChangeHistoryCalculator.Calculate(
            rowRevisions,
            sectionRevisions,
            columnBlockRevisions,
            publishedValues,
            sheet.Versions.ToDictionary(version => version.Id, version => version.VersionNumber),
            sheet.Rows.ToDictionary(row => row.Id, row => row.SheetSectionId),
            sheet.Sections.ToDictionary(section => section.Id, section => section.ParentSheetSectionId));
    }
}
