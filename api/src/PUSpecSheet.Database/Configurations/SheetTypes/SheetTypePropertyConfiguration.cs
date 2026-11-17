using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Database.Configurations.SheetTypes;

public sealed class SheetTypePropertyConfiguration : IEntityTypeConfiguration<DbSheetTypeProperty>
{
    public void Configure(EntityTypeBuilder<DbSheetTypeProperty> builder)
    {
        builder.ToTable("SheetTypeProperties", DatabaseSchemas.Setup);

        builder.HasKey(property => property.Id);

        builder.HasPublicId(property => property.PublicId);

        builder.Property(property => property.Key)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(property => property.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(property => property.ValueType)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(property => property.ControlType)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(property => property.CanBeSetInForm)
            .HasDefaultValue(true);

        builder.Property(property => property.SaveValueAsProperty)
            .HasDefaultValue(true);

        builder.Property(property => property.Placeholder)
            .HasMaxLength(512);

        builder.HasIndex(property => new { property.SheetTypeId, property.Key })
            .IsUnique();

        builder.HasOne(property => property.SheetType)
            .WithMany(sheetType => sheetType.Properties)
            .HasForeignKey(property => property.SheetTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
