using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Configuration.Access;

public class SpecSheetPhaseObserverConfiguration : IEntityTypeConfiguration<DbSpecSheetPhaseObserver>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetPhaseObserver> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetPhaseObservers", "access");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetPhaseObserverID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetPhaseId)
            .HasColumnName("SpecSheetPhaseID")
            .HasColumnType("smallint");

        builder.Property(x => x.UserId)
            .HasColumnName("UserID")
            .HasColumnType("smallint");
        builder.Property(x => x.IsApprover)
            .HasColumnName("IsApprover")
            .HasColumnType("bit");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}
