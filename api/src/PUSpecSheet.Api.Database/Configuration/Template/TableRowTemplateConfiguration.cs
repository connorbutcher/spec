using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Configuration;
using PUSpecSheet.Api.Database.Schema.Template;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.Template;

internal class TableRowTemplateConfiguration : IEntityTypeConfiguration<DbTableRowTemplate>
{
    public void Configure(EntityTypeBuilder<DbTableRowTemplate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("TableRowTemplates", "template");

        builder.Property(x => x.Id)
            .HasColumnName("TableRowTemplateID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.RowNumber)
            .HasColumnType("tinyint");

        builder.HasMany(x => x.CellTemplates)
            .WithOne()
            .HasForeignKey(x => x.TableRowTemplateId);

        builder.Property(x => x.Validators)
        .HasColumnName("RowValidators")
        .HasColumnType("nvarchar(max)")
        .HasConversion(
            v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
            v => JsonSerializer.Deserialize<ICollection<Validator>>(v, JsonSerializerSettings.DeserializerSettings));
    }
}
