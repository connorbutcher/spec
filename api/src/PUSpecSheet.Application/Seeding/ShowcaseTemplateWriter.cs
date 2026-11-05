using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Writes showcase templates and their versions straight to the database, as the template designer would
/// leave them: a new version is a whole new layout, and older versions stay as the sheets built from them
/// had them.
/// </summary>
public sealed class ShowcaseTemplateWriter(PuSpecSheetDbContext db)
{
    /// <summary>Adds <paramref name="template"/> with its version 1, at the end of its sheet type's list, and returns its id.</summary>
    public async Task<int> CreateAsync(IShowcaseTemplate template, ShowcaseCellTypeCatalog cellTypes, CancellationToken cancellationToken)
    {
        var sheetTypeId = await db.SheetTypes
            .Where(sheetType => sheetType.Name == template.SheetTypeName)
            .Select(sheetType => sheetType.Id)
            .SingleAsync(cancellationToken);
        var order = await db.TableTemplates
            .Where(candidate => candidate.SheetTypeId == sheetTypeId)
            .MaxAsync(candidate => (int?)candidate.DisplayOrder, cancellationToken) ?? 0;

        var entity = new TableTemplate { SheetTypeId = sheetTypeId, Name = template.Name, DisplayOrder = order + 1 };
        entity.Versions.Add(Lay(template, cellTypes, 1));
        db.TableTemplates.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();
        return entity.Id;
    }

    /// <summary>Adds version <paramref name="versionNumber"/> of the template <paramref name="templateId"/>, which new tables then use.</summary>
    public async Task AddVersionAsync(
        int templateId,
        IShowcaseTemplate template,
        ShowcaseCellTypeCatalog cellTypes,
        int versionNumber,
        CancellationToken cancellationToken)
    {
        var version = Lay(template, cellTypes, versionNumber);
        version.TableTemplateId = templateId;
        db.TableTemplateVersions.Add(version);
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();
    }

    private static TableTemplateVersion Lay(IShowcaseTemplate template, ShowcaseCellTypeCatalog cellTypes, int versionNumber)
    {
        var version = new TableTemplateVersion
        {
            VersionNumber = versionNumber,
            Orientation = template.Orientation,
            StickyColumnCount = template.StickyColumnCount,
            CreatedAtUtc = DateTime.UtcNow,
        };
        template.Lay(new TemplateLayout(version, cellTypes), versionNumber);
        return version;
    }
}
