using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetExternalReferenceRevisionConfiguration : IEntityTypeConfiguration<DbSheetExternalReferenceRevision>
{
    public void Configure(EntityTypeBuilder<DbSheetExternalReferenceRevision> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetExternalReferenceRevisions", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetExternalReferenceRevisionID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetExternalReferenceId)
            .HasColumnName("SheetExternalReferenceID")
            .HasColumnType("int");

        builder.Property(x => x.SheetId)
            .HasColumnName("SheetID")
            .HasColumnType("int");

        builder.Property(x => x.SheetVersionId)
            .HasColumnName("SheetVersionID")
            .HasColumnType("int");

        builder.Property(x => x.RevisionNumber)
            .HasColumnType("int");

        builder.Property(x => x.ExternalId)
            .HasColumnName("ExternalID")
            .HasColumnType("int");

        builder.Property(x => x.SystemName)
            .HasColumnType("varchar(256)");

        builder.Property(x => x.OwnerName)
            .HasColumnType("varchar(256)");

        builder.Property(x => x.Url)
            .HasColumnName("URL")
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Type)
            .HasColumnType("varchar(32)");

        builder.Property(x => x.Grouping)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.SystemIconUrl)
            .HasColumnName("SystemIconURL")
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IsDeleted)
            .HasColumnType("bit");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.Property(x => x.PublishedDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.SupersededDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne<DbSheet>()
            .WithMany()
            .HasForeignKey(x => x.SheetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<DbSheetVersion>()
            .WithMany()
            .HasForeignKey(x => x.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
