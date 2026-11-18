using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.Database.Configuration.Legacy.SpecSheets;

internal class SpecSheetTableNoteConfiguration : IEntityTypeConfiguration<DbSpecSheetTableNote>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTableNote> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTableNotes", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTableNoteID")
            .HasColumnType("int")

            .UseIdentityColumn();
        builder.Property(x => x.SpecSheetTableId)
            .HasColumnName("SpecSheetTableID")
            .HasColumnType("int");

        builder.Property(x => x.Note)
            .HasColumnName("Note")
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.SourceSystem)
            .HasColumnName("SourceSystem")
            .HasColumnType("varchar(16)");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("CreatedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnName("CreatedDateTime")
            .HasColumnType("datetime2");

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId);
    }
}
