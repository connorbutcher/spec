using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Conversions;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Database.Configurations.SheetTypes;

public sealed class SheetTypeIdentifierConfiguration : IEntityTypeConfiguration<DbSheetTypeIdentifier>
{
    public void Configure(EntityTypeBuilder<DbSheetTypeIdentifier> builder)
    {
        builder.ToTable("SheetTypeIdentifiers", DatabaseSchemas.Setup);

        builder.HasKey(identifier => identifier.Id);

        builder.Property(identifier => identifier.Key)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(identifier => identifier.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(identifier => identifier.Options)
            .HasConversion(new StringListJsonConverter(), new StringListComparer())
            .IsRequired(false);

        builder.HasIndex(identifier => new { identifier.SheetTypeId, identifier.Key })
            .IsUnique();

        builder.HasIndex(identifier => new { identifier.SheetTypeId, identifier.Name })
            .IsUnique();

        builder.HasOne(identifier => identifier.SheetType)
            .WithMany(sheetType => sheetType.Identifiers)
            .HasForeignKey(identifier => identifier.SheetTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
