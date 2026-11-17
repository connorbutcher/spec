using System.Globalization;
using System.Text;

namespace PUSpecSheet.Abstractions.SheetTypes;

/// <summary>
/// How identifier values are checked, stored and compared. A value is stored in one form per kind so that
/// two ways of writing the same thing (1.50 and 1.5, "esn-1" and "ESN-1") name the same sheet.
/// </summary>
public static class SheetIdentifierRules
{
    /// <summary>How many identifiers a sheet type can have.</summary>
    public const int MaxIdentifiers = 4;

    /// <summary>The longest value, in characters, once stored.</summary>
    public const int MaxValueLength = 80;

    /// <summary>The longest key, in characters.</summary>
    public const int MaxKeyLength = 50;

    private const char Separator = '\u001F';

    /// <summary>
    /// Checks a value given for an identifier and puts it in its stored form: trimmed text, an invariant
    /// number without trailing zeros, a yyyy-MM-dd date, or the option as the identifier spells it. A blank
    /// value is stored as null, and is only allowed when the identifier isn't required.
    /// </summary>
    public static bool TryNormalise(SheetIdentifierDefinition definition, string? raw, out string? stored, out string? error)
    {
        ArgumentNullException.ThrowIfNull(definition);
        stored = null;
        error = null;
        var text = raw?.Trim() ?? string.Empty;

        if (text.Length == 0)
        {
            if (definition.IsRequired)
            {
                error = $"{definition.Name} is required.";
                return false;
            }

            return true;
        }

        switch (definition.Kind)
        {
            case SheetIdentifierKind.Number:
                if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
                {
                    error = $"{definition.Name} must be a number.";
                    return false;
                }

                text = number.ToString("0.############################", CultureInfo.InvariantCulture);
                break;
            case SheetIdentifierKind.Date:
                if (!DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    error = $"{definition.Name} must be a date.";
                    return false;
                }

                text = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                break;
            case SheetIdentifierKind.Option:
                var option = definition.Options.FirstOrDefault(candidate => string.Equals(candidate, text, StringComparison.OrdinalIgnoreCase));
                if (option is null)
                {
                    error = $"{definition.Name} must be one of its options.";
                    return false;
                }

                text = option;
                break;
            default:
                break;
        }

        if (text.Length > MaxValueLength)
        {
            error = $"{definition.Name} can be at most {MaxValueLength} characters.";
            return false;
        }

        stored = text;
        return true;
    }

    /// <summary>
    /// A key for comparing sheets' identities in memory, never stored: each non-blank value in its compared
    /// form, by identifier id. Two sheets name the same thing when their keys are equal. A sheet with no
    /// values has an empty key, so adding an optional identifier to a sheet type later doesn't change the
    /// identities of the sheets it already has.
    /// </summary>
    public static string ComparisonKey(IEnumerable<SheetIdentifierValue> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var parts = values
            .Where(value => value.Value.Length > 0)
            .OrderBy(value => value.Definition.Id)
            .Select(value => string.Create(CultureInfo.InvariantCulture, $"{value.Definition.Id}={Compared(value)}"));
        return string.Join(Separator, parts);
    }

    /// <summary>
    /// A key for the published API's filters made from an identifier's name: "Serial Number" becomes
    /// <c>serialNumber</c>. Letters and digits only, starting with a letter.
    /// </summary>
    public static string KeyFromName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var words = new List<string>();
        var word = new StringBuilder();
        foreach (var character in name)
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                word.Append(character);
            }
            else if (word.Length > 0)
            {
                words.Add(word.ToString());
                word.Clear();
            }
        }

        if (word.Length > 0)
        {
            words.Add(word.ToString());
        }

        var key = new StringBuilder();
        foreach (var part in words)
        {
            if (key.Length == 0)
            {
                key.Append(part.ToLowerInvariant());
            }
            else
            {
                key.Append(char.ToUpperInvariant(part[0])).Append(part[1..]);
            }
        }

        if (key.Length == 0 || !char.IsAsciiLetter(key[0]))
        {
            key.Insert(0, "identifier");
        }

        return key.Length > MaxKeyLength ? key.ToString(0, MaxKeyLength) : key.ToString();
    }

    private static string Compared(SheetIdentifierValue value)
    {
        return value.Definition.Kind is SheetIdentifierKind.Text or SheetIdentifierKind.Option
            ? value.Value.ToUpperInvariant()
            : value.Value;
    }
}
