using System.Globalization;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Seeding.ReferenceData;
using PUSpecSheet.DataAccess.Seeding.Templates;
using PUSpecSheet.DataAccess.Sheets;
using PUSpecSheet.DataAccess.SheetTypes;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Seeding.Sheets;

/// <summary>
/// Fills in V6's Engine Specifications sheets, the showcase's example of many sheets of one type in a phase:
/// one as-built record per engine, told apart by serial number, build date and configuration. Each is
/// created through the sheets service as the phase's sheet list does, so the values are checked and kept
/// unique the same way. PU6-0001 and PU6-0003 have two published versions each; PU6-0002 started as a copy of
/// PU6-0001 and was published with its own readings; PU6-0004, a copy of PU6-0003 with no build date yet, is
/// still the developer's unpublished drafts. No other phase or sheet type has more than one sheet.
/// </summary>
public sealed class EngineSpecificationSheetSeeder
{
    public const string TableTitle = "As-built measurements";

    private static readonly BuildStage[] BaseStages =
    [
        new(
            "Short block",
            [
                ("Cylinder bore", 82.003m, 81.990m, 82.010m),
                ("Main bearing bore", 64.005m, 63.995m, 64.013m),
                ("Crankshaft end float", 0.150m, 0.080m, 0.250m),
            ]),
        new(
            "Cylinder head",
            [
                ("Head face flatness", 0.012m, 0.000m, 0.050m),
                ("Valve lash, intake", 0.200m, 0.160m, 0.240m),
                ("Valve lash, exhaust", 0.300m, 0.260m, 0.340m),
            ]),
    ];

    private static readonly BuildStage Turbocharger = new(
        "Turbocharger",
        [
            ("Shaft axial play", 0.060m, 0.030m, 0.090m),
            ("Wastegate rod preload", 2.000m, 1.500m, 2.500m),
        ]);

    private readonly PuSpecSheetDbContext db;
    private readonly ISheetsService sheets;
    private readonly SheetEditorFactory editors;
    private readonly ShowcaseSheetIdentifiers identifiers;
    private readonly ShowcaseUsers users;
    private readonly ISheetTypeDataService sheetTypes;

    public EngineSpecificationSheetSeeder(
        PuSpecSheetDbContext db,
        ISheetsService sheets,
        SheetEditorFactory editors,
        ShowcaseSheetIdentifiers identifiers,
        ShowcaseUsers users,
        ISheetTypeDataService sheetTypes)
    {
        this.db = db;
        this.sheets = sheets;
        this.editors = editors;
        this.identifiers = identifiers;
        this.users = users;
        this.sheetTypes = sheetTypes;
    }

    /// <summary>The four V6 engines, built from version 1 of the as-built record template.</summary>
    public async Task SeedV6Async(ShowcaseContext context, CancellationToken cancellationToken)
    {
        var identifierIds = await identifiers.EnsureAsync(cancellationToken);
        var sheetTypeId = await sheetTypes.GetIdByNameAsync(ShowcasePhases.EngineSpecificationsSheetType, cancellationToken);
        var templateId = context.TemplateIds[BuildRecordTemplate.TemplateName];
        var target = new SheetTarget(context, sheetTypeId, identifierIds);

        // PU6-0001: recorded by the developer, then engineer2 re-measures a bore after re-honing.
        await users.ActAsAsync(ShowcaseUsers.Developer, cancellationToken);
        var first = await CreateAsync(target, new EngineBuild("PU6-0001", new DateOnly(2026, 9, 14), ShowcaseSheetIdentifiers.BaseConfiguration), null, cancellationToken);
        var firstTable = await AddRecordAsync(first, templateId, BaseStages, engine: 0, cancellationToken);
        await first.PublishAsync("As-built record for PU6-0001", cancellationToken);

        await users.ActAsAsync(ShowcaseUsers.EngineerTwo, cancellationToken);
        await first.RefreshAsync(cancellationToken);
        await first.FillAsync(MeasurementRow(first, firstTable, 0, 0), null, 82.006m, null, null, "Pass");
        await AddNoteAsync(first, firstTable, "Cylinder 3 re-honed after a bore scuff; all bores re-measured.", new DateOnly(2026, 9, 16), cancellationToken);
        await first.PublishAsync("Cylinder 3 re-honed; bores re-measured", cancellationToken);

        // PU6-0002: started by engineer3 as a copy of PU6-0001's record, with this engine's own readings.
        await users.ActAsAsync(ShowcaseUsers.EngineerThree, cancellationToken);
        var second = await CreateAsync(target, new EngineBuild("PU6-0002", new DateOnly(2026, 9, 21), ShowcaseSheetIdentifiers.BaseConfiguration), first.Sheet.Id, cancellationToken);
        var secondTable = second.Sheet.Tables[0].Id;
        await RecordReadingsAsync(second, secondTable, BaseStages, engine: 1);
        var copiedNote = second.Copies(secondTable, null, BuildRecordTemplate.BuildNote)[0];
        await second.FillAsync(Row(copiedNote, 0), "Built to PU6-0001's record. No rework.", new DateOnly(2026, 9, 21));
        await second.PublishAsync("As-built record for PU6-0002, from PU6-0001's", cancellationToken);

        // PU6-0003: the high-output build, with a turbocharger stage. Its second version fails the end float.
        await users.ActAsAsync(ShowcaseUsers.Developer, cancellationToken);
        var third = await CreateAsync(target, new EngineBuild("PU6-0003", new DateOnly(2026, 10, 2), ShowcaseSheetIdentifiers.HighOutputConfiguration), null, cancellationToken);
        var thirdTable = await AddRecordAsync(third, templateId, [.. BaseStages, Turbocharger], engine: 2, cancellationToken);
        await third.PublishAsync("As-built record for PU6-0003, high output", cancellationToken);

        await users.ActAsAsync(ShowcaseUsers.EngineerTwo, cancellationToken);
        await third.RefreshAsync(cancellationToken);
        await third.FillAsync(MeasurementRow(third, thirdTable, 0, 2), null, 0.262m, null, null, "Fail");
        await AddNoteAsync(third, thirdTable, "End float out of limit after the hot test. Thrust washers to be replaced.", new DateOnly(2026, 10, 6), cancellationToken);
        await third.PublishAsync("End float out of limit after hot test", cancellationToken);

        // PU6-0004: a hybrid engine, copied from PU6-0003 by the developer and not built yet, so no build date and
        // nothing published: the copy is still the developer's own drafts, which the app shows them, with the
        // turbocharger stage and PU6-0003's note taken out.
        await users.ActAsAsync(ShowcaseUsers.Developer, cancellationToken);
        var fourth = await CreateAsync(target, new EngineBuild("PU6-0004", null, ShowcaseSheetIdentifiers.HybridConfiguration), third.Sheet.Id, cancellationToken);
        var fourthTable = fourth.Sheet.Tables[0].Id;
        await fourth.RemoveSectionAsync(fourth.Copies(fourthTable, null, BuildRecordTemplate.Stage)[2], cancellationToken);
        await fourth.RemoveSectionAsync(fourth.Copies(fourthTable, null, BuildRecordTemplate.BuildNote)[0], cancellationToken);
        await fourth.FillAsync(MeasurementRow(fourth, fourthTable, 0, 2), null, 0.150m, null, null, "Pass");
    }

