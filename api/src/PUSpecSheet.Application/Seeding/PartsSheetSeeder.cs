using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.CellTypes.LabelledText;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Fills in the Parts sheets with the "Parts matrix": a part number and material over each part's columns,
/// a baseline column, and groups of dimensions, single values, checks, first-article dates and (from
/// version 2 of the template) fasteners. V6 starts with three parts and gains a fourth, then has a limit
/// tightened, and engineer3 has a change waiting; the A phases are built from version 2, and A5 is left
/// with a fifth part the developer hasn't published yet.
/// </summary>
public sealed class PartsSheetSeeder(SheetEditorFactory editors, ShowcaseUsers users)
{
    private const int StartingParts = 3;

    /// <summary>The part whose numbers are the same on both hands.</summary>
    private const int SymmetricPart = 2;

    private static readonly string[] Materials = ["EN AC-42100 T6", "42CrMo4 QT", "38MnVS6 forged", "EN AC-42100 T6", "42CrMo4 QT"];

    private static readonly PartsGroup[] Groups =
    [
        new(
            "Piston",
            [
                (PartsMatrixTemplate.Dimension, "Skirt diameter", "DWG-2101 sh.2", 81.960m, 0.005m, 0.010m),
                (PartsMatrixTemplate.Dimension, "Compression height", "DWG-2101 sh.2", 31.500m, 0.050m, 0.030m),
                (PartsMatrixTemplate.Dimension, "Pin bore", "DWG-2101 sh.3", 21.000m, 0.000m, 0.008m),
                (PartsMatrixTemplate.SingleValue, "Weight (g)", "DWG-2101 sh.1", 410m, -6m, 0m),
                (PartsMatrixTemplate.Check, "Crown visual check", "QP-118", 0m, 0m, 0m),
                (PartsMatrixTemplate.FirstArticle, "First article inspection", "QP-102", 0m, 0m, 0m),
            ]),
        new(
            "Connecting rod",
            [
                (PartsMatrixTemplate.Dimension, "Big end bore", "DWG-2205 sh.1", 51.000m, 0.000m, 0.010m),
                (PartsMatrixTemplate.Dimension, "Small end bore", "DWG-2205 sh.1", 21.010m, 0.000m, 0.008m),
                (PartsMatrixTemplate.SingleValue, "Big end mass (g)", "DWG-2205 sh.2", 520m, 4m, 0m),
                (PartsMatrixTemplate.Fastener, "Rod bolt", "DWG-2205 sh.3", 8m, 1m, 0m),
            ]),
        new(
            "Ring pack",
            [
                (PartsMatrixTemplate.Dimension, "Top ring gap", "DWG-2140", 0.300m, 0.010m, 0.100m),
                (PartsMatrixTemplate.SingleValue, "Ring tension (N)", "DWG-2140", 12.5m, 0.5m, 0m),
                (PartsMatrixTemplate.Check, "Coating adhesion", "QP-131", 0m, 0m, 0m),
            ]),
    ];

    /// <summary>V6, built from version 1 of the template, which has no fastener rows.</summary>
    public async Task SeedV6Async(ShowcaseContext context, CancellationToken cancellationToken)
    {
        var (editor, table) = await BuildAsync(context, ShowcasePhases.V6, ShowcaseUsers.Developer, cancellationToken);
        await editor.PublishAsync("First parts matrix for V6", cancellationToken);

        // Version 2, by engineer2: a fourth part, which gets its cells in every existing row.
        await users.ActAsAsync(ShowcaseUsers.EngineerTwo, cancellationToken);
        await editor.RefreshAsync(cancellationToken);
        await editor.AddColumnBlockAsync(table, PartsMatrixTemplate.Part, cancellationToken);
        await FillPartsAsync(editor, table, step: 0, from: StartingParts, cancellationToken);
        await editor.PublishAsync("Added PU-1004-A, the lightweight piston", cancellationToken);

        // Version 3, by engineer3: the skirt diameter tightened on every part.
        await users.ActAsAsync(ShowcaseUsers.EngineerThree, cancellationToken);
        await editor.RefreshAsync(cancellationToken);
        var skirt = Rows(editor, table, PartsMatrixTemplate.Dimension).First();
        var requests = new List<CellValueRequest>();
        var parts = editor.ColumnBlocks(table, PartsMatrixTemplate.Part);
        for (var part = 0; part < parts.Count; part++)
        {
            var centre = 81.960m + (0.005m * part);
            requests.AddRange(editor.Values(skirt, parts[part].Id, centre - 0.006m, centre + 0.006m));
        }

        await editor.SaveAsync(skirt, requests, cancellationToken);
        await editor.PublishAsync("Skirt diameter tightened after first-article results", cancellationToken);

        // engineer3 starts recording the second round of first-article dates and leaves it unpublished.
        var firstArticle = Rows(editor, table, PartsMatrixTemplate.FirstArticle).First();
        await editor.SaveAsync(firstArticle, editor.Values(firstArticle, parts[3].Id, new DateOnly(2026, 10, 28)), cancellationToken);
    }

