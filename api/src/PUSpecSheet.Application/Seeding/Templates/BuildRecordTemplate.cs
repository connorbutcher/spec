using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.Styles;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.Application.Seeding.ReferenceData;

namespace PUSpecSheet.Application.Seeding.Templates;

/// <summary>
/// "As-built record", a vertical table on the Engine Specifications sheets, one sheet per engine serial
/// number: the assembly stages people add (at least one, at most eight), each holding the measurements taken
/// at that stage against their limits with a pass or fail, then build notes after the stages.
/// </summary>
public sealed class BuildRecordTemplate : IShowcaseTemplate
{
    public const string TemplateName = "As-built record";
    public const string Stage = "Assembly stage";
    public const string Measurement = "Measurement";
    public const string BuildNote = "Build note";

    private static readonly CellStyle Banner = new() { BackgroundColor = "#e2e8f0", Align = CellTextAlign.Center };

    public string SheetTypeName => ShowcasePhases.EngineSpecificationsSheetType;

    public string Name => TemplateName;

    public TemplateOrientation Orientation => TemplateOrientation.Vertical;

    public int StickyColumnCount => 1;

    public void Lay(TemplateLayout layout, int versionNumber)
    {
        var header = layout.Header();
        layout.Row(
            header,
            layout.Cell(ShowcaseCellTypeNames.Heading, 1, "Characteristic", rowSpan: 2),
            layout.Cell(ShowcaseCellTypeNames.Heading, 2, "Measured", rowSpan: 2),
            layout.Cell(ShowcaseCellTypeNames.Heading, 3, "Limits", columnSpan: 2, style: Banner),
            layout.Cell(ShowcaseCellTypeNames.Heading, 5, "Result", rowSpan: 2));
        layout.Row(
            header,
            layout.Cell(ShowcaseCellTypeNames.Heading, 3, "Min"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 4, "Max"));

        var stage = layout.Section(Stage, null, minimum: 1, maximum: 8, initial: 1);
        layout.Row(
            stage,
            layout.Cell(
                ShowcaseCellTypeNames.Text,
                1,
                "Stage",
                columnSpan: 5,
                style: new CellStyle { Bold = true, BackgroundColor = "#dbeafe", TextColor = "#1e3a8a" },
                configuration: new TextCellConfiguration { MaxLength = 80 },
                lookupKey: "stage",
                required: true));

        var measurement = layout.Section(Measurement, stage);
        layout.Row(
            measurement,
            layout.Cell(ShowcaseCellTypeNames.Text, 1, "Characteristic", lookupKey: "characteristic", required: true),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 2),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 3),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 4),
            layout.Cell(ShowcaseCellTypeNames.PassFail, 5));

        var note = layout.Section(BuildNote, null, maximum: 3);
        layout.Row(
            note,
            layout.Cell(ShowcaseCellTypeNames.Notes, 1, "Note", columnSpan: 3),
            layout.Cell(ShowcaseCellTypeNames.Heading, 4, "On", style: new CellStyle { Align = CellTextAlign.Right }),
            layout.Cell(ShowcaseCellTypeNames.Date, 5));
    }
}
