using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.LabelledText;
using PUSpecSheet.Abstractions.CellTypes.Styles;
using PUSpecSheet.DataAccess.CellTypes;
using PUSpecSheet.Database.Models.CellTypes;

namespace PUSpecSheet.DataAccess.Seeding.ReferenceData;

public sealed class ShowcaseCellTypes
{
    private static readonly CellStyle _right = new() { Align = CellTextAlign.Right };

    private static readonly CellStyle _centre = new() { Align = CellTextAlign.Center };

    private static readonly (string Name, CellKind Kind, string? Description, CellConfiguration Configuration, CellStyle Style, string[] Options)[] _definitions =
    [
        (ShowcaseCellTypeNames.Heading, CellKind.Heading, "Fixed text such as a column header.", new HeadingCellConfiguration(), new CellStyle { Bold = true }, []),
        (ShowcaseCellTypeNames.Text, CellKind.Text, null, new TextCellConfiguration { MaxLength = 200 }, new CellStyle(), []),
        (ShowcaseCellTypeNames.Number, CellKind.Number, null, new NumberCellConfiguration { DecimalPlaces = 2 }, _right, []),
        (ShowcaseCellTypeNames.Date, CellKind.Date, null, new DateCellConfiguration(), new CellStyle(), []),
        (ShowcaseCellTypeNames.Checkbox, CellKind.Checkbox, null, new CheckboxCellConfiguration(), _centre, []),
        (ShowcaseCellTypeNames.PassFail, CellKind.TextDropdown, null, new TextDropdownCellConfiguration(), _centre, ["Pass", "Fail", "N/A"]),
        (ShowcaseCellTypeNames.Group, CellKind.Group, "A caption that groups the cells around it.", new GroupCellConfiguration(), new CellStyle { Bold = true, BackgroundColor = "#f1f5f9" }, []),
        (
            ShowcaseCellTypeNames.Measurement,
            CellKind.Number,
            "A measured dimension to the micron.",
            new NumberCellConfiguration { DecimalPlaces = 3, MinValue = 0, Unit = "mm" },
            _right,
            []),
        (
            ShowcaseCellTypeNames.Torque,
            CellKind.Number,
            "A tightening torque.",
            new NumberCellConfiguration { DecimalPlaces = 1, MinValue = 0, MaxValue = 500, Unit = "Nm" },
            _right,
            []),
        (
            ShowcaseCellTypeNames.Notes,
            CellKind.Text,
            "Longer free text, entered over several lines.",
            new TextCellConfiguration { MaxLength = 1000, Multiline = true },
            new CellStyle { Italic = true },
            []),
        (
            ShowcaseCellTypeNames.Material,
            CellKind.TextDropdown,
            "The material a part is made from.",
            new TextDropdownCellConfiguration(),
            new CellStyle(),
            ["EN AC-42100 T6", "EN-GJL-250 grey iron", "42CrMo4 QT", "38MnVS6 forged", "X45CrSi9-3 valve steel"]),
        (
            ShowcaseCellTypeNames.ReleaseStatus,
            CellKind.TextDropdown,
            "Where a limit is in its release.",
            new TextDropdownCellConfiguration(),
            _centre,
            ["Released", "Provisional", "Under review", "Superseded"]),
        (
            ShowcaseCellTypeNames.FeatureClass,
            CellKind.TextDropdown,
            "How much a product feature matters: critical, significant or standard.",
            new TextDropdownCellConfiguration(),
            new CellStyle { Bold = true, Align = CellTextAlign.Center },
            ["CC", "SC", "Std"]),
        (
            ShowcaseCellTypeNames.ThreadSize,
            CellKind.NumberDropdown,
            "A metric thread's nominal diameter.",
            new NumberDropdownCellConfiguration { DecimalPlaces = 0, Unit = "mm" },
            _right,
            ["6", "8", "10", "12", "14"]),
        (
            ShowcaseCellTypeNames.SampleFrequency,
            CellKind.NumberDropdown,
            "Checked once in this many parts.",
            new NumberDropdownCellConfiguration { DecimalPlaces = 0, Unit = "pcs" },
            _right,
            ["1", "5", "10", "25", "50", "100"]),
        (
            ShowcaseCellTypeNames.Linked,
            CellKind.LinkedDropdown,
            "A choice from a column or row of another table, picked on the sheet.",
            new LinkedDropdownCellConfiguration(),
            new CellStyle(),
            []),
        (
            ShowcaseCellTypeNames.PartNumbers,
            CellKind.LabelledText,
            "A part number for each hand, each labelled in red.",
            new LabelledTextCellConfiguration
            {
                MaxLength = 20,
                Slots = [new LabelledTextSlot { Key = "rh", Label = "RH" }, new LabelledTextSlot { Key = "lh", Label = "LH" }],
                LabelColor = "#dc2626",
            },
            new CellStyle(),
            []),
    ];

    private readonly ICellTypeDataService _cellTypeData;

    public ShowcaseCellTypes(ICellTypeDataService cellTypeData)
    {
        _cellTypeData = cellTypeData;
    }

    public async Task<ShowcaseCellTypeCatalog> EnsureAsync(CancellationToken cancellationToken)
    {
        var names = _definitions.Select(definition => definition.Name).ToList();
        var existing = await _cellTypeData.ListNamesInUseAsync(names, cancellationToken);
        var order = await _cellTypeData.GetLastDisplayOrderAsync(cancellationToken) ?? 0;

        foreach (var definition in _definitions.Where(definition => !existing.Contains(definition.Name, StringComparer.OrdinalIgnoreCase)))
        {
            var cellType = new DbCellType
            {
                Name = definition.Name,
                Kind = definition.Kind,
                Description = definition.Description,
                DisplayOrder = ++order,
                Configuration = definition.Configuration,
                Style = definition.Style,
            };
            for (var index = 0; index < definition.Options.Length; index++)
            {
                cellType.Options.Add(new DbCellTypeOption { Value = definition.Options[index], DisplayOrder = index + 1 });
            }

            _cellTypeData.Add(cellType);
        }

        await _cellTypeData.SaveAsync(cancellationToken);

        var cellTypes = await _cellTypeData.ListByNamesAsync(names, cancellationToken);
        return new ShowcaseCellTypeCatalog(cellTypes);
    }
}
