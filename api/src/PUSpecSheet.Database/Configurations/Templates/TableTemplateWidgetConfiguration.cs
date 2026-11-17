using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Configurations.Templates;

public sealed class TableTemplateWidgetConfiguration : IEntityTypeConfiguration<DbTableTemplateWidget>
{
    public void Configure(EntityTypeBuilder<DbTableTemplateWidget> builder)
    {
        builder.ToTable("TableTemplateWidgets", DatabaseSchemas.Design);

        builder.HasKey(widget => widget.Id);

        builder.HasOne(widget => widget.TableTemplateVersion)
            .WithMany(version => version.Widgets)
            .HasForeignKey(widget => widget.TableTemplateVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(widget => widget.WidgetType)
            .WithMany()
            .HasForeignKey(widget => widget.WidgetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
