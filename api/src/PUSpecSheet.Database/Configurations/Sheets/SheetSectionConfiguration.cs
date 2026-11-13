using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetSectionConfiguration : IEntityTypeConfiguration<DbSheetSection>
{
    public void Configure(EntityTypeBuilder<DbSheetSection> builder)
    {
        builder.ToTable("SheetSections", DatabaseSchemas.Spec);

        builder.HasKey(section => section.Id);

        builder.HasPublicId(section => section.PublicId);

        builder.Property(section => section.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(section => section.SheetTable)
            .WithMany(table => table.Sections)
            .HasForeignKey(section => section.SheetTableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(section => section.TemplateSection)
            .WithMany()
            .HasForeignKey(section => section.TemplateSectionId)
            .OnDelete(DeleteBehavior.Restrict);

        // SQL Server can't cascade a self-reference; child sections go with their table instead.
        builder.HasOne(section => section.ParentSheetSection)
            .WithMany(section => section.ChildSheetSections)
            .HasForeignKey(section => section.ParentSheetSectionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
