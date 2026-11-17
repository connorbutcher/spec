using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetLinkedTableConfiguration : IEntityTypeConfiguration<DbSheetLinkedTable>
{
    public void Configure(EntityTypeBuilder<DbSheetLinkedTable> builder)
    {
        builder.ToTable("SheetLinkedTables", DatabaseSchemas.Spec);

        builder.HasKey(linked => linked.Id);

        builder.HasPublicId(linked => linked.PublicId);

        builder.HasIndex(linked => new { linked.SheetId, linked.LinkedTablePublicId })
            .IsUnique();

        builder.Property(linked => linked.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(linked => linked.Sheet)
            .WithMany(sheet => sheet.LinkedTables)
            .HasForeignKey(linked => linked.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(linked => linked.LinkedBy)
            .WithMany()
            .HasForeignKey(linked => linked.LinkedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
