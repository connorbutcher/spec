using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Phases;
using PUSpecSheet.Database.Models.SheetTypes;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.Database.Configurations.Values;

public sealed class LinkedSheetValueConfiguration : CellValueConfiguration<DbLinkedSheetValue>
{
    protected override string TableName => "LinkedSheetValues";

    protected override void ConfigureValue(EntityTypeBuilder<DbLinkedSheetValue> builder)
    {
        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.LinkedSheetValues)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<DbSheetType>()
            .WithMany()
            .HasForeignKey(value => value.LinkedSheetTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<DbPhase>()
            .WithMany()
            .HasForeignKey(value => value.LinkedPhaseId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
