using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetTableGroupWidgetConfiguration : IEntityTypeConfiguration<DbSheetTableGroupWidget>
{
    public void Configure(EntityTypeBuilder<DbSheetTableGroupWidget> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetTableGroupWidgets", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetTableGroupWidgetID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetTableGroupId)
            .HasColumnName("SheetTableGroupID")
            .HasColumnType("int");

        builder.Property(x => x.TableGroupTemplateWidgetId)
            .HasColumnName("TableGroupTemplateWidgetID")
            .HasColumnType("smallint");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.HasOne(x => x.TableGroupTemplateWidget)
            .WithMany()
            .HasForeignKey(x => x.TableGroupTemplateWidgetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
