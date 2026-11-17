using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetExternalReferenceConfiguration : IEntityTypeConfiguration<DbSheetExternalReference>
{
    public void Configure(EntityTypeBuilder<DbSheetExternalReference> builder)
    {
        builder.ToTable("SheetExternalReferences", DatabaseSchemas.Spec);

        builder.HasKey(reference => reference.Id);

        builder.HasPublicId(reference => reference.PublicId);

        builder.Property(reference => reference.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(reference => new { reference.SheetId, reference.ReferenceId })
            .IsUnique();

        builder.HasOne(reference => reference.Sheet)
            .WithMany(sheet => sheet.ExternalReferences)
            .HasForeignKey(reference => reference.SheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
