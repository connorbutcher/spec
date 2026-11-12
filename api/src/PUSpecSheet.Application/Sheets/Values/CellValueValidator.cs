using PUSpecSheet.Application.Common;
using PUSpecSheet.Data.Values;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.LabelledText;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>Checks a new cell value against its cell's kind and the settings its cell type and cell give it.</summary>
internal static class CellValueValidator
{
    /// <summary>Expects <see cref="TemplateCell.CellType"/> (with its options) to be loaded.</summary>
    public static void Validate(TemplateCell cell, CellValueBag request)
    {
        var cellType = cell.CellType;
        var configuration = CellSettingsResolver.ResolveConfiguration(cell);
        var name = string.IsNullOrWhiteSpace(cell.Caption) ? cellType.Name : cell.Caption;

        switch (cellType.Kind)
        {
            case CellKind.Heading:
            case CellKind.Group:
                throw new InvalidRequestException($"'{name}' is not filled in on a sheet.");

            case CellKind.Text:
                var maxLength = (configuration as TextCellConfiguration)?.MaxLength ?? TextCellConfiguration.LongestMaxLength;
                if (request.Text is { } text && text.Length > maxLength)
                {
                    throw new InvalidRequestException($"'{name}' can have at most {maxLength} characters.");
                }

                break;

            case CellKind.LabelledText:
                CheckSlots(name, request, configuration as LabelledTextCellConfiguration);
                break;

            case CellKind.Number:
                if (request.Number is { } number && configuration is NumberCellConfiguration numberConfiguration)
                {
                    CheckNumber(name, number, numberConfiguration);
                }

                break;

            case CellKind.TextDropdown:
            case CellKind.NumberDropdown:
                if (request.OptionId is { } optionId && cellType.Options.All(option => option.Id != optionId))
                {
                    throw new InvalidRequestException($"That isn't one of the choices for '{name}'.");
                }

                break;
        }
    }

    private static void CheckSlots(string name, CellValueBag request, LabelledTextCellConfiguration? configuration)
    {
        var maxLength = configuration?.MaxLength ?? TextCellConfiguration.LongestMaxLength;
        foreach (var (key, value) in request.Slots ?? new Dictionary<string, string>())
        {
            if (!LabelledTextRules.IsKey(key))
            {
                throw new InvalidRequestException($"'{key}' isn't one of the values of '{name}'.");
            }

            if (value?.Trim().Length > maxLength)
            {
                throw new InvalidRequestException($"Each value of '{name}' can have at most {maxLength} characters.");
            }
        }
    }

    private static void CheckNumber(string name, decimal number, NumberCellConfiguration configuration)
    {
        if (configuration.MinValue is { } min && number < min)
        {
            throw new InvalidRequestException($"'{name}' can't be less than {min}.");
        }

        if (configuration.MaxValue is { } max && number > max)
        {
            throw new InvalidRequestException($"'{name}' can't be more than {max}.");
        }

        if (configuration.DecimalPlaces is { } places && Math.Round(number, places) != number)
        {
            throw new InvalidRequestException($"'{name}' can have at most {places} decimal places.");
        }
    }
}
