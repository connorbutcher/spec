using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>Sets a table's title. An empty title falls back to the template's name.</summary>
public sealed record UpdateSheetTableRequest([StringLength(200)] string? Title);
