namespace PUSpecSheet.Database.Models.Templates;

public class DbWidgetType
{
    public int Id { get; set; }

    public Guid Key { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }
}
