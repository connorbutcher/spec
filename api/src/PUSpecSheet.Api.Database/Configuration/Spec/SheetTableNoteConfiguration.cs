using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetTableNoteConfiguration : IEntityTypeConfiguration<DbSheetTableNote>
{
    public void Configure(EntityTypeBuilder<DbSheetTableNote> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetTableNotes", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetTableNoteID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetTableId)
            .HasColumnName("SheetTableID")
            .HasColumnType("int");

        builder.Property(x => x.SourceSystem)
            .HasColumnType("varchar(16)");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("CreatedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        // The database has no foreign key for the author (see database/README.md), as for a copy's notes.
        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
