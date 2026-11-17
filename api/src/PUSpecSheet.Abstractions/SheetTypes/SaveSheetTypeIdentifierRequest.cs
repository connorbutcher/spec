using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.SheetTypes;

/// <summary>
/// Adds or changes an identifier of a sheet type. Its key is made from the name when it is added and
/// never changes. Its kind can't change once a sheet has a value for it.
/// </summary>
public sealed record SaveSheetTypeIdentifierRequest
{
    public SaveSheetTypeIdentifierRequest(
        [Required, MaxLength(100)] string name,
        SheetIdentifierKind kind,
        bool isRequired,
        [MaxLength(100)] IReadOnlyList<string>? options)
    {
        this.Name = name;
        this.Kind = kind;
        this.IsRequired = isRequired;
        this.Options = options;
    }

    public string Name { get; init; }

    public SheetIdentifierKind Kind { get; init; }

    public bool IsRequired { get; init; }

    /// <summary>The choices of an option identifier, in order. Ignored for the other kinds.</summary>
    public IReadOnlyList<string>? Options { get; init; }
}
