using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetTableConfiguration : IEntityTypeConfiguration<DbSheetTable>
{
    public void Configure(EntityTypeBuilder<DbSheetTable> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetTables", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetTableID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetId)
            .HasColumnName("SheetID")
            .HasColumnType("int");

        builder.Property(x => x.TableTemplateId)
            .HasColumnName("TableTemplateID")
            .HasColumnType("tinyint");

        builder.Property(x => x.ReferenceId)
            .HasColumnName("ReferenceID")
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("newsequentialid()");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.HasOne(x => x.TableTemplate)
            .WithMany()
            .HasForeignKey(x => x.TableTemplateId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.Revisions)
            .WithOne()
            .HasForeignKey(x => x.SheetTableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Groups)
            .WithOne()
            .HasForeignKey(x => x.SheetTableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Widgets)
            .WithOne()
            .HasForeignKey(x => x.SheetTableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Notes)
            .WithOne()
            .HasForeignKey(x => x.SheetTableId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
