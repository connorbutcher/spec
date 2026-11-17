using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Configurations.Templates;

public sealed class WidgetTypeConfiguration : IEntityTypeConfiguration<DbWidgetType>
{
    public void Configure(EntityTypeBuilder<DbWidgetType> builder)
    {
        builder.ToTable("WidgetTypes", DatabaseSchemas.Design);

        builder.HasKey(widgetType => widgetType.Id);

        builder.HasIndex(widgetType => widgetType.Key)
            .IsUnique();

        builder.Property(widgetType => widgetType.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(widgetType => widgetType.Description)
            .IsRequired()
            .HasMaxLength(256);
    }
}
