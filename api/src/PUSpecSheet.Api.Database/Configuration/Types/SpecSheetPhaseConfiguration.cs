using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Configuration.Types;

internal class SpecSheetPhaseConfiguration : IEntityTypeConfiguration<DbSpecSheetPhase>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetPhase> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetPhases", "types");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetPhaseID")
            .HasColumnType("smallint")
            .UseIdentityColumn();

        builder.Property(x => x.ParentSpecSheetPhaseId)
            .HasColumnName("ParentSpecSheetPhaseID")
            .HasColumnType("smallint");

        builder.Property(x => x.Phase)
            .HasColumnType("varchar(64)");

        builder.Property(x => x.PhaseShort)
            .HasColumnType("varchar(8)");

        builder.Property(x => x.DisplayOrder)
            .HasColumnType("smallint");

        builder.HasOne(builder => builder.ParentSpecSheetPhase)
            .WithMany()
            .HasForeignKey(builder => builder.ParentSpecSheetPhaseId);

        builder.HasMany(x => x.PhaseObservers)
            .WithOne()
            .HasForeignKey(x => x.SpecSheetPhaseId);

        builder.HasMany(x => x.SpecSheetTypes)
            .WithMany(x => x.Phases)
            .UsingEntity<Dictionary<string, object>>(
                "SpecSheetTypePhase",
                j => j
                    .HasOne<DbSpecSheetType>()
                    .WithMany()
                    .HasForeignKey("SpecSheetTypeID")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<DbSpecSheetPhase>()
                    .WithMany()
                    .HasForeignKey("SpecSheetPhaseID")
                    .OnDelete(DeleteBehavior.ClientCascade),
                j =>
                {
                    j.ToTable("SpecSheetTypePhases", "types");
                    j.HasKey("SpecSheetTypeID", "SpecSheetPhaseID");
                });
    }
}
