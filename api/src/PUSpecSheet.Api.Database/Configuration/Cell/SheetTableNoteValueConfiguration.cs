using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Cell;

internal class SheetTableNoteValueConfiguration : IEntityTypeConfiguration<DbSheetTableNoteValue>
{
    public void Configure(EntityTypeBuilder<DbSheetTableNoteValue> builder)
    {
        builder.HasKey(x => new { x.SheetTableRevisionId, x.SheetTableNoteId });
        builder.ToTable("TableNoteValues", "cell");

        builder.Property(x => x.SheetTableRevisionId)
            .HasColumnName("SheetTableRevisionID")
            .HasColumnType("int");

        builder.Property(x => x.SheetTableNoteId)
            .HasColumnName("SheetTableNoteID")
            .HasColumnType("int");

        builder.Property(x => x.Note)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IsObsolete)
            .HasColumnType("bit");

        builder.HasOne(x => x.SheetTableNote)
            .WithMany()
            .HasForeignKey(x => x.SheetTableNoteId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
