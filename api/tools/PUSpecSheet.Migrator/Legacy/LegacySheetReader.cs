using System.Data;
using Microsoft.Data.SqlClient;
using PUSpecSheet.Database.Conversions;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// Reads legacy sheets and their copies into <see cref="SheetSnapshot"/>s. All the SQL is in <see cref="LegacySql"/>;
/// this only runs it and puts the rows together. A sheet is read whole, every copy at once, as the migration
/// compares each copy with the one before.
/// </summary>
internal sealed class LegacySheetReader
{
    private readonly SqlConnection connection;

    /// <param name="connection">An open connection to the database holding the legacy tables.</param>
    public LegacySheetReader(SqlConnection connection)
    {
        this.connection = connection;
    }

    public async Task<IReadOnlyList<LegacySheet>> ListSheetsAsync(CancellationToken cancellationToken)
    {
        var sheets = new List<LegacySheet>();
        await using var command = new SqlCommand(LegacySql.Sheets, connection);
        await ReadAllAsync(command, null, reader => sheets.Add(new LegacySheet
        {
            Id = reader.GetInt32("Id"),
            PublicId = reader.GetGuid("PublicId"),
            PhaseId = reader.GetInt32("PhaseId"),
            SheetTypeId = reader.GetInt32("SheetTypeId"),
            CreatedAtUtc = reader.GetDateTime("CreatedAtUtc"),
        }), cancellationToken);

        return sheets;
    }

    /// <summary>Every copy of one legacy sheet, in no particular order.</summary>
    public async Task<IReadOnlyList<LegacyCopy>> LoadCopiesAsync(int sheetId, CancellationToken cancellationToken)
    {
        var versions = new List<LegacyCopy>();
        await using (var command = new SqlCommand(LegacySql.Versions, connection))
        {
            await ReadAllAsync(command, sheetId, reader => versions.Add(new LegacyCopy
            {
                Id = reader.GetInt32("Id"),
                VersionNumber = reader.GetInt32("VersionNumber"),
                PublishedAtUtc = NullableDateTime(reader, "PublishedAtUtc"),
                UserId = reader.GetInt32("UserId"),
                Note = NullableString(reader, "Note"),
            }), cancellationToken);
        }

        var tables = new List<(int VersionId, SnapshotTable Table)>();
        await using (var command = new SqlCommand(LegacySql.Tables, connection))
        {
            await ReadAllAsync(command, sheetId, reader => tables.Add((reader.GetInt32("SheetVersionId"), new SnapshotTable
            {
                Id = reader.GetInt32("Id"),
                PublicId = reader.GetGuid("PublicId"),
                TableTemplateVersionId = reader.GetInt32("TableTemplateVersionId"),
                Title = NullableString(reader, "Title"),
                DisplayOrder = reader.GetInt32("DisplayOrder"),
            })), cancellationToken);
        }

        var sections = new List<(int VersionId, SnapshotSection Section)>();
        await using (var command = new SqlCommand(LegacySql.Sections, connection))
        {
            await ReadAllAsync(command, sheetId, reader => sections.Add((reader.GetInt32("SheetVersionId"), new SnapshotSection
            {
                Id = reader.GetInt32("Id"),
                PublicId = reader.GetGuid("PublicId"),
                TableId = reader.GetInt32("SheetTableId"),
                ParentSectionId = NullableInt(reader, "ParentSheetSectionId"),
                TemplateSectionId = reader.GetInt32("TemplateSectionId"),
                DisplayOrder = reader.GetInt32("DisplayOrder"),
            })), cancellationToken);
        }

        var blocks = new List<(int VersionId, SnapshotColumnBlock Block)>();
        await using (var command = new SqlCommand(LegacySql.ColumnBlocks, connection))
        {
            await ReadAllAsync(command, sheetId, reader => blocks.Add((reader.GetInt32("SheetVersionId"), new SnapshotColumnBlock
            {
                Id = reader.GetInt32("Id"),
                PublicId = reader.GetGuid("PublicId"),
                TableId = reader.GetInt32("SheetTableId"),
                TemplateColumnBlockId = reader.GetInt32("TemplateColumnBlockId"),
                DisplayOrder = reader.GetInt32("DisplayOrder"),
            })), cancellationToken);
        }

        var rows = new List<(int VersionId, SnapshotRow Row)>();
        await using (var command = new SqlCommand(LegacySql.Rows, connection))
        {
            await ReadAllAsync(command, sheetId, reader => rows.Add((reader.GetInt32("SheetVersionId"), new SnapshotRow
            {
                Id = reader.GetInt32("Id"),
                PublicId = reader.GetGuid("PublicId"),
                SectionId = reader.GetInt32("SheetSectionId"),
                TemplateRowId = reader.GetInt32("TemplateRowId"),
                DisplayOrder = reader.GetInt32("DisplayOrder"),
            })), cancellationToken);
        }

        var contents = await ReadContentsAsync(sheetId, cancellationToken);

        var cells = new List<(int VersionId, SnapshotCell Cell)>();
        await using (var command = new SqlCommand(LegacySql.Cells, connection))
        {
            await ReadAllAsync(command, sheetId, reader =>
            {
                var id = reader.GetInt32("Id");
                cells.Add((reader.GetInt32("SheetVersionId"), new SnapshotCell
                {
                    Id = id,
                    PublicId = reader.GetGuid("PublicId"),
                    RowId = reader.GetInt32("SheetRowId"),
                    TemplateCellId = reader.GetInt32("TemplateCellId"),
                    ColumnBlockId = NullableInt(reader, "SheetColumnBlockId"),
                    Content = contents.TryGetValue(id, out var content) ? content.Build() : CellContent.Empty,
                }));
            }, cancellationToken);
        }

        return versions
            .Select(version => version with
            {
                Snapshot = new SheetSnapshot
                {
                    Tables = tables.Where(item => item.VersionId == version.Id).Select(item => item.Table).ToList(),
                    Sections = sections.Where(item => item.VersionId == version.Id).Select(item => item.Section).ToList(),
                    ColumnBlocks = blocks.Where(item => item.VersionId == version.Id).Select(item => item.Block).ToList(),
                    Rows = rows.Where(item => item.VersionId == version.Id).Select(item => item.Row).ToList(),
                    Cells = cells.Where(item => item.VersionId == version.Id).Select(item => item.Cell).ToList(),
                },
            })
            .ToList();
    }

