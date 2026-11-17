using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Configuration;
using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Abstractions.Domain.Styling;
using PUSpecSheet.Api.Database.Schema.Configuration;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.Configuration;

internal record CellConfigurationConfiguration : IEntityTypeConfiguration<DbCellConfiguration>
{
    public void Configure(EntityTypeBuilder<DbCellConfiguration> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("CellConfigurations", "configuration");

        builder.Property(x => x.Id)
            .HasColumnName("CellConfigurationID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.Styling)
            .HasColumnName("Styling")
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<CellStyling>(v, JsonSerializerSettings.DeserializerSettings)!);

        builder.Property(x => x.Properties)
            .HasColumnName("Properties")
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<ICellProperties>(v, JsonSerializerSettings.DeserializerSettings)!);

        builder.Property(x => x.Validators)
            .HasColumnName("Validators")
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<ICollection<Validator>>(v, JsonSerializerSettings.DeserializerSettings));
    }
}
