using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetTableWidgetConfiguration : IEntityTypeConfiguration<DbSheetTableWidget>
{
    public void Configure(EntityTypeBuilder<DbSheetTableWidget> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetTableWidgets", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetTableWidgetID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetTableId)
            .HasColumnName("SheetTableID")
            .HasColumnType("int");

        builder.Property(x => x.TableTemplateWidgetId)
            .HasColumnName("TableTemplateWidgetID")
            .HasColumnType("smallint");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.HasOne(x => x.TableTemplateWidget)
            .WithMany()
            .HasForeignKey(x => x.TableTemplateWidgetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
