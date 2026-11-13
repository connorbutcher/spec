using PUSpecSheet.Database.Models.Templates;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// "Parts matrix", a horizontal table on the Parts sheet. Its own two columns (characteristic and drawing
/// reference) are pinned; then a one-column "Baseline" block (exactly one copy, pinned once scrolled to) and
/// a two-column "Part" block people add copies of, one per part (one to eight, three to start with). The
/// three-row header carries each part's numbers, right hand and left hand with red RH and LH labels, in blue
/// and looked up by "partNumber", and its material.
/// Groups people add hold rows of dimensions (a Min and Max per part), single values (one value across a
/// part's two columns), checks, first-article dates and, from version 2, fastener thread sizes and torques.
/// </summary>
public sealed class PartsMatrixTemplate : IShowcaseTemplate
{
    public const string TemplateName = "Parts matrix";
    public const string Baseline = "Baseline";
    public const string Part = "Part";
    public const string Group = "Group";
    public const string Dimension = "Dimension";
    public const string SingleValue = "Single value";
    public const string Check = "Check";
    public const string FirstArticle = "First article";
    public const string Fastener = "Fastener";

    private static readonly CellStyle Banner = new() { BackgroundColor = "#e2e8f0", Align = CellTextAlign.Center };

    public string SheetTypeName => ShowcasePhases.PartsSheetType;

    public string Name => TemplateName;

    public TemplateOrientation Orientation => TemplateOrientation.Horizontal;

    public int StickyColumnCount => 2;

    public void Lay(TemplateLayout layout, int versionNumber)
    {
        var baseline = layout.Block(Baseline, minimum: 1, maximum: 1, initial: 1, stickyColumns: 1);
        var part = layout.Block(Part, minimum: 1, maximum: 8, initial: 3);

        var header = layout.Header();
        layout.Row(
            header,
            layout.Cell(ShowcaseCellTypeNames.Heading, 1, "Characteristic", rowSpan: 3),
            layout.Cell(ShowcaseCellTypeNames.Heading, 2, "Drawing ref.", rowSpan: 3),
            layout.Cell(ShowcaseCellTypeNames.Heading, 1, "Baseline", rowSpan: 2, block: baseline, style: Banner),
            layout.Cell(
                ShowcaseCellTypeNames.PartNumbers,
                1,
                "Part numbers",
                columnSpan: 2,
                block: part,
                style: new CellStyle { BackgroundColor = "#bfdbfe", TextColor = "#1e3a8a", Bold = true, Align = CellTextAlign.Center },
                lookupKey: "partNumber",
                required: true));
        layout.Row(header, layout.Cell(ShowcaseCellTypeNames.Material, 1, "Material", columnSpan: 2, block: part));
        layout.Row(
            header,
            layout.Cell(ShowcaseCellTypeNames.Heading, 1, "Value", block: baseline),
            layout.Cell(ShowcaseCellTypeNames.Heading, 1, "Min", block: part, style: new CellStyle { Align = CellTextAlign.Center }),
            layout.Cell(ShowcaseCellTypeNames.Heading, 2, "Max", block: part, style: new CellStyle { Align = CellTextAlign.Center }));

        var group = layout.Section(Group, null);
        layout.Row(
            group,
            layout.Cell(ShowcaseCellTypeNames.Text, 1, "Group", columnSpan: 2, style: new CellStyle { Bold = true, BackgroundColor = "#f1f5f9" }));

        var dimension = layout.Section(Dimension, group);
        layout.Row(
            dimension,
            Characteristic(layout),
            Reference(layout),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 1, block: baseline, style: new CellStyle { Italic = true }),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 1, block: part),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 2, block: part));

        var single = layout.Section(SingleValue, group);
        layout.Row(
            single,
            Characteristic(layout),
            Reference(layout),
            layout.Cell(ShowcaseCellTypeNames.Number, 1, block: baseline, style: new CellStyle { Italic = true }),
            layout.Cell(ShowcaseCellTypeNames.Number, 1, columnSpan: 2, block: part, style: new CellStyle { Align = CellTextAlign.Center }));

        var check = layout.Section(Check, group);
        layout.Row(
            check,
            Characteristic(layout),
            Reference(layout),
            layout.Cell(ShowcaseCellTypeNames.Checkbox, 1, "Required", block: baseline),
            layout.Cell(ShowcaseCellTypeNames.PassFail, 1, columnSpan: 2, block: part));

        var firstArticle = layout.Section(FirstArticle, group, maximum: 1);
        layout.Row(
            firstArticle,
            Characteristic(layout),
            Reference(layout),
            layout.Cell(ShowcaseCellTypeNames.Date, 1, "Due", block: baseline),
            layout.Cell(ShowcaseCellTypeNames.Date, 1, "Inspected", columnSpan: 2, block: part));

        if (versionNumber > 1)
        {
            var fastener = layout.Section(Fastener, group);
            layout.Row(
                fastener,
                Characteristic(layout),
                Reference(layout),
                layout.Cell(ShowcaseCellTypeNames.ThreadSize, 1, block: baseline),
                layout.Cell(ShowcaseCellTypeNames.ThreadSize, 1, block: part),
                layout.Cell(
                    ShowcaseCellTypeNames.Torque,
                    2,
                    block: part,
                    configuration: new NumberCellConfiguration { MinValue = 5, MaxValue = 150 }));
        }
    }

    private static DbTemplateCell Characteristic(TemplateLayout layout)
    {
        return layout.Cell(ShowcaseCellTypeNames.Text, 1, "Characteristic", lookupKey: "characteristic");
    }

    private static DbTemplateCell Reference(TemplateLayout layout)
    {
        return layout.Cell(ShowcaseCellTypeNames.Text, 2, "Drawing ref.", style: new CellStyle { TextColor = "#475569" });
    }
}
