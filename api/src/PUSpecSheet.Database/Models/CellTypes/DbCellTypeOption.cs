namespace PUSpecSheet.Database.Models.CellTypes;

/// <summary>One choice of a dropdown <see cref="DbCellType"/>.</summary>
public class DbCellTypeOption
{
    public int Id { get; set; }

    public int CellTypeId { get; set; }

    public DbCellType CellType { get; set; } = null!;

    public string Value { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}
