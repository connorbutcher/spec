using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Types;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.Database.Configuration.Values;

internal class LinkedSpecSheetValueConfiguration : IEntityTypeConfiguration<DbLinkedSpecSheetValue>
{
    public void Configure(EntityTypeBuilder<DbLinkedSpecSheetValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("LinkedSpecSheetValues", "values");

        builder.Property(x => x.Id)
            .HasColumnName("LinkedSpecSheetValueID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableCellId)
            .HasColumnName("SpecSheetTableCellID")
            .HasColumnType("int");

        builder.Property(x => x.TableReferenceId)
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.LinkedSpecSheetTypeId)
            .HasColumnName("LinkedSpecSheetTypeID")
            .HasColumnType("tinyint");

        builder.Property(x => x.LinkedSpecSheetPhaseId)
            .HasColumnName("LinkedSpecSheetPhaseID")
            .HasColumnType("smallint");

        builder.Property(x => x.LinkedReferenceId)
            .HasColumnType("uniqueidentifier");

        builder.HasOne<DbSpecSheetType>()
            .WithMany()
            .HasForeignKey(x => x.LinkedSpecSheetTypeId);

        builder.HasOne<DbSpecSheetPhase>()
            .WithMany()
            .HasForeignKey(x => x.LinkedSpecSheetPhaseId);
    }
}