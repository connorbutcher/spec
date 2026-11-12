using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Fills in the Specification sheets. V6 has two tables built from version 1 of the template and a
/// history of three versions by two people, the last of which (<see cref="IssueV6Async"/>) is published at
/// the very end of the showcase. The A2 to A5 sheets are built from version 2, so they also have surface
/// finishes: A3 is left with rows locked by engineer2's unpublished changes, and A4 with the developer's own.
/// </summary>
public sealed class SpecificationSheetSeeder(SheetEditorFactory editors, ShowcaseUsers users)
{
    public const string ShortBlockTitle = "Short block";
    public const string HeadTitle = "Cylinder head and valve train";

    private static readonly SpecificationSystem[] ShortBlock =
    [
        new(
            "Cylinder block",
            [("Cylinder bore", 82.000m, 81.990m, 82.010m), ("Deck height", 218.500m, 218.450m, 218.550m), ("Main bearing bore", 64.000m, 63.995m, 64.013m)],
            [("Block casting", "EN-GJL-250 grey iron")],
            [("Main bearing cap bolt", 10, 50.0m, 90)],
            [("Cylinder bore, plateau honed", 0.40m)]),
        new(
            "Crankshaft",
            [("Main journal diameter", 59.980m, 59.971m, 59.990m), ("Crankpin diameter", 47.980m, 47.971m, 47.990m), ("End float", 0.150m, 0.080m, 0.250m)],
            [("Crankshaft", "38MnVS6 forged")],
            [("Flywheel bolt", 12, 80.0m, 45)],
            [("Main journals", 0.20m)]),
        new(
            "Pistons and connecting rods",
            [("Piston skirt diameter", 81.960m, 81.950m, 81.970m), ("Ring gap, top ring", 0.300m, 0.200m, 0.400m), ("Big end bore", 51.000m, 50.995m, 51.010m)],
            [("Piston", "EN AC-42100 T6"), ("Connecting rod", "42CrMo4 QT")],
            [("Big end cap bolt", 8, 25.0m, 60)],
            []),
    ];

    private static readonly SpecificationSystem[] Head =
    [
        new(
            "Cylinder head",
            [("Head face flatness", 0.000m, 0.000m, 0.050m), ("Combustion chamber depth", 9.500m, 9.400m, 9.600m)],
            [("Head casting", "EN AC-42100 T6")],
            [("Cylinder head bolt", 10, 40.0m, 180)],
            [("Head gasket face", 1.60m)]),
        new(
            "Valve train",
            [("Intake valve stem", 5.975m, 5.965m, 5.985m), ("Exhaust valve stem", 5.960m, 5.950m, 5.970m), ("Valve lash, intake", 0.200m, 0.170m, 0.230m)],
            [("Exhaust valve", "X45CrSi9-3 valve steel")],
            [("Camshaft cap bolt", 6, 10.0m, 0)],
            []),
    ];

    private static readonly SpecificationSystem Lubrication = new(
        "Lubrication",
        [("Oil gallery diameter", 12.000m, 11.900m, 12.100m), ("Oil pump rotor clearance", 0.080m, 0.040m, 0.120m)],
        [],
        [("Oil pump bolt", 8, 22.0m, 0)],
        []);

    private SheetEditor? v6;

    private int v6ShortBlockId;

    /// <summary>V6, built from version 1 of the template: two versions now, the third from <see cref="IssueV6Async"/>.</summary>
    public async Task SeedV6Async(ShowcaseContext context, CancellationToken cancellationToken)
    {
        await users.ActAsAsync(ShowcaseUsers.Developer, cancellationToken);
        var editor = await editors.OpenAsync(context.PhaseIds[ShowcasePhases.V6], ShowcasePhases.SpecificationSheetType, context.CellTypes, cancellationToken);
        var templateId = context.TemplateIds[SpecificationTemplate.TemplateName];

        v6ShortBlockId = await AddTableAsync(editor, templateId, ShortBlockTitle, ShortBlock, step: 0, cancellationToken);
        var headId = await AddTableAsync(editor, templateId, HeadTitle, Head, step: 0, cancellationToken);
        await editor.PublishAsync("First issue of the V6 specification", cancellationToken);

        // Version 2, by engineer2: wider valve lash after hot testing, every bore verified, lubrication
        // added and the exhaust valve material taken out until it is re-sourced.
        await users.ActAsAsync(ShowcaseUsers.EngineerTwo, cancellationToken);
        await editor.RefreshAsync(cancellationToken);
        var valveTrain = editor.Copies(headId, null, SpecificationTemplate.System)[1];
        var lash = editor.Copies(headId, valveTrain.Id, SpecificationTemplate.Dimension)[2];
        await editor.FillAsync(Row(lash, 0), null, null, 0.160m, 0.240m, "Under review");
        var block = editor.Copies(v6ShortBlockId, null, SpecificationTemplate.System)[0];
        foreach (var dimension in editor.Copies(v6ShortBlockId, block.Id, SpecificationTemplate.Dimension))
        {
            await editor.FillAsync(Row(dimension, 0), null, null, null, null, "Released", true);
        }

        await editor.RemoveSectionAsync(editor.Copies(headId, valveTrain.Id, SpecificationTemplate.Material)[0], cancellationToken);
        await AddSystemAsync(editor, v6ShortBlockId, Lubrication, step: 0, cancellationToken);
        await editor.PublishAsync("Valve lash widened after V6 hot tests; lubrication added", cancellationToken);
        v6 = editor;
    }

