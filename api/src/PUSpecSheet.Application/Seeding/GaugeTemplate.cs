using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// "Gauges", a small vertical table on the PFKs sheet listing the gauges a phase checks with. The PFK
/// register's gauge column is a linked dropdown pointed at this table's gauge ids.
/// </summary>
public sealed class GaugeTemplate : IShowcaseTemplate
{
    public const string TemplateName = "Gauges";
    public const string Gauge = "Gauge";
    public const string GaugeIdCaption = "Gauge id";

    public string SheetTypeName => ShowcasePhases.PfksSheetType;

    public string Name => TemplateName;

    public TemplateOrientation Orientation => TemplateOrientation.Vertical;

    public int StickyColumnCount => 1;

    public void Lay(TemplateLayout layout, int versionNumber)
    {
        var header = layout.Header();
        layout.Row(
            header,
            layout.Cell(ShowcaseCellTypeNames.Heading, 1, GaugeIdCaption),
            layout.Cell(ShowcaseCellTypeNames.Heading, 2, "Description"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 3, "Calibrated until"),
            layout.Cell(ShowcaseCellTypeNames.Heading, 4, "In service"));

        var gauge = layout.Section(Gauge, null);
        layout.Row(
            gauge,
            layout.Cell(
                ShowcaseCellTypeNames.Text,
                1,
                GaugeIdCaption,
                style: new CellStyle { Bold = true },
                configuration: new TextCellConfiguration { MaxLength = 20 },
                lookupKey: "gaugeId",
                required: true),
            layout.Cell(ShowcaseCellTypeNames.Text, 2),
            layout.Cell(ShowcaseCellTypeNames.Date, 3),
            layout.Cell(ShowcaseCellTypeNames.Checkbox, 4));
    }
}
