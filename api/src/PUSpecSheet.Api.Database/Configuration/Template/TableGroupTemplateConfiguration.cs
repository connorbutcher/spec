using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Configuration.Template;

internal class TableGroupTemplateConfiguration : IEntityTypeConfiguration<DbTableGroupTemplate>
{
    public void Configure(EntityTypeBuilder<DbTableGroupTemplate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("TableGroupTemplates", "template");

        builder.Property(x => x.Id)
            .HasColumnName("TableGroupTemplateID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.ParentTableGroupTemplateId)
            .HasColumnName("ParentTableGroupTemplateID")
            .HasColumnType("int");

        builder.Property(x => x.TableTemplateId)
            .HasColumnName("TableTemplateID")
            .HasColumnType("tinyint");

        builder.Property(x => x.Title)
            .HasColumnType("varchar(128)");

        builder.Property(x => x.GroupKey)
            .HasColumnType("varchar(64)");

        builder.Property(x => x.GroupType)
            .HasColumnType("varchar(32)");

        builder.Property(x => x.DisplayOrder)
            .HasColumnType("tinyint");

        builder.HasMany(x => x.RowTemplates)
            .WithOne()
            .HasForeignKey(x => x.TableGroupTemplateId);

        builder.HasMany(x => x.TableGroupTemplateWidgets)
            .WithOne()
            .HasForeignKey(x => x.TableGroupTemplateId);
    }
}
