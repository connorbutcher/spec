using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.SpecSheets;

internal class SpecSheetTableConfiguration : IEntityTypeConfiguration<DbSpecSheetTable>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTable> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTables", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTableID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetId)
            .HasColumnName("SpecSheetID")
            .HasColumnType("int");

        builder.Property(x => x.TableTemplateId)
            .HasColumnName("TableTemplateID")
            .HasColumnType("tinyint");

        builder.Property(x => x.Title)
            .HasColumnType("varchar(64)");

        builder.Property(x => x.DisplayOrder)
            .HasColumnType("smallint");

        builder.Property(x => x.ReferenceId)
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("CreatedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.LastModifiedByUserId)
            .HasColumnName("LastModifiedByUserID")
            .HasColumnType("smallint");

        builder.HasOne(builder => builder.CreatedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.CreatedByUserId);

        builder.HasOne(builder => builder.LastModifiedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.LastModifiedByUserId);

        builder.HasOne(builder => builder.TableTemplate)
            .WithMany()
            .HasForeignKey(builder => builder.TableTemplateId);

        builder.HasMany(x => x.Groups)
            .WithOne()
            .HasForeignKey(x => x.SpecSheetTableId);

        builder.HasMany(x => x.Widgets)
            .WithOne()
            .HasForeignKey(x => x.SpecSheetTableId);

        builder.HasMany(x => x.Notes)
            .WithOne()
            .HasForeignKey(x => x.SpecSheetTableId);

        builder.Property(x => x.ConfigurationOverride)
            .HasColumnName("ConfigurationOverride")
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<ITableProperties>(v, JsonSerializerSettings.DeserializerSettings));
    }
}