    /// <summary>The A phases, built from version 2 of the template, each a step further on from V6.</summary>
    public async Task SeedPhasesAsync(ShowcaseContext context, CancellationToken cancellationToken)
    {
        var templateId = context.TemplateIds[SpecificationTemplate.TemplateName];

        // A2: carried over by engineer3 in one version.
        var a2 = await BuildAsync(context, ShowcasePhases.A2, ShowcaseUsers.EngineerThree, templateId, cancellationToken);
        await a2.PublishAsync("Carried over from V6 for the A2 build", cancellationToken);

        // A3: published, then engineer2 starts changing two limits and leaves them locked.
        var a3 = await BuildAsync(context, ShowcasePhases.A3, ShowcaseUsers.Developer, templateId, cancellationToken);
        await a3.PublishAsync("A3 specification", cancellationToken);
        await users.ActAsAsync(ShowcaseUsers.EngineerTwo, cancellationToken);
        await a3.RefreshAsync(cancellationToken);
        var shortBlock = a3.Sheet.Tables[0].Id;
        var crank = a3.Copies(shortBlock, null, SpecificationTemplate.System)[1];
        var journals = a3.Copies(shortBlock, crank.Id, SpecificationTemplate.Dimension);
        await a3.FillAsync(Row(journals[0], 0), null, null, 59.973m, 59.988m, "Under review", false);
        await a3.FillAsync(Row(journals[2], 0), null, null, 0.100m, 0.230m, "Under review", false);

        // A4: published, then the developer (who the app runs as) has changes of their own waiting.
        var a4 = await BuildAsync(context, ShowcasePhases.A4, ShowcaseUsers.Developer, templateId, cancellationToken);
        await a4.PublishAsync("A4 specification", cancellationToken);
        var a4Block = a4.Sheet.Tables[0].Id;
        var a4Pistons = a4.Copies(a4Block, null, SpecificationTemplate.System)[2];
        await a4.FillAsync(Row(a4.Copies(a4Block, a4Pistons.Id, SpecificationTemplate.Dimension)[1], 0), null, 0.280m, 0.200m, 0.350m, "Provisional", false);
        var extra = await a4.AddSectionAsync(a4Block, a4Pistons, SpecificationTemplate.Dimension, cancellationToken);
        await a4.FillAsync(Row(extra, 0), "Ring gap, second ring", 0.450m, 0.350m, 0.600m, "Provisional", false);

        // A5: two versions, the second releasing the surface finishes, which were still provisional, and
        // signing it off.
        var a5 = await BuildAsync(context, ShowcasePhases.A5, ShowcaseUsers.EngineerThree, templateId, cancellationToken);
        await a5.PublishAsync("A5 specification for review", cancellationToken);
        await users.ActAsAsync(ShowcaseUsers.EngineerTwo, cancellationToken);
        await a5.RefreshAsync(cancellationToken);
        foreach (var table in a5.Sheet.Tables)
        {
            foreach (var system in a5.Copies(table.Id, null, SpecificationTemplate.System))
            {
                foreach (var finish in a5.Copies(table.Id, system.Id, SpecificationTemplate.SurfaceFinish))
                {
                    await a5.FillAsync(Row(finish, 0), null, null, "Released", true);
                }
            }
        }

        var approval = await a5.AddSectionAsync(a5.Sheet.Tables[0].Id, null, SpecificationTemplate.Approval, cancellationToken);
        await a5.FillAsync(Row(approval, 0), "Engineer Two", new DateOnly(2026, 11, 2), true);
        await a5.PublishAsync("Released for the A5 build", cancellationToken);
    }

