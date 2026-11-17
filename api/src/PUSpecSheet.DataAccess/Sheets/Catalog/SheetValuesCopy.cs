using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.DataAccess.Sheets.Catalog;

public sealed record SheetValuesCopy
{
    public required IReadOnlyList<DbTextValue> Texts { get; init; }

    public required IReadOnlyList<DbNumericValue> Numbers { get; init; }

    public required IReadOnlyList<DbDateValue> Dates { get; init; }

    public required IReadOnlyList<DbBooleanValue> Booleans { get; init; }

    public required IReadOnlyList<DbOptionValue> Options { get; init; }

    public required IReadOnlyList<DbLabelledTextValue> LabelledTexts { get; init; }

    public required IReadOnlyList<DbCellSettingsValue> Settings { get; init; }

    public required IReadOnlyList<DbLinkedSheetValue> LinkedSheets { get; init; }
}
