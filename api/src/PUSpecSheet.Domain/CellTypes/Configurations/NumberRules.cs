namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>Checks shared by the number-based configurations.</summary>
internal static class NumberRules
{
    public const int MostDecimalPlaces = 6;

    public const int LongestUnit = 20;

    public static string? CheckDecimalPlaces(int? decimalPlaces)
    {
        if (decimalPlaces is < 0 or > MostDecimalPlaces)
        {
            return $"Decimal places must be between 0 and {MostDecimalPlaces}.";
        }

        return null;
    }

    public static string? CheckUnit(string? unit)
    {
        if (unit?.Length > LongestUnit)
        {
            return $"The unit can be at most {LongestUnit} characters.";
        }

        return null;
    }
}
