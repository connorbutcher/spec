using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Phases;

namespace PUSpecSheet.Data.Configurations;

public sealed class PhaseConfiguration : IEntityTypeConfiguration<Phase>
{
    public void Configure(EntityTypeBuilder<Phase> builder)
    {
        builder.ToTable("Phases", table =>
        {
            // Longer loops (a phase under its own child) are refused by the service when a phase is moved.
            table.HasCheckConstraint("CK_Phases_NotItsOwnParent", "[ParentPhaseId] <> [Id]");
        });

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
