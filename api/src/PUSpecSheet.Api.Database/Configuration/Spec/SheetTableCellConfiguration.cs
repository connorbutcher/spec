using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetTableCellConfiguration : IEntityTypeConfiguration<DbSheetTableCell>
{
    public void Configure(EntityTypeBuilder<DbSheetTableCell> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetTableCells", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetTableCellID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetTableRowId)
            .HasColumnName("SheetTableRowID")
            .HasColumnType("int");

        builder.Property(x => x.TableCellTemplateId)
            .HasColumnName("TableCellTemplateID")
            .HasColumnType("int");

        builder.Property(x => x.ReferenceId)
            .HasColumnName("ReferenceID")
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("newsequentialid()");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.HasOne(x => x.TableCellTemplate)
            .WithMany()
            .HasForeignKey(x => x.TableCellTemplateId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
