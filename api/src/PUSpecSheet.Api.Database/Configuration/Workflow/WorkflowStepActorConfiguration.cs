using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Workflow;

namespace PUSpecSheet.Api.Database.Configuration.Workflow;

internal class WorkflowStepActorsConfiguration : IEntityTypeConfiguration<DbWorkflowStepActor>
{
    public void Configure(EntityTypeBuilder<DbWorkflowStepActor> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("WorkflowStepActors", "workflow");

        builder.Property(x => x.Id)
            .HasColumnName("WorkflowStepActorID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.ActorUserId)
            .HasColumnType("smallint");

        builder.Property(x => x.State)
            .HasColumnType("tinyint");

        builder.Property(x => x.Comment)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.ActorUser)
            .WithMany()
            .HasForeignKey(x => x.ActorUserId);
    }
}
