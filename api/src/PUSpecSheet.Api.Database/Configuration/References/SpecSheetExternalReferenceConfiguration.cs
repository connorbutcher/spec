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
    }
}
