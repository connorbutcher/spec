using Microsoft.Extensions.Logging;
using PUSpecSheet.DataAccess.Seeding.ReferenceData;
using PUSpecSheet.DataAccess.Seeding.Sheets;
using PUSpecSheet.DataAccess.Seeding.Templates;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Seeding;

/// <summary>
/// Development-only sample data that shows off the whole app: templates for the Specification, PFKs and
/// Parts sheet types with several versions each (and an as-built record for Engine Specifications), every
/// cell kind, addable sections nested three deep with minimums and maximums, column blocks, per-cell
/// settings and styles and lookup keys, and sheets for V6 and A2 to A5 with published history by three
/// people, unpublished changes and the locks they hold, and several Engine Specifications sheets in V6
/// told apart by serial number, one of them copied from another.
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

    private readonly PuSpecSheetDbContext _db;
    private readonly ShowcaseCellTypes _cellTypes;
    private readonly ShowcasePhases _phases;
    private readonly ShowcaseTemplateWriter _templates;
    private readonly SpecificationSheetSeeder _specification;
    private readonly PfkSheetSeeder _pfks;
    private readonly PartsSheetSeeder _parts;
    private readonly EngineSpecificationSheetSeeder _engineSpecifications;
    private readonly ShowcaseUsers _users;
    private readonly ILogger<ShowcaseDataSeeder> _logger;

    public ShowcaseDataSeeder(
        PuSpecSheetDbContext db,
        ShowcaseCellTypes cellTypes,
        ShowcasePhases phases,
        ShowcaseTemplateWriter templates,
        SpecificationSheetSeeder specification,
        PfkSheetSeeder pfks,
        PartsSheetSeeder parts,
        EngineSpecificationSheetSeeder engineSpecifications,
        ShowcaseUsers users,
        ILogger<ShowcaseDataSeeder> logger)
    {
        _db = db;
        _cellTypes = cellTypes;
        _phases = phases;
        _templates = templates;
        _specification = specification;
        _pfks = pfks;
        _parts = parts;
        _engineSpecifications = engineSpecifications;
        _users = users;
        _logger = logger;
    }

    public async Task SeedAsync(bool rebuild, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!rebuild && await SheetQueries.AnyVersionWithNoteAsync(_db, CompletedNote, cancellationToken))
            {
                return;
            }

            LogBuilding(_logger);
            await SampleDataQueries.ClearAsync(_db, cancellationToken);
            await BuildAsync(cancellationToken);
            LogBuilt(_logger);
        }
        catch (Exception exception)
        {
            LogFailed(_logger, exception);
        }
        finally
        {
            _users.Reset();
        }
    }

    private async Task BuildAsync(CancellationToken cancellationToken)
    {
        var catalog = await _cellTypes.EnsureAsync(cancellationToken);
        var phaseIds = await _phases.EnsureAsync(cancellationToken);

        var specificationTemplate = new SpecificationTemplate();
        var gaugeTemplate = new GaugeTemplate();
        var registerTemplate = new PfkRegisterTemplate();
        var partsTemplate = new PartsMatrixTemplate();
        var buildRecordTemplate = new BuildRecordTemplate();
        var templateIds = new Dictionary<string, int>();
        IShowcaseTemplate[] all = [specificationTemplate, gaugeTemplate, registerTemplate, partsTemplate, buildRecordTemplate];
        foreach (var template in all)
        {
            templateIds[template.Name] = await _templates.CreateAsync(template, catalog, cancellationToken);
        }

        var context = new ShowcaseContext { CellTypes = catalog, PhaseIds = phaseIds, TemplateIds = templateIds };

        // V6 is built from version 1 of each template.
        await _specification.SeedV6Async(context, cancellationToken);
        await _pfks.SeedV6Async(context, cancellationToken);
        await _parts.SeedV6Async(context, cancellationToken);

        // V6's Engine Specifications: several sheets of the one type, one per engine serial number.
        await _engineSpecifications.SeedV6Async(context, cancellationToken);

        // Version 2 of three of them, which the A phases are built from.
        await _templates.AddVersionAsync(templateIds[specificationTemplate.Name], specificationTemplate, catalog, 2, cancellationToken);
        await _templates.AddVersionAsync(templateIds[registerTemplate.Name], registerTemplate, catalog, 2, cancellationToken);
        await _templates.AddVersionAsync(templateIds[partsTemplate.Name], partsTemplate, catalog, 2, cancellationToken);
        await _specification.SeedPhasesAsync(context, cancellationToken);
        await _pfks.SeedPhasesAsync(context, cancellationToken);
        await _parts.SeedPhasesAsync(context, cancellationToken);

        // A third register version, still open in the template designer: no sheet uses it.
        await _templates.AddVersionAsync(templateIds[registerTemplate.Name], registerTemplate, catalog, 3, cancellationToken);

        // Last, so the note is only there once everything else is.
        await _specification.IssueV6Async(CompletedNote, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Clearing the sheets and templates and building the showcase sample data.")]
    private static partial void LogBuilding(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "The showcase sample data is built.")]
    private static partial void LogBuilt(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The showcase sample data couldn't be built; it will be tried again on the next start.")]
    private static partial void LogFailed(ILogger logger, Exception exception);
}
