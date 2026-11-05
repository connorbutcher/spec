using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// "Engine specification", a vertical table on the Specification sheet: a two-row header with grouped
/// column captions, then systems people add (at least one, at most eight, twelve from version 2), each
/// holding the kinds of limit row people add under it: dimensions, materials, bolted joints (with a group
/// caption row) and, from version 2, surface finishes. Release notes and one approval sit after the systems.
/// </summary>
public sealed class SpecificationTemplate : IShowcaseTemplate
{
    public const string TemplateName = "Engine specification";
    public const string System = "System";
    public const string Dimension = "Dimension";
    public const string Material = "Material";
    public const string BoltedJoint = "Bolted joint";
    public const string SurfaceFinish = "Surface finish";
    public const string ReleaseNote = "Release note";
    public const string Approval = "Approval";

    private static readonly CellStyle Banner = new() { BackgroundColor = "#e2e8f0", Align = CellTextAlign.Center };

    public string SheetTypeName => ShowcasePhases.SpecificationSheetType;

    public string Name => TemplateName;

    public TemplateOrientation Orientation => TemplateOrientation.Vertical;

    public int StickyColumnCount => 1;

    public void Lay(TemplateLayout layout, int versionNumber)
    {
        var header = layout.Header();
        layout.Row(
            header,
            layout.Cell(ShowcaseCellTypeNames.Heading, 1, "Characteristic", rowSpan: 2),
            layout.Cell(ShowcaseCellTypeNames.Heading, 2, "Limits", columnSpan: 3, style: Banner),
            layout.Cell(ShowcaseCellTypeNames.Heading, 5, "Release", columnSpan: 2, style: Banner));
        layout.Row(
            header,
            layout.Cell(ShowcaseCellTypeNames.Heading, 2, "Nominal"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 3, "Min"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 4, "Max"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 5, "Status"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 6, "Verified"));

        var system = layout.Section(System, null, minimum: 1, maximum: versionNumber == 1 ? 8 : 12, initial: 1);
        layout.Row(
            system,
            layout.Cell(
                ShowcaseCellTypeNames.Text,
                1,
                "System name",
                columnSpan: 6,
                style: new CellStyle { Bold = true, BackgroundColor = "#dbeafe", TextColor = "#1e3a8a" },
                configuration: new TextCellConfiguration { MaxLength = 80 },
                lookupKey: "system",
                required: true));

        var dimension = layout.Section(Dimension, system);
        layout.Row(
            dimension,
            layout.Cell(ShowcaseCellTypeNames.Text, 1, "Characteristic", lookupKey: "characteristic", required: versionNumber > 1),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 2),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 3),
            layout.Cell(ShowcaseCellTypeNames.Measurement, 4),
            layout.Cell(ShowcaseCellTypeNames.ReleaseStatus, 5),
            layout.Cell(ShowcaseCellTypeNames.Checkbox, 6));

        var material = layout.Section(Material, system, maximum: 2);
        layout.Row(
            material,
            layout.Cell(ShowcaseCellTypeNames.Text, 1, "Component", lookupKey: "characteristic"),
            layout.Cell(ShowcaseCellTypeNames.Material, 2, columnSpan: 3),
            layout.Cell(ShowcaseCellTypeNames.ReleaseStatus, 5),
            layout.Cell(ShowcaseCellTypeNames.Checkbox, 6));

        var joint = layout.Section(BoltedJoint, system, maximum: 4);
        layout.Row(joint, layout.Cell(ShowcaseCellTypeNames.Group, 1, "Bolted joint: thread, torque and final angle", columnSpan: 6));
        layout.Row(
            joint,
            layout.Cell(ShowcaseCellTypeNames.Text, 1, "Fastener", lookupKey: "characteristic"),
            layout.Cell(ShowcaseCellTypeNames.ThreadSize, 2),
            layout.Cell(ShowcaseCellTypeNames.Torque, 3),
            layout.Cell(
                ShowcaseCellTypeNames.Number,
                4,
                "Angle",
                configuration: new NumberCellConfiguration { DecimalPlaces = 0, MinValue = 0, MaxValue = 360, Unit = "°" }),
            layout.Cell(ShowcaseCellTypeNames.ReleaseStatus, 5),
            layout.Cell(ShowcaseCellTypeNames.Checkbox, 6));

        if (versionNumber > 1)
        {
            var finish = layout.Section(SurfaceFinish, system);
            layout.Row(
                finish,
                layout.Cell(ShowcaseCellTypeNames.Text, 1, "Surface", lookupKey: "characteristic"),
                layout.Cell(
                    ShowcaseCellTypeNames.Number,
                    2,
                    "Ra",
                    columnSpan: 3,
                    style: new CellStyle { Align = CellTextAlign.Center },
                    configuration: new NumberCellConfiguration { DecimalPlaces = 2, MinValue = 0, MaxValue = 25, Unit = "µm Ra" }),
                layout.Cell(ShowcaseCellTypeNames.ReleaseStatus, 5),
                layout.Cell(ShowcaseCellTypeNames.Checkbox, 6));
        }

        var note = layout.Section(ReleaseNote, null, maximum: 3);
        layout.Row(
            note,
            layout.Cell(ShowcaseCellTypeNames.Notes, 1, "What changed and why", columnSpan: 4),
            layout.Cell(ShowcaseCellTypeNames.Heading, 5, "Effective from", style: new CellStyle { Align = CellTextAlign.Right }),
            layout.Cell(ShowcaseCellTypeNames.Date, 6));

        var approval = layout.Section(Approval, null, maximum: 1);
        layout.Row(
            approval,
            layout.Cell(ShowcaseCellTypeNames.Heading, 1, "Approved by"),
            layout.Cell(ShowcaseCellTypeNames.Text, 2, "Name", columnSpan: 2, required: true),
            layout.Cell(ShowcaseCellTypeNames.Heading, 4, "On", style: new CellStyle { Align = CellTextAlign.Right }),
            layout.Cell(ShowcaseCellTypeNames.Date, 5, required: true),
            layout.Cell(ShowcaseCellTypeNames.Checkbox, 6, "Signed"));
    }
}
