using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.References;

namespace PUSpecSheet.Api.Database.Configuration.References;

internal record SpecSheetExternalReferenceConfiguration : IEntityTypeConfiguration<DbSpecSheetExternalReference>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetExternalReference> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetExternalReferences", "references");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetExternalReferenceID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetId)
            .HasColumnName("SpecSheetID")
            .HasColumnType("int");

        builder.Property(x => x.ExternalId)
            .HasColumnName("ExternalID")
            .HasColumnType("int");

        builder.Property(x => x.ReferenceId)
            .HasColumnName("ReferenceID")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.SystemName)
            .HasColumnType("varchar(256)");

        builder.Property(x => x.OwnerName)
            .HasColumnType("varchar(256)");

        builder.Property(x => x.Url)
            .HasColumnName("URL")
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Type)
            .HasColumnType("varchar(32)");

        builder.Property(x => x.SystemIconUrl)
            .HasColumnName("SystemIconURL")
            .HasColumnType("nvarchar(max)");
    }
}
