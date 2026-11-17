using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;

namespace PUSpecSheet.Migrator.Legacy;

// What the legacy tables leave out and the target's design tables supply. The target's templates were set up by
// hand with the legacy ids (a legacy group template is the template section with the same id, and so on), so this
// is mostly checking ids and finding which template version they belong to.
internal sealed class DesignLookup
{
    private readonly Dictionary<int, int> _versionOfTemplate = [];
    private readonly HashSet<int> _ambiguousTemplates = [];
    private readonly Dictionary<int, int?> _sectionParents;
    private readonly HashSet<int> _templateRows;
    private readonly HashSet<int> _templateCells;

    private DesignLookup(Dictionary<int, int?> sectionParents, HashSet<int> templateRows, HashSet<int> templateCells)
    {
        _sectionParents = sectionParents;
        _templateRows = templateRows;
        _templateCells = templateCells;
    }

    public static async Task<DesignLookup> LoadAsync(PuSpecSheetDbContext target, SqlConnection legacy, CancellationToken cancellationToken)
    {
        var sections = await target.TemplateSections.AsNoTracking()
            .Select(section => new { section.Id, section.TableTemplateVersionId, section.ParentSectionId })
            .ToListAsync(cancellationToken);
        var versions = await target.TableTemplateVersions.AsNoTracking()
            .Select(version => new { version.Id, version.TableTemplateId, version.VersionNumber })
            .ToListAsync(cancellationToken);
        var lookup = new DesignLookup(
            sections.ToDictionary(section => section.Id, section => section.ParentSectionId),
            (await target.TemplateRows.AsNoTracking().Select(row => row.Id).ToListAsync(cancellationToken)).ToHashSet(),
            (await target.TemplateCells.AsNoTracking().Select(cell => cell.Id).ToListAsync(cancellationToken)).ToHashSet());

        var sectionVersions = sections.ToDictionary(section => section.Id, section => section.TableTemplateVersionId);
        var groupTemplates = new List<(int Id, int TableTemplateId)>();
        await using (var command = new SqlCommand(LegacySql.GroupTemplates, legacy))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                groupTemplates.Add((reader.GetInt32("Id"), Convert.ToInt32(reader["TableTemplateId"], System.Globalization.CultureInfo.InvariantCulture)));
            }
        }

        // A table template is the version holding its group templates. One with no groups takes its latest version.
        var latest = versions
            .GroupBy(version => version.TableTemplateId)
            .ToDictionary(group => group.Key, group => group.MaxBy(version => version.VersionNumber)!.Id);
        foreach (var (templateId, latestVersion) in latest)
        {
            lookup._versionOfTemplate[templateId] = latestVersion;
        }

        foreach (var template in groupTemplates.GroupBy(group => group.TableTemplateId))
        {
            var found = template
                .Where(group => sectionVersions.ContainsKey(group.Id))
                .Select(group => sectionVersions[group.Id])
                .Distinct()
                .ToList();
            if (found.Count == 1)
            {
                lookup._versionOfTemplate[template.Key] = found[0];
            }
            else if (found.Count > 1)
            {
                lookup._ambiguousTemplates.Add(template.Key);
            }
        }

        return lookup;
    }

    public int VersionOfTemplate(int legacyTableTemplateId)
    {
        if (_ambiguousTemplates.Contains(legacyTableTemplateId))
        {
            throw new MigrationException($"Table template {legacyTableTemplateId}'s group templates are spread over more than one template version in the target database.");
        }

        return _versionOfTemplate.TryGetValue(legacyTableTemplateId, out var versionId)
            ? versionId
            : throw new MigrationException($"Table template {legacyTableTemplateId} has no version in the target database.");
    }

    public bool HasSection(int templateSectionId)
    {
        return _sectionParents.ContainsKey(templateSectionId);
    }

    public int? ParentOf(int templateSectionId)
    {
        return _sectionParents.GetValueOrDefault(templateSectionId);
    }

    public bool HasRow(int templateRowId)
    {
        return _templateRows.Contains(templateRowId);
    }

    public bool HasCell(int templateCellId)
    {
        return _templateCells.Contains(templateCellId);
    }
}
