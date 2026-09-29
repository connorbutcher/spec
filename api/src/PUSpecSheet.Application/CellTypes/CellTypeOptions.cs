using System.Globalization;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.CellTypes;

/// <summary>Checks and applies the choices of a dropdown cell type.</summary>
internal static class CellTypeOptions
{
    private const int LongestOption = 100;

    /// <summary>
    /// Trims the options and drops blanks. Duplicates are refused, and a number dropdown's options must
    /// all be numbers.
    /// </summary>
    public static List<string> Clean(CellKind kind, IReadOnlyList<string>? options)
    {
        var cleaned = (options ?? [])
            .Select(option => option.Trim())
            .Where(option => option.Length > 0)
            .ToList();

        var duplicate = cleaned
            .GroupBy(option => option, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidRequestException($"The option \"{duplicate.Key}\" is listed more than once.");
        }

        if (cleaned.Any(option => option.Length > LongestOption))
        {
            throw new InvalidRequestException($"Options can be at most {LongestOption} characters.");
        }

        if (kind == CellKind.NumberDropdown)
        {
            var notNumber = cleaned.FirstOrDefault(option => !IsNumber(option));
            if (notNumber is not null)
            {
                throw new InvalidRequestException($"\"{notNumber}\" isn't a number. A number dropdown's options must all be numbers.");
            }
        }

        return cleaned;
    }

    /// <summary>
    /// Makes the cell type's options match <paramref name="values"/> in order, keeping existing option
    /// records whose value matches (ignoring case) so their ids survive.
    /// </summary>
    public static void Sync(CellType cellType, IReadOnlyList<string> values)
    {
        var existing = cellType.Options.ToList();

        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];
            var match = existing.FirstOrDefault(option =>
                string.Equals(option.Value, value, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                cellType.Options.Add(new CellTypeOption { Value = value, DisplayOrder = index + 1 });
            }
            else
            {
                existing.Remove(match);
                match.Value = value;
                match.DisplayOrder = index + 1;
            }
        }

        foreach (var removed in existing)
        {
            cellType.Options.Remove(removed);
        }
    }

    private static bool IsNumber(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _);
    }
}
