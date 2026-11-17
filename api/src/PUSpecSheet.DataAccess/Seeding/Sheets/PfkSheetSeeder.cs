using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Seeding.ReferenceData;
using PUSpecSheet.DataAccess.Seeding.Templates;

namespace PUSpecSheet.DataAccess.Seeding.Sheets;

public sealed class PfkSheetSeeder
{
    private static readonly (string Id, string Description, DateOnly CalibratedUntil, bool InService)[] _gauges =
    [
        ("G-101", "Air bore gauge, 82 mm", new DateOnly(2027, 3, 31), true),
        ("G-114", "Outside micrometer, 50 to 75 mm", new DateOnly(2027, 1, 15), true),
        ("G-120", "Height gauge, 300 mm", new DateOnly(2026, 12, 1), true),
        ("G-131", "Roundness tester", new DateOnly(2026, 11, 20), false),
    ];

    private static readonly PfkOperation[] _operations =
    [
        new PfkOperation
        {
            Name = "OP10 Rough and finish bore cylinders",
            Features =
            [
                ("PFK-101", "Cylinder bore diameter", "CC", 81.990m, 82.010m, 1, "G-101", "Pass", "Stop the line, check every block made since the last good part and quarantine any out of tolerance."),
                ("PFK-102", "Bore roundness", "SC", 0.000m, 0.008m, 10, "G-131", "Pass", null),
            ],
        },
        new PfkOperation
        {
            Name = "OP20 Line bore main bearings",
            Features =
            [
                ("PFK-201", "Main bearing bore", "CC", 63.995m, 64.013m, 5, "G-114", "Pass", "Re-measure on the CMM; segregate the batch if confirmed and inform quality."),
                ("PFK-202", "Deck height", "SC", 218.450m, 218.550m, 25, "G-120", "Fail", "Re-measure on the CMM; rework the deck face if more than 0.02 mm high."),
                ("PFK-203", "Bearing cap bolt hole depth", "Std", 24.500m, 25.500m, 50, "G-120", "Pass", null),
            ],
        },
    ];

    private static readonly PfkOperation _torque = new PfkOperation
    {
        Name = "OP30 Fit and torque main bearing caps",
        Features =
        [
            ("PFK-301", "Cap seating gap", "SC", 0.000m, 0.030m, 10, "G-114", "Pass", null),
            ("PFK-302", "Crankshaft end float", "CC", 0.080m, 0.250m, 1, "G-120", "N/A", "Strip and rebuild; replace the thrust washers."),
        ],
    };

    private readonly SheetEditorFactory _editors;
    private readonly ShowcaseUsers _users;

    public PfkSheetSeeder(SheetEditorFactory editors, ShowcaseUsers users)
    {
        _editors = editors;
        _users = users;
    }

    public async Task SeedV6Async(ShowcaseContext context, CancellationToken cancellationToken)
    {
        var editor = await BuildAsync(context, ShowcasePhases.V6, ShowcaseUsers.Developer, _operations, cancellationToken);
        await editor.PublishAsync("PFK register for V6", cancellationToken);

        // Version 2, by engineer3: the torque operation and the week 41 audit dates.
        await _users.ActAsAsync(ShowcaseUsers.EngineerThree, cancellationToken);
        await editor.RefreshAsync(cancellationToken);
        var register = Register(editor);
        await AddOperationAsync(editor, register, _torque, step: 0, cancellationToken);
        foreach (var operation in editor.Copies(register, null, PfkRegisterTemplate.Operation))
        {
            foreach (var feature in editor.Copies(register, operation.Id, PfkRegisterTemplate.Feature))
            {
                await editor.FillAsync(Row(feature), null, null, null, null, null, null, null, new DateOnly(2026, 10, 9));
            }
        }

        await editor.PublishAsync("OP30 added; audit dates from week 41", cancellationToken);

        // engineer2 starts the deck height re-check and leaves it unpublished, locking the row.
        await _users.ActAsAsync(ShowcaseUsers.EngineerTwo, cancellationToken);
        await editor.RefreshAsync(cancellationToken);
        var lineBore = editor.Copies(register, null, PfkRegisterTemplate.Operation)[1];
        var deck = editor.Copies(register, lineBore.Id, PfkRegisterTemplate.Feature)[1];
        await editor.FillAsync(Row(deck), null, null, null, null, null, 10m, null, new DateOnly(2026, 10, 23), "Pass");
    }

    public async Task SeedPhasesAsync(ShowcaseContext context, CancellationToken cancellationToken)
    {
        PfkOperation[] all = [.. _operations, _torque];

        var a2 = await BuildAsync(context, ShowcasePhases.A2, ShowcaseUsers.EngineerThree, all, cancellationToken);
        await AddNoteAsync(a2, "Bores now finished on the new honing cell; frequencies kept as V6 until capability is shown.", cancellationToken);
        await a2.PublishAsync("PFK register for A2", cancellationToken);

        var a3 = await BuildAsync(context, ShowcasePhases.A3, ShowcaseUsers.Developer, all, cancellationToken);
        await a3.PublishAsync("PFK register for A3", cancellationToken);

        // A4: published, then the developer has two changes of their own waiting: a new feature and a
        // relaxed frequency.
        var a4 = await BuildAsync(context, ShowcasePhases.A4, ShowcaseUsers.Developer, all, cancellationToken);
        await a4.PublishAsync("PFK register for A4", cancellationToken);
        var a4Register = Register(a4);
        var bore = a4.Copies(a4Register, null, PfkRegisterTemplate.Operation)[0];
        await a4.FillAsync(Row(a4.Copies(a4Register, bore.Id, PfkRegisterTemplate.Feature)[1]), null, null, null, null, null, 25m);
        var added = await a4.AddSectionAsync(a4Register, bore, PfkRegisterTemplate.Feature, cancellationToken);
        await a4.FillAsync(Row(added), "PFK-103", "Bore cylindricity", "SC", 0.000m, 0.010m, 10m, "G-131");

        // A5: engineer2 builds it, then the developer extends a gauge's calibration in a second version.
        var a5 = await BuildAsync(context, ShowcasePhases.A5, ShowcaseUsers.EngineerTwo, all, cancellationToken);
        await a5.PublishAsync("PFK register for A5", cancellationToken);
        await _users.ActAsAsync(ShowcaseUsers.Developer, cancellationToken);
        await a5.RefreshAsync(cancellationToken);
        var gauges = GaugeTable(a5);
        var roundness = a5.Copies(gauges, null, GaugeTemplate.Gauge)[3];
        await a5.FillAsync(Row(roundness), null, null, new DateOnly(2027, 11, 20), true);
        await a5.PublishAsync("G-131 recalibrated and back in service", cancellationToken);
    }