    /// <summary>Creates the engine's sheet in V6 with its identifier values, optionally as a copy of another, and opens it.</summary>
    private async Task<SheetEditor> CreateAsync(SheetTarget target, EngineBuild build, int? copyFromSheetId, CancellationToken cancellationToken)
    {
        List<SheetIdentifierValueRequest> values = [new(target.IdentifierIds[ShowcaseSheetIdentifiers.SerialNumber], build.SerialNumber)];
        if (build.BuildDate is { } date)
        {
            values.Add(new(target.IdentifierIds[ShowcaseSheetIdentifiers.BuildDate], date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }

        if (build.Configuration is { } configuration)
        {
            values.Add(new(target.IdentifierIds[ShowcaseSheetIdentifiers.Configuration], configuration));
        }

        var phaseId = target.Context.PhaseIds[ShowcasePhases.V6];
        var created = await sheets.CreateAsync(phaseId, new CreateSheetRequest(target.SheetTypeId, values, copyFromSheetId), cancellationToken);
        return await editors.OpenSheetAsync(created.Id, target.Context.CellTypes, cancellationToken);
    }

    /// <summary>Adds the as-built table with a stage for each of <paramref name="stages"/> and their readings, and returns its id.</summary>
    private static async Task<int> AddRecordAsync(SheetEditor editor, int templateId, BuildStage[] stages, int engine, CancellationToken cancellationToken)
    {
        var tableId = await editor.AddTableAsync(templateId, TableTitle, cancellationToken);
        for (var stageIndex = 0; stageIndex < stages.Length; stageIndex++)
        {
            // The table starts with one stage, so the first call fills that in.
            var stage = await editor.SectionAsync(tableId, null, BuildRecordTemplate.Stage, stageIndex, cancellationToken);
            await editor.FillAsync(Row(stage, 0), stages[stageIndex].Name);
            foreach (var (name, _, min, max) in stages[stageIndex].Measurements)
            {
                var measurement = await editor.AddSectionAsync(tableId, stage, BuildRecordTemplate.Measurement, cancellationToken);
                await editor.FillAsync(Row(measurement, 0), name, null, min, max);
            }
        }

        await RecordReadingsAsync(editor, tableId, stages, engine);
        return tableId;
    }

    /// <summary>Fills in this engine's measured values and results on a table that already has the stages' rows.</summary>
    private static async Task RecordReadingsAsync(SheetEditor editor, int tableId, BuildStage[] stages, int engine)
    {
        for (var stageIndex = 0; stageIndex < stages.Length; stageIndex++)
        {
            var measurements = stages[stageIndex].Measurements;
            for (var index = 0; index < measurements.Length; index++)
            {
                var (_, measured, min, max) = measurements[index];
                var reading = measured + (Spread(engine, stageIndex + index) * 0.001m);
                var result = reading >= min && reading <= max ? "Pass" : "Fail";
                await editor.FillAsync(MeasurementRow(editor, tableId, stageIndex, index), null, reading, null, null, result);
            }
        }
    }

    private static async Task AddNoteAsync(SheetEditor editor, int tableId, string text, DateOnly on, CancellationToken cancellationToken)
    {
        var note = await editor.AddSectionAsync(tableId, null, BuildRecordTemplate.BuildNote, cancellationToken);
        await editor.FillAsync(Row(note, 0), text, on);
    }

    /// <summary>How far, in microns, an engine's reading sits from the nominal reading: a small spread from -2 to 2.</summary>
    private static int Spread(int engine, int position)
    {
        return (((engine * 3) + (position * 7)) % 5) - 2;
    }

    private static SheetRowDto MeasurementRow(SheetEditor editor, int tableId, int stageIndex, int measurementIndex)
    {
        var stage = editor.Copies(tableId, null, BuildRecordTemplate.Stage)[stageIndex];
        return Row(editor.Copies(tableId, stage.Id, BuildRecordTemplate.Measurement)[measurementIndex], 0);
    }

    private static SheetRowDto Row(SheetSectionDto section, int index)
    {
        return section.Rows.OrderBy(row => row.DisplayOrder).ElementAt(index);
    }
}
