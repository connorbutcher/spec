using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetTableGroupConfiguration : IEntityTypeConfiguration<DbSheetTableGroup>
{
    public void Configure(EntityTypeBuilder<DbSheetTableGroup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetTableGroups", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetTableGroupID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetTableId)
            .HasColumnName("SheetTableID")
            .HasColumnType("int");

        builder.Property(x => x.TableGroupTemplateId)
            .HasColumnName("TableGroupTemplateID")
            .HasColumnType("int");

        builder.Property(x => x.ReferenceId)
            .HasColumnName("ReferenceID")
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("newsequentialid()");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.HasOne(x => x.TableGroupTemplate)
            .WithMany()
            .HasForeignKey(x => x.TableGroupTemplateId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.Revisions)
            .WithOne()
            .HasForeignKey(x => x.SheetTableGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Rows)
            .WithOne()
            .HasForeignKey(x => x.SheetTableGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
