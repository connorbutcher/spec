using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Api.Database.Configuration.SpecSheets;

internal class SpecSheetTableGroupConfiguration : IEntityTypeConfiguration<DbSpecSheetTableGroup>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTableGroup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTableGroups", "dbo");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTableGroupID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableId)
            .HasColumnName("SpecSheetTableID")
            .HasColumnType("int");

        builder.Property(x => x.TableGroupTemplateId)
            .HasColumnName("TableGroupTemplateID")
            .HasColumnType("int");

        builder.Property(x => x.LinkedSpecSheetTableGroupId)
            .HasColumnName("LinkedSpecSheetTableGroupID")
            .HasColumnType("int");

        builder.Property(x => x.Order)
            .HasColumnType("int");

        builder.Property(x => x.ReferenceId)
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("CreatedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.LastModifiedByUserId)
            .HasColumnName("LastModifiedByUserID")
            .HasColumnType("smallint");

        builder.HasOne(builder => builder.TableGroupTemplate)
            .WithMany()
            .HasForeignKey(builder => builder.TableGroupTemplateId);

        builder.HasMany(builder => builder.Rows)
            .WithOne()
            .HasForeignKey(builder => builder.SpecSheetTableGroupId);

        builder.HasMany(builder => builder.Widgets)
            .WithOne()
            .HasForeignKey(builder => builder.SpecSheetTableGroupId);

        builder.HasOne(builder => builder.LinkedSpecSheetTableGroup)
            .WithMany()
            .HasForeignKey(builder => builder.LinkedSpecSheetTableGroupId);

        builder.HasOne(builder => builder.CreatedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.CreatedByUserId);

        builder.HasOne(builder => builder.LastModifiedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.LastModifiedByUserId);
    }
}
