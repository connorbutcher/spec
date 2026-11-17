using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Api.Database.Configuration.SpecSheets;

internal class SpecSheetConfiguration : IEntityTypeConfiguration<DbSpecSheet>
{
    public void Configure(EntityTypeBuilder<DbSpecSheet> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheets", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.LegacySpecSheetId)
            .HasColumnName("LegacySpecSheetID")
            .HasColumnType("int");

        builder.Property(x => x.SpecSheetPhaseId)
            .HasColumnName("SpecSheetPhaseID")
            .HasColumnType("smallint");

        builder.Property(x => x.SpecSheetTypeId)
            .HasColumnName("SpecSheetTypeID")
            .HasColumnType("tinyint");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("CreatedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.PublishedByUserId)
            .HasColumnName("PublishedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.ChangeLog)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(builder => builder.CreatedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.CreatedByUserId);

        builder.HasOne(builder => builder.PublishedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.PublishedByUserId);

        builder.HasOne(builder => builder.SpecSheetPhase)
            .WithMany()
            .HasForeignKey(builder => builder.SpecSheetPhaseId);

        builder.HasOne(builder => builder.SpecSheetType)
            .WithMany()
            .HasForeignKey(builder => builder.SpecSheetTypeId);

        builder.HasMany(builder => builder.Tables)
            .WithOne()
            .HasForeignKey(builder => builder.SpecSheetId);

        builder.HasMany(builder => builder.PropertyValues)
            .WithOne()
            .HasForeignKey(builder => builder.SpecSheetId);

        builder.HasMany(builder => builder.WorkflowSteps)
            .WithOne()
            .HasForeignKey(builder => builder.SpecSheetId);

        builder.HasMany(builder => builder.ExternalReferences)
            .WithOne()
            .HasForeignKey(builder => builder.SpecSheetId);
    }
}
