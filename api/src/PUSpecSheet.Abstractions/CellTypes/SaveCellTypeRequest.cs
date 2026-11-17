using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.Styles;

namespace PUSpecSheet.Abstractions.CellTypes;

/// <summary>
/// Creates or replaces a cell type. A <see cref="Configuration"/> for a different kind than
/// <see cref="Kind"/>, or none, starts the kind with nothing set. <see cref="Options"/> is the full list
/// of dropdown choices, in order, and is ignored for other kinds.
/// </summary>
public sealed record SaveCellTypeRequest
{
    public SaveCellTypeRequest(
        [Required, MaxLength(100)] string name,
        CellKind kind,
        [MaxLength(500)] string? description,
        CellConfiguration? configuration,
        CellStyle? style,
        IReadOnlyList<string>? options)
    {
        Name = name;
        Kind = kind;
        Description = description;
        Configuration = configuration;
        Style = style;
        Options = options;
    }

    public string Name { get; init; }

    public CellKind Kind { get; init; }

    public string? Description { get; init; }

    public CellConfiguration? Configuration { get; init; }

    public CellStyle? Style { get; init; }

    public IReadOnlyList<string>? Options { get; init; }
}
