using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.CellTypes;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.CellTypes;

/// <summary>
/// Copies a <see cref="SaveCellTypeRequest"/> onto a cell type, keeping only the settings its kind uses.
/// </summary>
internal static class CellTypeSettings
{
    public static void Apply(SaveCellTypeRequest request, CellType cellType)
    {
        cellType.Name = request.Name.Trim();
        cellType.Kind = request.Kind;
        cellType.Description = NullIfBlank(request.Description);

        var isText = request.Kind == CellKind.Text;
        var isNumber = request.Kind == CellKind.Number;

        cellType.MaxLength = isText ? request.MaxLength : null;
        cellType.DecimalPlaces = isNumber ? request.DecimalPlaces : null;
        cellType.MinValue = isNumber ? request.MinValue : null;
        cellType.MaxValue = isNumber ? request.MaxValue : null;
        cellType.Unit = isNumber ? NullIfBlank(request.Unit) : null;

        if (cellType.MinValue > cellType.MaxValue)
        {
            throw new InvalidRequestException("The minimum value can't be more than the maximum value.");
        }

        var options = request.Kind == CellKind.Dropdown ? CleanOptions(request.Options) : [];
        SyncOptions(cellType, options);
    }

    private static List<string> CleanOptions(IReadOnlyList<string>? options)
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

        if (cleaned.Any(option => option.Length > 100))
        {
            throw new InvalidRequestException("Options can be at most 100 characters.");
        }

        return cleaned;
    }

    /// <summary>
    /// Makes the cell type's options match <paramref name="values"/> in order, keeping existing option
    /// records whose value matches (ignoring case) so their ids survive.
    /// </summary>
    private static void SyncOptions(CellType cellType, IReadOnlyList<string> values)
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

    private static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
