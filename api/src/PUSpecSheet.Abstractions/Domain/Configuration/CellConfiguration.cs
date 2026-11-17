using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Abstractions.Domain.Styling;

namespace PUSpecSheet.Abstractions.Domain.Configuration;

public record CellConfiguration
{
    public required int Id { get; init; }

    public required CellStyling Styling { get; init; }

    public required ICellProperties Properties { get; init; }

    public required ICollection<Validator>? Validators { get; init; }

    public required bool IsIdentifier { get; init; }
}
