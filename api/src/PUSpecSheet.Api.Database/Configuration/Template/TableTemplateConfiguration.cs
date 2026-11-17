using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Api.Database.Schema.Template;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.Template;

internal class TableTemplateConfiguration : IEntityTypeConfiguration<DbTableTemplate>
{
    public void Configure(EntityTypeBuilder<DbTableTemplate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("TableTemplates", "template");

        builder.Property(x => x.Id)
            .HasColumnName("TableTemplateID")
            .HasColumnType("tinyint")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTypeId)
            .HasColumnName("SpecSheetTypeID")
            .HasColumnType("tinyint");

        builder.Property(x => x.Type)
            .HasColumnType("varchar(64)");

        builder.Property(x => x.Description)
            .HasColumnType("varchar(2048)");

        builder.HasMany(x => x.TableGroupTemplates)
            .WithOne()
            .HasForeignKey(x => x.TableTemplateId);

        builder.HasMany(x => x.TableTemplateWidgets)
            .WithOne()
            .HasForeignKey(x => x.TableTemplateId);

        builder.Property(x => x.DefaultConfiguration)
            .HasColumnName("DefaultConfiguration")
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<ITableProperties>(v, JsonSerializerSettings.DeserializerSettings));
    }
}
