using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetTableGroupRevisionConfiguration : IEntityTypeConfiguration<DbSheetTableGroupRevision>
{
    public void Configure(EntityTypeBuilder<DbSheetTableGroupRevision> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetTableGroupRevisions", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetTableGroupRevisionID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetTableGroupId)
            .HasColumnName("SheetTableGroupID")
            .HasColumnType("int");

        builder.Property(x => x.SheetId)
            .HasColumnName("SheetID")
            .HasColumnType("int");

        builder.Property(x => x.RevisionNumber)
            .HasColumnType("int");

        builder.Property(x => x.Status)
            .HasColumnType("tinyint");

        builder.Property(x => x.DisplayOrder)
            .HasColumnType("int");

        builder.Property(x => x.LinkedSheetTableGroupReferenceId)
            .HasColumnName("LinkedSheetTableGroupReferenceID")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.WidgetValues)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<ICollection<SheetWidgetValue>>(v, JsonSerializerSettings.DeserializerSettings));

        builder.Property(x => x.IsDeleted)
            .HasColumnType("bit");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("CreatedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.Property(x => x.LastModifiedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.Property(x => x.PublishedDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.SupersededDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.SheetVersionId)
            .HasColumnName("SheetVersionID")
            .HasColumnType("int");

        builder.Property(x => x.SheetSubmissionId)
            .HasColumnName("SheetSubmissionID")
            .HasColumnType("int");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<DbSheet>()
            .WithMany()
            .HasForeignKey(x => x.SheetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.SheetVersion)
            .WithMany()
            .HasForeignKey(x => x.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<DbSheetSubmission>()
            .WithMany()
            .HasForeignKey(x => x.SheetSubmissionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
