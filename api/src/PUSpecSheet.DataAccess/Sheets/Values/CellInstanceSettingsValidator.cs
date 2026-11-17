using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.CellTypes.LabelledText;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Linking;

namespace PUSpecSheet.DataAccess.Sheets.Values;

internal static class CellInstanceSettingsValidator
{
    public static void Validate(string name, CellKind kind, CellInstanceSettings? settings, Sheet sheet)
    {
        if (!kind.HasInstanceSettings())
        {
            throw new InvalidRequestException($"'{name}' has no settings to choose on a sheet.");
        }

        if (settings is null || settings.IsEmpty)
        {
            return;
        }

        if (settings.Kind != kind)
        {
            throw new InvalidRequestException($"Those settings aren't for '{name}', which is a {kind} cell.");
        }

        switch (settings)
        {
            case LinkedDropdownInstanceSettings linked:
                CheckLinkedDropdown(name, linked, sheet);
                break;

            case LabelledTextInstanceSettings labelled when LabelledTextRules.CheckSlots(labelled.Slots) is { } problem:
                throw new InvalidRequestException($"'{name}': {problem}");
        }
    }

    private static void CheckLinkedDropdown(string name, LinkedDropdownInstanceSettings settings, Sheet sheet)
    {
        if (settings.SourceTemplateCellId is not null && settings.SourceSheetRowId is not null)
        {
            throw new InvalidRequestException($"'{name}' takes its choices from a column or from a row, not both.");
        }

        if (!settings.HasSource)
        {
            throw new InvalidRequestException($"Choose a table, and a column or a row of it, for '{name}'.");
        }

        if (!SheetLinks.SourceExists(sheet, settings))
        {
            throw new InvalidRequestException($"The table, column or row chosen for '{name}' isn't on this sheet.");
        }
    }
}
