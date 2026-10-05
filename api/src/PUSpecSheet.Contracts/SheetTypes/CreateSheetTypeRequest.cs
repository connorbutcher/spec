using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.SheetTypes;

/// <summary>Adds a sheet type to the end of the list.</summary>
public sealed record CreateSheetTypeRequest([Required, MaxLength(100)] string Name);
