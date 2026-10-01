using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>Changes cell values in one row. Saving locks the row to its author until they publish or discard.</summary>
public sealed record SaveRowValuesRequest([MinLength(1)] IReadOnlyList<CellValueRequest> Values);