    private async Task<SheetEditor> BuildAsync(
        ShowcaseContext context,
        string phaseCode,
        string userName,
        PfkOperation[] operations,
        CancellationToken cancellationToken)
    {
        await _users.ActAsAsync(userName, cancellationToken);
        var editor = await _editors.OpenAsync(context.PhaseIds[phaseCode], ShowcasePhases.PfksSheetType, context.CellTypes, cancellationToken);
        var step = ShowcaseContext.Step(phaseCode);

        var gauges = await editor.AddTableAsync(context.TemplateIds[GaugeTemplate.TemplateName], "Gauges", cancellationToken);
        foreach (var (id, description, calibratedUntil, inService) in _gauges)
        {
            var gauge = await editor.AddSectionAsync(gauges, null, GaugeTemplate.Gauge, cancellationToken);
            await editor.FillAsync(Row(gauge), id, description, calibratedUntil.AddDays(step * 7), inService);
        }

        var register = await editor.AddTableAsync(context.TemplateIds[PfkRegisterTemplate.TemplateName], "PFK register", cancellationToken);
        foreach (var operation in operations)
        {
            await AddOperationAsync(editor, register, operation, step, cancellationToken);
        }

        return editor;
    }

    private static async Task AddOperationAsync(SheetEditor editor, int register, PfkOperation operation, int step, CancellationToken cancellationToken)
    {
        var existing = editor.Copies(register, null, PfkRegisterTemplate.Operation);
        var section = existing.Count == 1 && Row(existing[0]).Cells.All(cell => cell.Value is not string)
            ? existing[0]
            : await editor.AddSectionAsync(register, null, PfkRegisterTemplate.Operation, cancellationToken);
        await editor.FillAsync(Row(section), operation.Name);

        for (var index = 0; index < operation.Features.Length; index++)
        {
            var (number, feature, featureClass, lower, upper, every, gauge, result, reactionPlan) = operation.Features[index];
            var copy = await editor.SectionAsync(register, section, PfkRegisterTemplate.Feature, index, cancellationToken);
            var row = Row(copy);
            await LinkGaugeAsync(editor, row, cancellationToken);
            var drift = step * 0.001m;
            await editor.FillAsync(row, number, feature, featureClass, lower + drift, upper + drift, every, gauge, new DateOnly(2026, 9, 25).AddDays(step * 14), result);

            if (reactionPlan is not null)
            {
                var plan = await editor.AddSectionAsync(register, copy, PfkRegisterTemplate.ReactionPlan, cancellationToken);
                await editor.FillAsync(Row(plan), reactionPlan);
            }
        }
    }

    private static async Task LinkGaugeAsync(SheetEditor editor, SheetRow row, CancellationToken cancellationToken)
    {
        var templateCells = editor.Sheet.TemplateCells.ToDictionary(cell => cell.Id);
        var cell = row.Cells
            .Where(candidate => candidate.SheetColumnBlockId is null)
            .Single(candidate => templateCells[candidate.TemplateCellId].Column == 7);
        if (cell.Settings is not null)
        {
            return;
        }

        var gauges = editor.Table(GaugeTable(editor));
        var column = gauges.LinkableColumns.FirstOrDefault(candidate => candidate.Label == GaugeTemplate.GaugeIdCaption) ?? gauges.LinkableColumns[0];
        var settings = new LinkedDropdownInstanceSettings { SourceSheetTableId = gauges.Id, SourceTemplateCellId = column.TemplateCellId };
        await editor.ChooseSettingsAsync(row, cell, settings, cancellationToken);
    }

    private static async Task AddNoteAsync(SheetEditor editor, string text, CancellationToken cancellationToken)
    {
        var register = Register(editor);
        var bore = editor.Copies(register, null, PfkRegisterTemplate.Operation)[0];
        var note = await editor.AddSectionAsync(register, bore, PfkRegisterTemplate.OperationNote, cancellationToken);
        await editor.FillAsync(Row(note), text);
    }

    private static int Register(SheetEditor editor)
    {
        return editor.Sheet.Tables.Single(table => table.TemplateName == PfkRegisterTemplate.TemplateName).Id;
    }

    private static int GaugeTable(SheetEditor editor)
    {
        return editor.Sheet.Tables.Single(table => table.TemplateName == GaugeTemplate.TemplateName).Id;
    }

    private static SheetRow Row(SheetSection section)
    {
        return section.Rows.OrderBy(row => row.DisplayOrder).First();
    }
}
