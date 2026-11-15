using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// What the legacy layout leaves out and the new database's design tables supply: which template version a
/// legacy table becomes a copy of (its template's latest), and the template orders legacy items are sorted by,
/// as legacy items have no order of their own.
/// </summary>
internal sealed class DesignLookup
{
    private readonly Dictionary<int, int> latestVersions;
    private readonly Dictionary<int, int> tableOrders;
    private readonly Dictionary<int, int> sectionOrders;
    private readonly Dictionary<int, int> rowOrders;

    private DesignLookup(
        Dictionary<int, int> latestVersions,
        Dictionary<int, int> tableOrders,
        Dictionary<int, int> sectionOrders,
        Dictionary<int, int> rowOrders)
    {
        this.latestVersions = latestVersions;
        this.tableOrders = tableOrders;
        this.sectionOrders = sectionOrders;
        this.rowOrders = rowOrders;
    }

    public static async Task<DesignLookup> LoadAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        var versions = await db.TableTemplateVersions.AsNoTracking()
            .Select(version => new { version.Id, version.TableTemplateId, version.VersionNumber })
            .ToListAsync(cancellationToken);
        var latestVersions = versions
            .GroupBy(version => version.TableTemplateId)
            .ToDictionary(group => group.Key, group => group.MaxBy(version => version.VersionNumber)!.Id);

        var tableOrders = await db.TableTemplates.AsNoTracking()
            .ToDictionaryAsync(template => template.Id, template => template.DisplayOrder, cancellationToken);
        var sectionOrders = await db.TemplateSections.AsNoTracking()
            .ToDictionaryAsync(section => section.Id, section => section.DisplayOrder, cancellationToken);
        var rowOrders = await db.TemplateRows.AsNoTracking()
            .ToDictionaryAsync(row => row.Id, row => row.DisplayOrder, cancellationToken);

        return new DesignLookup(latestVersions, tableOrders, sectionOrders, rowOrders);
    }

    /// <exception cref="MigrationException">The table template has no versions.</exception>
    public int LatestVersionOf(int tableTemplateId)
    {
        return latestVersions.TryGetValue(tableTemplateId, out var versionId)
            ? versionId
            : throw new MigrationException($"Table template {tableTemplateId} has no versions in the target database.");
    }

    public int TableOrder(int tableTemplateId)
    {
        return tableOrders.GetValueOrDefault(tableTemplateId);
    }

    public int SectionOrder(int templateSectionId)
    {
        return sectionOrders.GetValueOrDefault(templateSectionId);
    }

    public int RowOrder(int templateRowId)
    {
        return rowOrders.GetValueOrDefault(templateRowId);
    }
}