    /// <summary>The A phases, built from version 2 of the template.</summary>
    public async Task SeedPhasesAsync(ShowcaseContext context, CancellationToken cancellationToken)
    {
        var (a2, _) = await BuildAsync(context, ShowcasePhases.A2, ShowcaseUsers.Developer, cancellationToken);
        await a2.PublishAsync("Parts matrix for A2", cancellationToken);

        var (a3, _) = await BuildAsync(context, ShowcasePhases.A3, ShowcaseUsers.EngineerTwo, cancellationToken);
        await a3.PublishAsync("Parts matrix for A3", cancellationToken);

        var (a4, _) = await BuildAsync(context, ShowcasePhases.A4, ShowcaseUsers.EngineerThree, cancellationToken);
        await a4.PublishAsync("Parts matrix for A4", cancellationToken);

        // A5: published, then the developer adds a fifth part and hasn't published it yet.
        var (a5, table) = await BuildAsync(context, ShowcasePhases.A5, ShowcaseUsers.Developer, cancellationToken);
        await a5.PublishAsync("Parts matrix for A5", cancellationToken);
        await a5.AddColumnBlockAsync(table, PartsMatrixTemplate.Part, cancellationToken);
        await FillPartsAsync(a5, table, ShowcaseContext.Step(ShowcasePhases.A5), from: StartingParts, cancellationToken);
    }

    private async Task<(SheetEditor Editor, int Table)> BuildAsync(ShowcaseContext context, string phaseCode, string userName, CancellationToken cancellationToken)
    {
        await users.ActAsAsync(userName, cancellationToken);
        var editor = await editors.OpenAsync(context.PhaseIds[phaseCode], ShowcasePhases.PartsSheetType, context.CellTypes, cancellationToken);
        var step = ShowcaseContext.Step(phaseCode);
        var table = await editor.AddTableAsync(context.TemplateIds[PartsMatrixTemplate.TemplateName], "Piston and connecting rod parts", cancellationToken);

        foreach (var group in Groups)
        {
            var section = await editor.AddSectionAsync(table, null, PartsMatrixTemplate.Group, cancellationToken);
            await editor.FillAsync(Row(section), group.Name);
            var kinds = editor.FindSection(table, section.Id).AddableSections.Select(addable => addable.Name).ToHashSet();
            foreach (var (kind, characteristic, reference, baseValue, _, _) in group.Rows.Where(row => kinds.Contains(row.Kind)))
            {
                var copy = await editor.AddSectionAsync(table, section, kind, cancellationToken);
                var row = Row(copy);
                var requests = editor.Values(row, null, characteristic, reference);
                requests.AddRange(editor.Values(row, Baseline(editor, table), BaselineValue(kind, baseValue, step)));
                await editor.SaveAsync(row, requests, cancellationToken);
            }
        }

        await FillPartsAsync(editor, table, step, from: 0, cancellationToken);
        return (editor, table);
    }

