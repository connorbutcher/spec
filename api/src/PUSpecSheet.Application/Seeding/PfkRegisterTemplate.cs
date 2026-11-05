using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// "PFK register", a wide vertical table on the PFKs sheet with its first two columns pinned. Each machining
/// operation people add (at least one) holds the product features checked at it (at least one, at most
/// twenty), and a feature can hold a reaction plan: three levels of addable sections. Version 2 allows up to
/// three reaction plans and adds a free-text note under an operation. Version 3 is still being edited in the
/// template designer, so no sheet uses it yet.
/// </summary>
public sealed class PfkRegisterTemplate : IShowcaseTemplate
{
    public const string TemplateName = "PFK register";
    public const string Operation = "Operation";
    public const string Feature = "PFK";
    public const string ReactionPlan = "Reaction plan";
    public const string OperationNote = "Operation note";
    public const string Revalidation = "Revalidation";

    private static readonly CellStyle Banner = new() { BackgroundColor = "#e2e8f0", Align = CellTextAlign.Center };

    public string SheetTypeName => ShowcasePhases.PfksSheetType;

    public string Name => TemplateName;

    public TemplateOrientation Orientation => TemplateOrientation.Vertical;

    public int StickyColumnCount => 2;

    public void Lay(TemplateLayout layout, int versionNumber)
    {
        var header = layout.Header();
        layout.Row(
            header,
            layout.Cell(ShowcaseCellTypeNames.Heading, 1, "PFK no.", rowSpan: 2),
            layout.Cell(ShowcaseCellTypeNames.Heading, 2, "Feature", rowSpan: 2),
            layout.Cell(ShowcaseCellTypeNames.Heading, 3, "Class", rowSpan: 2),
            layout.Cell(ShowcaseCellTypeNames.Heading, 4, "Tolerance", columnSpan: 2, style: Banner),
            layout.Cell(ShowcaseCellTypeNames.Heading, 6, "Control", columnSpan: 4, style: Banner));
        layout.Row(
            header,
            layout.Cell(ShowcaseCellTypeNames.Heading, 4, "Lower"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 5, "Upper"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 6, "Every"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 7, "Gauge"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 8, "Last audit"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 9, "Result"));

        var operation = layout.Section(Operation, null, minimum: 1, initial: 1);
        layout.Row(
            operation,
            layout.Cell(
                ShowcaseCellTypeNames.Text,
                1,
                "Operation",
                columnSpan: 9,
                style: new CellStyle { Bold = true, BackgroundColor = "#f1f5f9" },
                lookupKey: "operation",
                required: true));

        var feature = layout.Section(Feature, operation, minimum: 1, maximum: 20, initial: 1);
        layout.Row(
            feature,
            layout.Cell(
                ShowcaseCellTypeNames.Text,
                1,
                "PFK no.",
                style: new CellStyle { Bold = true, TextColor = "#1e3a8a" },
                configuration: new TextCellConfiguration { MaxLength = 12 },
                lookupKey: "pfkNumber",
                required: true),
            layout.Cell(ShowcaseCellTypeNames.Text, 2, "Feature"),
            layout.Cell(ShowcaseCellTypeNames.FeatureClass, 3),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 4),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 5),
            layout.Cell(ShowcaseCellTypeNames.SampleFrequency, 6),
            layout.Cell(ShowcaseCellTypeNames.Linked, 7),
            layout.Cell(ShowcaseCellTypeNames.Date, 8),
            layout.Cell(ShowcaseCellTypeNames.PassFail, 9));

        var plan = layout.Section(ReactionPlan, feature, maximum: versionNumber == 1 ? 1 : 3);
        layout.Row(
            plan,
            layout.Cell(ShowcaseCellTypeNames.Group, 1, "Reaction plan", columnSpan: 2),
            layout.Cell(ShowcaseCellTypeNames.Notes, 3, "What to do when the feature is out of tolerance", columnSpan: 7));

        if (versionNumber > 1)
        {
            var note = layout.Section(OperationNote, operation, maximum: 1);
            layout.Row(
                note,
                layout.Cell(ShowcaseCellTypeNames.Notes, 1, "Note", columnSpan: 9, style: new CellStyle { TextColor = "#475569" }));
        }

        if (versionNumber > 2)
        {
            var revalidation = layout.Section(Revalidation, feature, maximum: 1);
            layout.Row(
                revalidation,
                layout.Cell(ShowcaseCellTypeNames.Group, 1, "Revalidated", columnSpan: 2),
                layout.Cell(ShowcaseCellTypeNames.Date, 3, columnSpan: 2),
                layout.Cell(
                    ShowcaseCellTypeNames.Number,
                    5,
                    "Cpk",
                    configuration: new NumberCellConfiguration { DecimalPlaces = 2, MinValue = 0, MaxValue = 10 }),
                layout.Cell(ShowcaseCellTypeNames.PassFail, 9));
        }
    }
}
