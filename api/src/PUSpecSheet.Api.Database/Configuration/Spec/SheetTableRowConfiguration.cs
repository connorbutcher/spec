using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetTableRowConfiguration : IEntityTypeConfiguration<DbSheetTableRow>
{
    public void Configure(EntityTypeBuilder<DbSheetTableRow> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetTableRows", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetTableRowID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetTableGroupId)
            .HasColumnName("SheetTableGroupID")
            .HasColumnType("int");

        builder.Property(x => x.TableRowTemplateId)
            .HasColumnName("TableRowTemplateID")
            .HasColumnType("int");

        builder.Property(x => x.ReferenceId)
            .HasColumnName("ReferenceID")
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("newsequentialid()");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.HasOne(x => x.TableRowTemplate)
            .WithMany()
            .HasForeignKey(x => x.TableRowTemplateId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.Revisions)
            .WithOne()
            .HasForeignKey(x => x.SheetTableRowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Cells)
            .WithOne()
            .HasForeignKey(x => x.SheetTableRowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
