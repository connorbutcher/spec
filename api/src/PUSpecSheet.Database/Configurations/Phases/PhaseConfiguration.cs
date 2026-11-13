using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.Database.Configurations.Phases;

public sealed class PhaseConfiguration : IEntityTypeConfiguration<DbPhase>
{
    public void Configure(EntityTypeBuilder<DbPhase> builder)
    {
        builder.ToTable("Phases", DatabaseSchemas.Setup);

        builder.HasKey(phase => phase.Id);

        builder.Property(phase => phase.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(phase => phase.Code)
            .IsUnique();

        builder.Property(phase => phase.Description)
            .HasMaxLength(500);

        // A phase that still has children can't be deleted; the children must be moved or removed first.
        builder.HasOne(phase => phase.ParentPhase)
            .WithMany(phase => phase.ChildPhases)
            .HasForeignKey(phase => phase.ParentPhaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
