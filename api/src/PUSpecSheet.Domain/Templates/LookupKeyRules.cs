namespace PUSpecSheet.Domain.Templates;

/// <summary>
/// What a <see cref="TemplateCell.LookupKey"/> can be: a short name that goes in a query string as it is,
/// starting with a letter and then letters, digits, dots, hyphens and underscores.
/// </summary>
public static class LookupKeyRules
{
    public const int MaxLength = 50;

    public const string Description = "A lookup key starts with a letter and holds only letters, digits, dots, hyphens and underscores, up to 50 characters.";

    public static bool IsValid(string key)
    {
        if (key.Length is 0 or > MaxLength || !char.IsAsciiLetter(key[0]))
        {
            return false;
        }

        foreach (var character in key)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('.' or '-' or '_'))
            {
                return false;
            }
        }

        return true;
    }
}
