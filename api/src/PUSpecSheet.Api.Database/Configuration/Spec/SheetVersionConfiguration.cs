using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetVersionConfiguration : IEntityTypeConfiguration<DbSheetVersion>
{
    public void Configure(EntityTypeBuilder<DbSheetVersion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetVersions", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetVersionID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetId)
            .HasColumnName("SheetID")
            .HasColumnType("int");

        builder.Property(x => x.VersionNumber)
            .HasColumnType("int");

        builder.Property(x => x.PublishedDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.PublishedByUserId)
            .HasColumnName("PublishedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.ChangeLog)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.SourceSpecSheetId)
            .HasColumnName("SourceSpecSheetID")
            .HasColumnType("int");

        builder.Property(x => x.LegacySpecSheetId)
            .HasColumnName("LegacySpecSheetID")
            .HasColumnType("int");

        builder.Property(x => x.HasBeenMigrated)
            .HasColumnType("bit");

        builder.HasOne(x => x.PublishedByUser)
            .WithMany()
            .HasForeignKey(x => x.PublishedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
