using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.DataAccess.Sheets.Queries;

// The SQL that copies every value and setting of one row revision onto another inside the database, one INSERT …
// SELECT per value table sent as one batch, so a new draft starts without the values coming back to the API and going
// out again. Table and column names come from the EF model, so a renamed column or a new value column is picked up; a
// computed column is left for the database to fill.
public static class RowValueCopySql
{
    private static readonly Type[] _valueTypes =
    [
        typeof(DbTextValue),
        typeof(DbNumericValue),
        typeof(DbDateValue),
        typeof(DbBooleanValue),
        typeof(DbOptionValue),
        typeof(DbCellSettingsValue),
        typeof(DbLabelledTextValue),
        typeof(DbLinkedSheetValue),
    ];

    public static string Build(IModel model)
    {
        var sql = new StringBuilder();
        foreach (var type in _valueTypes)
        {
            var entity = model.FindEntityType(type)
                ?? throw new InvalidOperationException($"{type.Name} is not in the model.");
            var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            var revision = Column(entity, nameof(IDbCellValue.SheetRowRevisionId), table);
            var copied = entity.GetProperties()
                .Where(property => property.Name != nameof(IDbCellValue.SheetRowRevisionId)
                    && property.GetComputedColumnSql(table) is null)
                .Select(property => Quote(property.GetColumnName(table)!))
                .ToList();
            var columns = string.Join(", ", copied);
            var name = entity.GetSchema() is { } schema ? $"{Quote(schema)}.{Quote(table.Name)}" : Quote(table.Name);

            sql.Append("INSERT INTO ").Append(name).Append(" (").Append(revision).Append(", ").Append(columns).Append(") ")
                .Append("SELECT {1}, ").Append(columns).Append(" FROM ").Append(name)
                .Append(" WHERE ").Append(revision).Append(" = {0};\n");
        }

        return sql.ToString();
    }

    private static string Column(IEntityType entity, string propertyName, StoreObjectIdentifier table)
    {
        return Quote(entity.FindProperty(propertyName)!.GetColumnName(table)!);
    }

    private static string Quote(string identifier)
    {
        return "[" + identifier.Replace("]", "]]", StringComparison.Ordinal) + "]";
    }
}
