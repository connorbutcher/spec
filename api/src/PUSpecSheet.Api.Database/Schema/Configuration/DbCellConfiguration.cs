using PUSpecSheet.Abstractions.Domain.Configuration;
using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Abstractions.Domain.Styling;

namespace PUSpecSheet.Api.Database.Schema.Configuration;

public record DbCellConfiguration
{
    public required int Id { get; init; }

    public required CellStyling Styling { get; init; }

    public required ICellProperties Properties { get; init; }

    public required ICollection<Validator>? Validators { get; init; }

    public required bool IsIdentifier { get; init; }
}
