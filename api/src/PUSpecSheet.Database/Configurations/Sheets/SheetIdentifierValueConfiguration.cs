using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetIdentifierValueConfiguration : IEntityTypeConfiguration<DbSheetIdentifierValue>
{
    public void Configure(EntityTypeBuilder<DbSheetIdentifierValue> builder)
    {
        builder.ToTable("SheetIdentifierValues", DatabaseSchemas.Spec);

        builder.HasKey(value => new { value.SheetId, value.SheetTypeIdentifierId });

        builder.Property(value => value.Value)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasOne(value => value.Sheet)
            .WithMany(sheet => sheet.IdentifierValues)
            .HasForeignKey(value => value.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        // An identifier some sheet has a value for can't be deleted.
        builder.HasOne(value => value.Identifier)
            .WithMany()
            .HasForeignKey(value => value.SheetTypeIdentifierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
