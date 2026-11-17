using System.Data;
using Microsoft.Data.SqlClient;
using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Database.Conversions;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// Reads legacy sheets and their copies into <see cref="SheetSnapshot"/>s. All the SQL is in <see cref="LegacySql"/>;
/// this runs it and puts the rows together, filling in from <see cref="DesignLookup"/> what the legacy layout
/// doesn't hold: each table's template version and every item's display order. Legacy items have no public ids,
/// so the snapshots have none and the migrated items get new ones. A sheet is read whole, every copy at once.
/// </summary>
internal sealed class LegacySheetReader
{
    /// <summary>The gap left between display orders, as the app leaves them.</summary>
    private const int OrderGap = 1024;

    private readonly SqlConnection connection;
    private readonly DesignLookup design;

    /// <param name="connection">An open connection to the database holding the legacy tables.</param>
    /// <param name="design">The target database's design tables.</param>
    public LegacySheetReader(SqlConnection connection, DesignLookup design)
    {
        this.connection = connection;
        this.design = design;
    }

    public async Task<IReadOnlyList<LegacySheet>> ListSheetsAsync(CancellationToken cancellationToken)
    {
        var sheets = new List<LegacySheet>();
        await using var command = new SqlCommand(LegacySql.Sheets, connection);
        await ReadAllAsync(command, null, reader => sheets.Add(new LegacySheet
        {
            PhaseId = reader.GetInt32("PhaseId"),
            SheetTypeId = reader.GetInt32("SheetTypeId"),
            PublicId = reader.GetGuid("PublicId"),
            CreatedAtUtc = reader.GetDateTime("CreatedAtUtc"),
        }), cancellationToken);

        return sheets;
    }

    /// <summary>Every copy of one legacy sheet, in no particular order.</summary>
    /// <exception cref="MigrationException">A table's template has no versions in the target database.</exception>
    public async Task<IReadOnlyList<LegacyCopy>> LoadCopiesAsync(LegacySheet sheet, CancellationToken cancellationToken)
    {
        var versions = new List<LegacyCopy>();
        await using (var command = new SqlCommand(LegacySql.Versions, connection))
        {
            await ReadAllAsync(command, sheet, reader => versions.Add(new LegacyCopy
            {
                Id = reader.GetInt32("Id"),
                VersionNumber = reader.GetInt32("VersionNumber"),
                PublishedAtUtc = NullableDateTime(reader, "PublishedAtUtc"),
                UserId = reader.GetInt32("UserId"),
            }), cancellationToken);
        }

        var tables = new List<(int VersionId, int TemplateId, SnapshotTable Table)>();
        await using (var command = new SqlCommand(LegacySql.Tables, connection))
        {
            await ReadAllAsync(command, sheet, reader =>
            {
                var templateId = reader.GetInt32("TableTemplateId");
                tables.Add((reader.GetInt32("SpecSheetId"), templateId, new SnapshotTable
                {
                    Id = reader.GetInt32("Id"),
                    TableTemplateVersionId = design.LatestVersionOf(templateId),
                    Title = NullableString(reader, "Title"),
                }));
            }, cancellationToken);
        }

        var sections = new List<(int VersionId, SnapshotSection Section)>();
        await using (var command = new SqlCommand(LegacySql.Groups, connection))
        {
            await ReadAllAsync(command, sheet, reader => sections.Add((reader.GetInt32("SpecSheetId"), new SnapshotSection
            {
                Id = reader.GetInt32("Id"),
                TableId = reader.GetInt32("TableId"),
                ParentSectionId = NullableInt(reader, "ParentGroupId"),
                TemplateSectionId = reader.GetInt32("GroupTemplateId"),
            })), cancellationToken);
        }

        var rows = new List<(int VersionId, SnapshotRow Row)>();
        await using (var command = new SqlCommand(LegacySql.Rows, connection))
        {
            await ReadAllAsync(command, sheet, reader => rows.Add((reader.GetInt32("SpecSheetId"), new SnapshotRow
            {
                Id = reader.GetInt32("Id"),
                SectionId = reader.GetInt32("GroupId"),
                TemplateRowId = reader.GetInt32("RowTemplateId"),
            })), cancellationToken);
        }

        var contents = await ReadContentsAsync(sheet, cancellationToken);

        var cells = new List<(int VersionId, SnapshotCell Cell)>();
        await using (var command = new SqlCommand(LegacySql.Cells, connection))
        {
            await ReadAllAsync(command, sheet, reader =>
            {
                var id = reader.GetInt32("Id");
                cells.Add((reader.GetInt32("SpecSheetId"), new SnapshotCell
                {
                    Id = id,
                    RowId = reader.GetInt32("TableRowId"),
                    TemplateCellId = reader.GetInt32("TemplateCellId"),
                    Content = contents.TryGetValue(id, out var content) ? content.Build() : CellContent.Empty,
                }));
            }, cancellationToken);
        }

        return versions
            .Select(version => version with
            {
                Snapshot = new SheetSnapshot
                {
                    Tables = Ordered(
                        tables.Where(item => item.VersionId == version.Id).ToList(),
                        _ => 0,
                        item => design.TableOrder(item.TemplateId),
                        item => item.Table.Id,
                        (item, order) => item.Table with { DisplayOrder = order }),
                    Sections = Ordered(
                        sections.Where(item => item.VersionId == version.Id).Select(item => item.Section).ToList(),
                        section => (section.TableId, section.ParentSectionId),
                        section => design.SectionOrder(section.TemplateSectionId),
                        section => section.Id,
                        (section, order) => section with { DisplayOrder = order }),
                    Rows = Ordered(
                        rows.Where(item => item.VersionId == version.Id).Select(item => item.Row).ToList(),
                        row => row.SectionId,
                        row => design.RowOrder(row.TemplateRowId),
                        row => row.Id,
                        (row, order) => row with { DisplayOrder = order }),
                    Cells = cells.Where(item => item.VersionId == version.Id).Select(item => item.Cell).ToList(),
                },
            })
            .ToList();
    }

