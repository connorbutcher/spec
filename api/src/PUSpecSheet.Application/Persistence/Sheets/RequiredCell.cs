using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>A sheet cell whose template cell must be filled in: which cell, its row and its kind.</summary>
public sealed record RequiredCell(int Id, int SheetRowId, CellKind Kind);
