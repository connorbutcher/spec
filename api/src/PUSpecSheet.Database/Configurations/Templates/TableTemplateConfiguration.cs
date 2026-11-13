using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Configurations.Templates;

public sealed class TableTemplateConfiguration : IEntityTypeConfiguration<DbTableTemplate>
{
    public void Configure(EntityTypeBuilder<DbTableTemplate> builder)
    {
        builder.ToTable("TableTemplates", DatabaseSchemas.Template);

        builder.HasKey(template => template.Id);

        builder.Property(template => template.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(template => new { template.SheetTypeId, template.Name })
            .IsUnique();

        // A sheet type that still has templates can't be deleted.
        builder.HasOne(template => template.SheetType)
            .WithMany()
            .HasForeignKey(template => template.SheetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
