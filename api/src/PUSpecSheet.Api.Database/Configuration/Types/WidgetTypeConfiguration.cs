using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Configuration.Types;

internal class WidgetTypeConfiguration : IEntityTypeConfiguration<DbWidgetType>
{
    public void Configure(EntityTypeBuilder<DbWidgetType> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("WidgetTypes", "types");

        builder.Property(x => x.Id)
            .HasColumnName("WidgetTypeID")
            .HasColumnType("tinyint")
            .UseIdentityColumn();

        builder.Property(x => x.Key)
            .HasColumnName("WidgetTypeKey")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.Title)
            .HasColumnType("varchar(64)");

        builder.Property(x => x.Description)
            .HasColumnType("varchar(256)");
    }
}
