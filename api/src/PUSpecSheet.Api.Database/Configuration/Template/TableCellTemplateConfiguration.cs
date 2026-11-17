using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Configuration.Template;

internal class TableCellTemplateConfiguration : IEntityTypeConfiguration<DbTableCellTemplate>
{
    public void Configure(EntityTypeBuilder<DbTableCellTemplate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("TableCellTemplates", "template");

        builder.Property(x => x.Id)
            .HasColumnName("TableCellTemplateID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.TableRowTemplateId)
            .HasColumnName("TableRowTemplateID")
            .HasColumnType("int");

        builder.Property(x => x.TableCellTypeId)
            .HasColumnName("TableCellTypeID")
            .HasColumnType("tinyint");

        builder.Property(x => x.CellConfigurationId)
            .HasColumnName("CellConfigurationID")
            .HasColumnType("int");

        builder.Property(x => x.ColumnNumber)
            .HasColumnType("tinyint");

        builder.Property(x => x.ColSpan)
            .HasColumnType("tinyint");

        builder.Property(x => x.RowSpan)
            .HasColumnType("tinyint");

        builder.Property(x => x.GroupKey)
            .HasColumnType("varchar(64)");
    }
}
