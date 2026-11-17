namespace PUSpecSheet.Database.Models.CellTypes;

/// <summary>One choice of a dropdown <see cref="DbCellType"/>.</summary>
public class DbCellTypeOption
{
    public int Id { get; set; }

    public int CellTypeId { get; set; }

    public required string Value { get; set; }

    public int DisplayOrder { get; set; }

    public virtual DbCellType CellType { get; set; } = null!;
}
