namespace PUSpecSheet.Database.Models.SheetTypes;

public class DbSheetTypeProperty
{
    public int Id { get; set; }

    public Guid PublicId { get; set; }

    public int SheetTypeId { get; set; }

    public required string Key { get; set; }

    public required string Name { get; set; }

    public required string ValueType { get; set; }

    public required string ControlType { get; set; }

    public bool CanBeSetInForm { get; set; } = true;

    public bool SaveValueAsProperty { get; set; } = true;

    public int DisplayOrder { get; set; }

    public string? Placeholder { get; set; }

    public string? ValidatorConfiguration { get; set; }

    public virtual DbSheetType SheetType { get; set; } = null!;
}
