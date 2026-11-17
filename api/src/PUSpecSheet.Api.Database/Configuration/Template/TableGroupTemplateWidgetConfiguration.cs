using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Configuration.Template;

internal class TableGroupTemplateWidgetConfiguration : IEntityTypeConfiguration<DbTableGroupTemplateWidget>
{
    public void Configure(EntityTypeBuilder<DbTableGroupTemplateWidget> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("TableGroupTemplateWidgets", "template");

        builder.Property(x => x.Id)
            .HasColumnName("TableGroupTemplateWidgetID")
            .HasColumnType("smallint")
            .UseIdentityColumn();

        builder.Property(x => x.TableGroupTemplateId)
            .HasColumnName("TableGroupTemplateID")
            .HasColumnType("int");

        builder.Property(x => x.WidgetTypeId)
            .HasColumnName("WidgetTypeID")
            .HasColumnType("tinyint");

        builder.HasOne(x => x.WidgetType)
            .WithMany()
            .HasForeignKey(x => x.WidgetTypeId);
    }
}