    /// <summary>
    /// V6's third version: a release note and the approval, by the developer. The showcase publishes this
    /// last and looks for its note to know it has been built.
    /// </summary>
    public async Task IssueV6Async(string note, CancellationToken cancellationToken)
    {
        var editor = v6 ?? throw new InvalidOperationException("The V6 specification has to be seeded first.");
        await users.ActAsAsync(ShowcaseUsers.Developer, cancellationToken);
        await editor.RefreshAsync(cancellationToken);

        var release = await editor.AddSectionAsync(v6ShortBlockId, null, SpecificationTemplate.ReleaseNote, cancellationToken);
        await editor.FillAsync(
            Row(release, 0),
            "Valve lash widened to 0.16 to 0.24 mm after hot testing. Lubrication limits added. Exhaust valve material to follow once re-sourced.",
            new DateOnly(2026, 10, 12));
        var approval = await editor.AddSectionAsync(v6ShortBlockId, null, SpecificationTemplate.Approval, cancellationToken);
        await editor.FillAsync(Row(approval, 0), "Engine Developer", new DateOnly(2026, 10, 14), true);
        await editor.PublishAsync(note, cancellationToken);
    }

    private async Task<SheetEditor> BuildAsync(ShowcaseContext context, string phaseCode, string userName, int templateId, CancellationToken cancellationToken)
    {
        await users.ActAsAsync(userName, cancellationToken);
        var editor = await editors.OpenAsync(context.PhaseIds[phaseCode], ShowcasePhases.SpecificationSheetType, context.CellTypes, cancellationToken);
        var step = ShowcaseContext.Step(phaseCode);
        await AddTableAsync(editor, templateId, ShortBlockTitle, [.. ShortBlock, Lubrication], step, cancellationToken);
        await AddTableAsync(editor, templateId, HeadTitle, Head, step, cancellationToken);
        return editor;
    }

    private static async Task<int> AddTableAsync(
        SheetEditor editor,
        int templateId,
        string title,
        SpecificationSystem[] systems,
        int step,
        CancellationToken cancellationToken)
    {
        var tableId = await editor.AddTableAsync(templateId, title, cancellationToken);
        foreach (var system in systems)
        {
            await AddSystemAsync(editor, tableId, system, step, cancellationToken);
        }

        return tableId;
    }

    /// <summary>
    /// Fills the table's first system if it is still empty (a table starts with one), or adds another, and
    /// adds its limits. Surface finishes go in only where the table's template version has them.
    /// </summary>
    private static async Task AddSystemAsync(SheetEditor editor, int tableId, SpecificationSystem system, int step, CancellationToken cancellationToken)
    {
        var existing = editor.Copies(tableId, null, SpecificationTemplate.System);
        var section = existing.Count == 1 && Row(existing[0], 0).Cells.All(cell => cell.Value is not string)
            ? existing[0]
            : await editor.AddSectionAsync(tableId, null, SpecificationTemplate.System, cancellationToken);
        await editor.FillAsync(Row(section, 0), system.Name);

        var drift = step * 0.002m;
        for (var index = 0; index < system.Dimensions.Length; index++)
        {
            var (name, nominal, min, max) = system.Dimensions[index];
            var copy = await editor.AddSectionAsync(tableId, section, SpecificationTemplate.Dimension, cancellationToken);
            await editor.FillAsync(Row(copy, 0), name, nominal + drift, min + drift, max + drift, Status(step, index), step > 2 || index == 0);
        }

        foreach (var (component, material) in system.Materials)
        {
            var copy = await editor.AddSectionAsync(tableId, section, SpecificationTemplate.Material, cancellationToken);
            await editor.FillAsync(Row(copy, 0), component, material, "Released", true);
        }

        for (var index = 0; index < system.Joints.Length; index++)
        {
            var (fastener, thread, torque, angle) = system.Joints[index];
            var copy = await editor.AddSectionAsync(tableId, section, SpecificationTemplate.BoltedJoint, cancellationToken);
            await editor.FillAsync(Row(copy, 1), fastener, thread, torque + (step * 2), angle, Status(step, index + 1), false);
        }

        var hasFinishes = editor.FindSection(tableId, section.Id).AddableSections.Any(addable => addable.Name == SpecificationTemplate.SurfaceFinish);
        foreach (var (surface, ra) in hasFinishes ? system.Finishes : [])
        {
            var copy = await editor.AddSectionAsync(tableId, section, SpecificationTemplate.SurfaceFinish, cancellationToken);
            await editor.FillAsync(Row(copy, 0), surface, ra, "Provisional", false);
        }
    }

    /// <summary>Mostly released, with a few limits still provisional or under review in the earlier phases.</summary>
    private static string Status(int step, int index)
    {
        if (step < 3 && index % 3 == 2)
        {
            return "Provisional";
        }

        return step is 1 or 2 && index % 3 == 1 ? "Under review" : "Released";
    }

    private static SheetRowDto Row(SheetSectionDto section, int index)
    {
        return section.Rows.OrderBy(row => row.DisplayOrder).ElementAt(index);
    }
}
