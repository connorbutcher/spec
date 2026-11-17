using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Api.Database.Schema.Types;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.Types;

public class SpecSheetTypePropertyConfiguration : IEntityTypeConfiguration<DbSpecSheetTypeProperty>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTypeProperty> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTypeProperties", "types");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTypePropertyID")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTypeId)
            .HasColumnType("tinyint")
            .HasColumnName("SpecSheetTypeID");

        builder.Property(x => x.ReferenceId)
            .HasColumnType("uniqueidentifier")
            .HasColumnName("ReferenceID");

        builder.Property(x => x.ValueName)
            .HasColumnType("varchar(256)");

        builder.Property(x => x.ValueKey)
            .HasColumnType("varchar(256)");

        builder.Property(x => x.ValueType)
            .HasColumnType("varchar(64)");

        builder.Property(x => x.ValueControlType)
            .HasColumnType("varchar(64)");

        builder.Property(x => x.PlaceholderText)
            .HasColumnType("varchar(512)");

        builder.Property(x => x.DisplayOrder)
            .HasColumnType("smallint");

        builder.Property(x => x.ValidatorConfiguration)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<ICollection<SpecSheetTypePropertyValidator>>(v, JsonSerializerSettings.DeserializerSettings));
    }
}
