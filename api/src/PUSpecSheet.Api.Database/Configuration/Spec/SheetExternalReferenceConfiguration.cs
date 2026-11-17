using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetExternalReferenceConfiguration : IEntityTypeConfiguration<DbSheetExternalReference>
{
    public void Configure(EntityTypeBuilder<DbSheetExternalReference> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetExternalReferences", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetExternalReferenceID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetId)
            .HasColumnName("SheetID")
            .HasColumnType("int");

        builder.Property(x => x.ReferenceId)
            .HasColumnName("ReferenceID")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.HasMany(x => x.Revisions)
            .WithOne()
            .HasForeignKey(x => x.SheetExternalReferenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
