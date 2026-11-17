namespace PUSpecSheet.Database.Models.Templates;

public class DbTemplateSectionWidget
{
    public int Id { get; set; }

    public int TemplateSectionId { get; set; }

    public int WidgetTypeId { get; set; }

    public virtual DbTemplateSection TemplateSection { get; set; } = null!;

    public virtual DbWidgetType WidgetType { get; set; } = null!;
}
