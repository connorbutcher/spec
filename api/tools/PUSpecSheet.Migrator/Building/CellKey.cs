namespace PUSpecSheet.Migrator.Building;

/// <summary>Identifies a cell across copies: its row, its template cell, and its column block copy if it has one.</summary>
internal readonly record struct CellKey(int RowKey, int TemplateCellId, int? ColumnBlockKey);
