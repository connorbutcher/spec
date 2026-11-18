using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.Database.Configuration.Legacy.SpecSheets;

internal class SpecSheetLinkedTableConfiguration : IEntityTypeConfiguration<DbSpecSheetLinkedTable>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetLinkedTable> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetLinkedTables", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetLinkedTableID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetId)
            .HasColumnName("SpecSheetID")
            .HasColumnType("int");

        builder.Property(x => x.DisplayOrder)
            .HasColumnType("smallint");

        builder.Property(x => x.TableReferenceId)
            .HasColumnName("TableReferenceID")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.LinkedByUserId)
            .HasColumnName("LinkedByUserID")
            .HasColumnType("smallint");

        builder.HasOne(builder => builder.LinkedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.LinkedByUserId);
    }
}