    /// <summary>The part number, material and every row's values for the parts from <paramref name="from"/> on.</summary>
    private static async Task FillPartsAsync(SheetEditor editor, int table, int step, int from, CancellationToken cancellationToken)
    {
        var parts = editor.ColumnBlocks(table, PartsMatrixTemplate.Part);
        var header = editor.Table(table).Sections[0].Rows.OrderBy(row => row.DisplayOrder).ToList();
        var numbers = new List<CellValueRequest>();
        var materials = new List<CellValueRequest>();
        for (var part = from; part < parts.Count; part++)
        {
            numbers.AddRange(editor.Values(header[0], parts[part].Id, await PartNumbersAsync(editor, header[0], parts[part].Id, part, step, cancellationToken)));
            materials.AddRange(editor.Values(header[1], parts[part].Id, Materials[part % Materials.Length]));
        }

        await editor.SaveAsync(header[0], numbers, cancellationToken);
        await editor.SaveAsync(header[1], materials, cancellationToken);

        foreach (var group in editor.Copies(table, null, PartsMatrixTemplate.Group))
        {
            var spec = Groups.Single(candidate => candidate.Name == Row(group).Cells.Select(cell => cell.Value as string).First(text => text is not null));
            foreach (var (kind, characteristic, _, baseValue, change, spread) in spec.Rows)
            {
                var copies = editor.Copies(table, group.Id, kind);
                var copy = copies.FirstOrDefault(candidate => Row(candidate).Cells.Any(cell => Equals(cell.Value, characteristic)));
                if (copy is null)
                {
                    continue;
                }

                var row = Row(copy);
                var requests = new List<CellValueRequest>();
                for (var part = from; part < parts.Count; part++)
                {
                    requests.AddRange(editor.Values(row, parts[part].Id, PartValues(kind, baseValue + (change * part) + (step * change / 2), spread, part, step)));
                }

                await editor.SaveAsync(row, requests, cancellationToken);
            }
        }
    }

    /// <summary>
    /// A part's right-hand and left-hand numbers. The third part is the same on both hands, so on the sheet its
    /// cell is changed to hold one number labelled "Both" instead of the template's RH and LH.
    /// </summary>
    private static async Task<IReadOnlyDictionary<string, string>> PartNumbersAsync(
        SheetEditor editor,
        SheetRowDto header,
        int partId,
        int part,
        int step,
        CancellationToken cancellationToken)
    {
        var suffix = (char)('A' + step);
        if (part != SymmetricPart)
        {
            return new Dictionary<string, string> { ["rh"] = $"PU-{1001 + part}-{suffix}", ["lh"] = $"PU-{1051 + part}-{suffix}" };
        }

        // The part's only cell in the top header row is its part numbers.
        var cell = header.Cells.Single(candidate => candidate.SheetColumnBlockId == partId);
        var settings = new LabelledTextInstanceSettings { Slots = [new LabelledTextSlot("both", "Both")] };
        await editor.ChooseSettingsAsync(header, cell, settings, cancellationToken);
        return new Dictionary<string, string> { ["both"] = $"PU-{1001 + part}-{suffix}" };
    }

    private static object BaselineValue(string kind, decimal baseValue, int step)
    {
        return kind switch
        {
            PartsMatrixTemplate.Check => true,
            PartsMatrixTemplate.FirstArticle => new DateOnly(2026, 9, 1).AddDays(step * 21),
            PartsMatrixTemplate.Fastener => (int)baseValue,
            _ => baseValue,
        };
    }

    private static object?[] PartValues(string kind, decimal centre, decimal spread, int part, int step)
    {
        return kind switch
        {
            PartsMatrixTemplate.Dimension => [Math.Round(centre - spread, 3), Math.Round(centre + spread, 3)],
            PartsMatrixTemplate.SingleValue => [Math.Round(centre, 2)],
            PartsMatrixTemplate.Check => [part == 2 && step == 0 ? "Fail" : "Pass"],
            PartsMatrixTemplate.FirstArticle => [new DateOnly(2026, 9, 8).AddDays((step * 21) + (part * 3))],
            PartsMatrixTemplate.Fastener => [part % 2 == 0 ? 8 : 10, 25m + (part * 5)],
            _ => [],
        };
    }

    /// <summary>Every row of one kind of section, top to bottom through the groups.</summary>
    private static List<SheetRowDto> Rows(SheetEditor editor, int table, string kind)
    {
        return editor.Copies(table, null, PartsMatrixTemplate.Group)
            .SelectMany(group => editor.Copies(table, group.Id, kind))
            .Select(Row)
            .ToList();
    }

    private static int Baseline(SheetEditor editor, int table)
    {
        return editor.ColumnBlocks(table, PartsMatrixTemplate.Baseline).Single().Id;
    }

    private static SheetRowDto Row(SheetSectionDto section)
    {
        return section.Rows.OrderBy(row => row.DisplayOrder).First();
    }
}
