using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetColumnBlockConfiguration : IEntityTypeConfiguration<DbSheetColumnBlock>
{
    public void Configure(EntityTypeBuilder<DbSheetColumnBlock> builder)
    {
        builder.ToTable("SheetColumnBlocks");

        builder.HasKey(block => block.Id);

        builder.HasPublicId(block => block.PublicId);

        builder.Property(block => block.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(block => block.SheetTable)
            .WithMany(table => table.ColumnBlocks)
            .HasForeignKey(block => block.SheetTableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(block => block.TemplateColumnBlock)
            .WithMany()
            .HasForeignKey(block => block.TemplateColumnBlockId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
