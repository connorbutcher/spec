using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Configurations;

public sealed class TemplateRowConfiguration : IEntityTypeConfiguration<TemplateRow>
{
    public void Configure(EntityTypeBuilder<TemplateRow> builder)
    {
        builder.ToTable("TemplateRows");

        builder.HasKey(row => row.Id);

        builder.HasOne(row => row.TemplateSection)
            .WithMany(section => section.Rows)
            .HasForeignKey(row => row.TemplateSectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
