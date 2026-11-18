using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Legacy.Workflow;

namespace PUSpecSheet.Api.Database.Configuration.Legacy.Workflow;

internal class WorkflowStepsConfiguration : IEntityTypeConfiguration<DbWorkflowStep>
{
    public void Configure(EntityTypeBuilder<DbWorkflowStep> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("WorkflowSteps", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("WorkflowStepID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.State)
            .HasColumnType("tinyint");

        builder.HasMany(x => x.Actors)
            .WithOne()
            .HasForeignKey(x => x.WorkflowStepId);
    }
}
