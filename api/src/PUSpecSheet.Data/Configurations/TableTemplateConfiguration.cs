using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Configurations;

public sealed class TableTemplateConfiguration : IEntityTypeConfiguration<TableTemplate>
{
    public void Configure(EntityTypeBuilder<TableTemplate> builder)
    {
        builder.ToTable("TableTemplates");

        builder.HasKey(template => template.Id);

        builder.Property(template => template.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(template => template.Orientation)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(template => new { template.SheetTypeId, template.Name })
            .IsUnique();

        // A sheet type that still has templates can't be deleted.
        builder.HasOne(template => template.SheetType)
            .WithMany()
            .HasForeignKey(template => template.SheetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
