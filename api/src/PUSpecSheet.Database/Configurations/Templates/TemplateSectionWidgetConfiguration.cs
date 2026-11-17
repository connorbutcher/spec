using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Configurations.Templates;

public sealed class TemplateSectionWidgetConfiguration : IEntityTypeConfiguration<DbTemplateSectionWidget>
{
    public void Configure(EntityTypeBuilder<DbTemplateSectionWidget> builder)
    {
        builder.ToTable("TemplateSectionWidgets", DatabaseSchemas.Design);

        builder.HasKey(widget => widget.Id);

        builder.HasOne(widget => widget.TemplateSection)
            .WithMany(section => section.Widgets)
            .HasForeignKey(widget => widget.TemplateSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(widget => widget.WidgetType)
            .WithMany()
            .HasForeignKey(widget => widget.WidgetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