    /// <summary>
    /// Gives items the display order legacy items lack: among the items of one parent, by their template's order
    /// and then by id (the order they were added in), spaced <see cref="OrderGap"/> apart.
    /// </summary>
    private static List<TResult> Ordered<TItem, TParent, TResult>(
        List<TItem> items,
        Func<TItem, TParent> parent,
        Func<TItem, int> templateOrder,
        Func<TItem, int> id,
        Func<TItem, int, TResult> withOrder)
    {
        return items
            .GroupBy(parent)
            .SelectMany(siblings => siblings
                .OrderBy(templateOrder)
                .ThenBy(id)
                .Select((item, index) => withOrder(item, (index + 1) * OrderGap)))
            .ToList();
    }

    private async Task<Dictionary<int, CellContentParts>> ReadContentsAsync(LegacySheet sheet, CancellationToken cancellationToken)
    {
        var parts = new Dictionary<int, CellContentParts>();

        CellContentParts For(SqlDataReader reader)
        {
            var cellId = reader.GetInt32("CellId");
            if (!parts.TryGetValue(cellId, out var cell))
            {
                cell = new CellContentParts();
                parts[cellId] = cell;
            }

            return cell;
        }

        await using (var command = new SqlCommand(LegacySql.TextValues, connection))
        {
            await ReadAllAsync(command, sheet, reader => For(reader).Text = reader.GetString("Value"), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.NumericValues, connection))
        {
            await ReadAllAsync(command, sheet, reader => For(reader).Number = reader.GetDecimal("Value"), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.DateValues, connection))
        {
            await ReadAllAsync(command, sheet, reader => For(reader).Date = DateOnly.FromDateTime(reader.GetDateTime("Value")), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.BooleanValues, connection))
        {
            await ReadAllAsync(command, sheet, reader => For(reader).Boolean = reader.GetBoolean("Value"), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.OptionValues, connection))
        {
            await ReadAllAsync(command, sheet, reader => For(reader).OptionId = reader.GetInt32("CellTypeOptionId"), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.LabelledTextValues, connection))
        {
            await ReadAllAsync(command, sheet, reader => For(reader).LabelledText.Add(new LabelledSlot(
                reader.GetString("SlotKey"),
                reader.GetInt32("Position"),
                reader.GetString("Value"))), cancellationToken);
        }

        await using (var command = new SqlCommand(LegacySql.CellSettings, connection))
        {
            await ReadAllAsync(
                command,
                sheet,
                reader => For(reader).Settings = CellJson.Deserialize<CellInstanceSettings>(reader.GetString("Settings")),
                cancellationToken);
        }

        return parts;
    }

    private static async Task ReadAllAsync(SqlCommand command, LegacySheet? sheet, Action<SqlDataReader> read, CancellationToken cancellationToken)
    {
        if (sheet is not null)
        {
            command.Parameters.Add(new SqlParameter("@phaseId", SqlDbType.Int) { Value = sheet.PhaseId });
            command.Parameters.Add(new SqlParameter("@sheetTypeId", SqlDbType.Int) { Value = sheet.SheetTypeId });
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
