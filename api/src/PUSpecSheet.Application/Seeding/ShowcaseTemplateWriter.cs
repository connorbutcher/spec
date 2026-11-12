using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.SheetTypes;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Database.Entities.Templates;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Writes showcase templates and their versions straight to the database, as the template designer would
/// leave them: a new version is a whole new layout, and older versions stay as the sheets built from them
/// had them.
/// </summary>
public sealed class ShowcaseTemplateWriter
{
    private readonly ISheetTypeRepository sheetTypes;
    private readonly ITableTemplateRepository templates;
    private readonly IUnitOfWork unitOfWork;

    public ShowcaseTemplateWriter(ISheetTypeRepository sheetTypes, ITableTemplateRepository templates, IUnitOfWork unitOfWork)
    {
        this.sheetTypes = sheetTypes;
        this.templates = templates;
        this.unitOfWork = unitOfWork;
    }

    /// <summary>Adds <paramref name="template"/> with its version 1, at the end of its sheet type's list, and returns its id.</summary>
    public async Task<int> CreateAsync(IShowcaseTemplate template, ShowcaseCellTypeCatalog cellTypes, CancellationToken cancellationToken)
    {
        var sheetTypeId = await sheetTypes.GetIdByNameAsync(template.SheetTypeName, cancellationToken);
        var order = await templates.GetLastDisplayOrderAsync(sheetTypeId, cancellationToken) ?? 0;

        var entity = new TableTemplate { SheetTypeId = sheetTypeId, Name = template.Name, DisplayOrder = order + 1 };
        entity.Versions.Add(Lay(template, cellTypes, 1));
        templates.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        unitOfWork.ClearTracking();
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
        templates.AddVersion(version);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        unitOfWork.ClearTracking();
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
