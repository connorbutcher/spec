namespace PUSpecSheet.Domain.SheetTypes;

/// <summary>What kind of value an identifier of a sheet type takes, such as an engine's serial number.</summary>
public enum SheetIdentifierKind
{
    /// <summary>Free text, compared without regard to case.</summary>
    Text = 0,

    /// <summary>A number, stored in invariant form so 1.50 and 1.5 are the same sheet.</summary>
    Number = 1,

    /// <summary>A calendar date, stored as yyyy-MM-dd.</summary>
    Date = 2,

    /// <summary>One of the identifier's options.</summary>
    Option = 3,
}
