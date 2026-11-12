using Microsoft.Extensions.Logging;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Development-only sample data that shows off the whole app: templates for the Specification, PFKs and
/// Parts sheet types with several versions each, every cell kind, addable sections nested three deep with
/// minimums and maximums, column blocks, per-cell settings and styles and lookup keys, and sheets for V6
/// and A2 to A5 with published history by three people, unpublished changes and the locks they hold.
/// <para>
/// <b>It clears the database first.</b> Every sheet, template and user-made cell type is deleted (see
/// <see cref="SampleDataQueries"/>) and the showcase is built in their place. That happens once: the
/// showcase's last publish carries <see cref="CompletedNote"/>, and while a version with that note exists
/// nothing is touched again, so work done on top of the showcase is kept. Set <c>SampleData:Rebuild</c> to
/// true to clear and rebuild it on the next start regardless. A failure is logged instead of stopping the
/// API, and as the note is only written at the end, the next start tries again from a clean database.
/// </para>
/// </summary>
public sealed partial class ShowcaseDataSeeder
{
    /// <summary>The note on the showcase's last publish, which marks it as built.</summary>
    public const string CompletedNote = "Issued for V6 build sign-off";

    private readonly PuSpecSheetDbContext db;
    private readonly ShowcaseCellTypes cellTypes;
    private readonly ShowcasePhases phases;
    private readonly ShowcaseTemplateWriter templates;
    private readonly SpecificationSheetSeeder specification;
    private readonly PfkSheetSeeder pfks;
    private readonly PartsSheetSeeder parts;
    private readonly ShowcaseUsers users;
    private readonly ILogger<ShowcaseDataSeeder> logger;

    public ShowcaseDataSeeder(
        PuSpecSheetDbContext db,
        ShowcaseCellTypes cellTypes,
        ShowcasePhases phases,
        ShowcaseTemplateWriter templates,
        SpecificationSheetSeeder specification,
        PfkSheetSeeder pfks,
        PartsSheetSeeder parts,
        ShowcaseUsers users,
        ILogger<ShowcaseDataSeeder> logger)
    {
        this.db = db;
        this.cellTypes = cellTypes;
        this.phases = phases;
        this.templates = templates;
        this.specification = specification;
        this.pfks = pfks;
        this.parts = parts;
        this.users = users;
        this.logger = logger;
    }

    public async Task SeedAsync(bool rebuild, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!rebuild && await SheetQueries.AnyVersionWithNoteAsync(db, CompletedNote, cancellationToken))
            {
                return;
            }

            LogBuilding(logger);
            await SampleDataQueries.ClearAsync(db, cancellationToken);
            await BuildAsync(cancellationToken);
            LogBuilt(logger);
        }
        catch (Exception exception)
        {
            LogFailed(logger, exception);
        }
        finally
        {
            users.Reset();
        }
    }

    private async Task BuildAsync(CancellationToken cancellationToken)
    {
        var catalog = await cellTypes.EnsureAsync(cancellationToken);
        var phaseIds = await phases.EnsureAsync(cancellationToken);

        var specificationTemplate = new SpecificationTemplate();
        var gaugeTemplate = new GaugeTemplate();
        var registerTemplate = new PfkRegisterTemplate();
        var partsTemplate = new PartsMatrixTemplate();
        var templateIds = new Dictionary<string, int>();
        IShowcaseTemplate[] all = [specificationTemplate, gaugeTemplate, registerTemplate, partsTemplate];
        foreach (var template in all)
        {
            templateIds[template.Name] = await templates.CreateAsync(template, catalog, cancellationToken);
        }

        var context = new ShowcaseContext(catalog, phaseIds, templateIds);

        // V6 is built from version 1 of each template.
        await specification.SeedV6Async(context, cancellationToken);
        await pfks.SeedV6Async(context, cancellationToken);
        await parts.SeedV6Async(context, cancellationToken);

        // Version 2 of three of them, which the A phases are built from.
        await templates.AddVersionAsync(templateIds[specificationTemplate.Name], specificationTemplate, catalog, 2, cancellationToken);
        await templates.AddVersionAsync(templateIds[registerTemplate.Name], registerTemplate, catalog, 2, cancellationToken);
        await templates.AddVersionAsync(templateIds[partsTemplate.Name], partsTemplate, catalog, 2, cancellationToken);
        await specification.SeedPhasesAsync(context, cancellationToken);
        await pfks.SeedPhasesAsync(context, cancellationToken);
        await parts.SeedPhasesAsync(context, cancellationToken);

        // A third register version, still open in the template designer: no sheet uses it.
        await templates.AddVersionAsync(templateIds[registerTemplate.Name], registerTemplate, catalog, 3, cancellationToken);

        // Last, so the note is only there once everything else is.
        await specification.IssueV6Async(CompletedNote, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Clearing the sheets and templates and building the showcase sample data.")]
    private static partial void LogBuilding(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "The showcase sample data is built.")]
    private static partial void LogBuilt(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The showcase sample data couldn't be built; it will be tried again on the next start.")]
    private static partial void LogFailed(ILogger logger, Exception exception);
}
