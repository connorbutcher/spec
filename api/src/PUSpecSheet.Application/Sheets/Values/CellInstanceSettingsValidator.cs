using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.CellTypes.LabelledText;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>
/// Checks the settings chosen on a sheet for one cell against the cell's kind and the sheet they are for.
/// Each kind of settings has its own check here; a kind with nothing to check needs none.
/// </summary>
internal static class CellInstanceSettingsValidator
{
    /// <param name="name">What to call the cell in a message.</param>
    /// <param name="kind">The cell's kind.</param>
    /// <param name="settings">The settings to check. Null, or nothing set, clears them and is always allowed.</param>
    /// <param name="sheet">The sheet as the user sees it.</param>
    public static void Validate(string name, CellKind kind, CellInstanceSettings? settings, SheetDto sheet)
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

    private static void CheckLinkedDropdown(string name, LinkedDropdownInstanceSettings settings, SheetDto sheet)
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
