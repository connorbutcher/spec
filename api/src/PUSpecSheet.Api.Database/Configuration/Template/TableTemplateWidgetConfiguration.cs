using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Configuration.Template;

internal class TableTemplateWidgetConfiguration : IEntityTypeConfiguration<DbTableTemplateWidget>
{
    public void Configure(EntityTypeBuilder<DbTableTemplateWidget> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("TableTemplateWidgets", "template");

        builder.Property(x => x.Id)
            .HasColumnName("TableTemplateWidgetID")
            .HasColumnType("smallint")
            .UseIdentityColumn();

        builder.Property(x => x.TableTemplateId)
            .HasColumnName("TableTemplateID")
            .HasColumnType("tinyint");

        builder.Property(x => x.WidgetTypeId)
            .HasColumnName("WidgetTypeID")
            .HasColumnType("tinyint");

        builder.HasOne(x => x.WidgetType)
            .WithMany()
            .HasForeignKey(x => x.WidgetTypeId);
    }
}
