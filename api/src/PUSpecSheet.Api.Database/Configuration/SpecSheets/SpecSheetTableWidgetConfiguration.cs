using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.Database.Configuration.SpecSheets;

internal class SpecSheetTableWidgetConfiguration : IEntityTypeConfiguration<DbSpecSheetTableWidget>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTableWidget> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTableWidgets", "dbo");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTableWidgetID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableId)
            .HasColumnName("SpecSheetTableID")
            .HasColumnType("int");

        builder.Property(x => x.TableTemplateWidgetId)
            .HasColumnName("TableTemplateWidgetID")
            .HasColumnType("smallint");

        builder.HasOne(x => x.TableTemplateWidget)
            .WithMany()
            .HasForeignKey(x => x.TableTemplateWidgetId);

        builder.HasOne(x => x.Value)
            .WithOne()
            .HasForeignKey<DbTableWidgetValue>(x => x.SpecSheetTableWidgetId);
    }
}
