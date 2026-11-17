using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetLinkedTableConfiguration : IEntityTypeConfiguration<DbSheetLinkedTable>
{
    public void Configure(EntityTypeBuilder<DbSheetLinkedTable> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetLinkedTables", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetLinkedTableID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetId)
            .HasColumnName("SheetID")
            .HasColumnType("int");

        builder.Property(x => x.TableReferenceId)
            .HasColumnName("TableReferenceID")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.LinkedByUserId)
            .HasColumnName("LinkedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.LinkedDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.HasOne(x => x.LinkedByUser)
            .WithMany()
            .HasForeignKey(x => x.LinkedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.Revisions)
            .WithOne()
            .HasForeignKey(x => x.SheetLinkedTableId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
