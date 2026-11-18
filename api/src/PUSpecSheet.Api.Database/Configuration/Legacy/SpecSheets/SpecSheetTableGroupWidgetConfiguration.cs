using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.Database.Configuration.Legacy.SpecSheets;

internal class SpecSheetTableGroupWidgetConfiguration : IEntityTypeConfiguration<DbSpecSheetTableGroupWidget>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTableGroupWidget> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTableGroupWidgets", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTableGroupWidgetID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableGroupId)
            .HasColumnName("SpecSheetTableGroupID")
            .HasColumnType("int");

        builder.Property(x => x.TableGroupTemplateWidgetId)
            .HasColumnName("TableGroupTemplateWidgetID")
            .HasColumnType("smallint");

        builder.HasOne(x => x.TableGroupTemplateWidget)
            .WithMany()
            .HasForeignKey(x => x.TableGroupTemplateWidgetId);

        builder.HasOne(x => x.Value)
            .WithOne()
            .HasForeignKey<DbTableGroupWidgetValue>(x => x.SpecSheetTableGroupWidgetId);
    }
}
