namespace PUSpecSheet.Database.Models.Templates;

public class DbTableTemplateWidget
{
    public int Id { get; set; }

    public int TableTemplateVersionId { get; set; }

    public int WidgetTypeId { get; set; }

    public virtual DbTableTemplateVersion TableTemplateVersion { get; set; } = null!;

    public virtual DbWidgetType WidgetType { get; set; } = null!;
}