    private async Task<Dictionary<int, CellContentParts>> ReadContentsAsync(int sheetId, CancellationToken cancellationToken)
    {
        var parts = new Dictionary<int, CellContentParts>();

        CellContentParts For(SqlDataReader reader)
        {
            var cellId = reader.GetInt32("SheetCellId");
            if (!parts.TryGetValue(cellId, out var cell))
            {
                cell = new CellContentParts();
                parts[cellId] = cell;
            }

            return cell;
        }

        await using (var command = new SqlCommand(LegacySql.TextValues, connection))
        {
            await ReadAllAsync(command, sheetId, reader => For(reader).Text = reader.GetString("Value"), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.NumericValues, connection))
        {
            await ReadAllAsync(command, sheetId, reader => For(reader).Number = reader.GetDecimal("Value"), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.DateValues, connection))
        {
            await ReadAllAsync(command, sheetId, reader => For(reader).Date = DateOnly.FromDateTime(reader.GetDateTime("Value")), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.BooleanValues, connection))
        {
            await ReadAllAsync(command, sheetId, reader => For(reader).Boolean = reader.GetBoolean("Value"), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.OptionValues, connection))
        {
            await ReadAllAsync(command, sheetId, reader => For(reader).OptionId = reader.GetInt32("CellTypeOptionId"), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.LabelledTextValues, connection))
        {
            await ReadAllAsync(command, sheetId, reader => For(reader).LabelledText.Add(new LabelledSlot(
                reader.GetString("SlotKey"),
                reader.GetInt32("Position"),
                reader.GetString("Value"))), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.CellSettings, connection))
        {
            await ReadAllAsync(
                command,
                sheetId,
                reader => For(reader).Settings = CellJson.Deserialize<CellInstanceSettings>(reader.GetString("Settings")),
                cancellationToken);
        }

        return parts;
    }

    private static async Task ReadAllAsync(SqlCommand command, int? sheetId, Action<SqlDataReader> read, CancellationToken cancellationToken)
    {
        if (sheetId is { } id)
        {
            command.Parameters.Add(new SqlParameter("@sheetId", SqlDbType.Int) { Value = id });
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            read(reader);
        }
    }

    private static int? NullableInt(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static string? NullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static DateTime? NullableDateTime(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
