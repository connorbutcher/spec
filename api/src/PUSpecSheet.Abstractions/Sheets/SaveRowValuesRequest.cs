using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>Changes cell values in one row. Saving locks the row to its author until they publish it or change it back.</summary>
public sealed record SaveRowValuesRequest
{
    public SaveRowValuesRequest([MinLength(1)] IReadOnlyList<CellValueRequest> values)
    {
        Values = values;
    }

    public IReadOnlyList<CellValueRequest> Values { get; init; }
}
