using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.Database.Configuration.Legacy.SpecSheets;

internal class SpecSheetTableRowConfiguration : IEntityTypeConfiguration<DbSpecSheetTableRow>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTableRow> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTableRows", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTableRowID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableGroupId)
            .HasColumnName("SpecSheetTableGroupID")
            .HasColumnType("int");

        builder.Property(x => x.TableRowTemplateId)
            .HasColumnName("TableRowTemplateID")
            .HasColumnType("int");

        builder.Property(x => x.RowNumber)
            .HasColumnType("tinyint");

        builder.Property(x => x.ReferenceId)
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("CreatedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.LastModifiedByUserId)
            .HasColumnName("LastModifiedByUserID")
            .HasColumnType("smallint");

        builder.HasMany(builder => builder.Cells)
            .WithOne()
            .HasForeignKey(builder => builder.SpecSheetTableRowId);

        builder.HasOne(builder => builder.TableRowTemplate)
            .WithMany()
            .HasForeignKey(builder => builder.TableRowTemplateId);

        builder.HasOne(builder => builder.CreatedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.CreatedByUserId);

        builder.HasOne(builder => builder.LastModifiedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.LastModifiedByUserId);
    }
}
