namespace PUSpecSheet.Domain.CellTypes;

/// <summary>One choice of a dropdown <see cref="CellType"/>.</summary>
public class CellTypeOption
{
    public int Id { get; set; }

    public int CellTypeId { get; set; }

    public CellType CellType { get; set; } = null!;

    public string Value { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}
