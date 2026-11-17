using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Database;

namespace PUSpecSheet.Migrator.Legacy;

// Before any sheet: fills the setup tables the new layout added for legacy data (widget types, template widgets and
// sheet type properties) with their legacy ids, checks every legacy user is in the target, and lists what each user
// could do in the legacy app, as roles in the new one are given by hand. Rows already there are left alone, so it can
// run again. It keeps what it adds even on a dry run: the sheets' checks need it, and it is only reference data.
internal sealed class ReferenceDataMigrator
{
    private readonly SqlConnection _legacy;
    private readonly PuSpecSheetDbContext _target;
    private readonly DesignLookup _design;
    private readonly ILogger _logger;

    public ReferenceDataMigrator(SqlConnection legacy, PuSpecSheetDbContext target, DesignLookup design, ILogger logger)
    {
        _legacy = legacy;
        _target = target;
        _design = design;
        _logger = logger;
    }

    // False when something is missing that the sheets need, which has been logged.
    public async Task<bool> MigrateAsync(CancellationToken cancellationToken)
    {
        var usersOk = await CheckUsersAsync(cancellationToken);
        await ReportAccessAsync(cancellationToken);
        if (!usersOk)
        {
            return false;
        }

        await using var transaction = await _target.Database.BeginTransactionAsync(cancellationToken);
        var added = await AddWidgetTypesAsync(cancellationToken)
            + await AddTableTemplateWidgetsAsync(cancellationToken)
            + await AddSectionWidgetsAsync(cancellationToken)
            + await AddSheetTypePropertiesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _logger.ReferenceDataAdded(added);
        return true;
    }

    private async Task<bool> CheckUsersAsync(CancellationToken cancellationToken)
    {
        var targetIds = (await _target.Users.AsNoTracking().Select(user => user.Id).ToListAsync(cancellationToken)).ToHashSet();
        var missing = new List<string>();
        await ReadAllAsync(LegacySql.UserIds, reader =>
        {
            var id = reader.Int("Id");
            if (!targetIds.Contains(id))
            {
                missing.Add($"{id} ({reader.GetString("DisplayName")})");
            }
        }, cancellationToken);

        foreach (var user in missing)
        {
            _logger.UserMissing(user);
        }

        return missing.Count == 0;
    }

    private async Task ReportAccessAsync(CancellationToken cancellationToken)
    {
        await ReadAllAsync(LegacySql.UserAccess, reader => _logger.UserAccess(reader.Int("UserId"), reader.GetString("Access")), cancellationToken);
        await ReadAllAsync(LegacySql.SheetTypeAccess, reader => _logger.SheetTypeAccess(reader.Int("SheetTypeId"), reader.GetString("Permission")), cancellationToken);
    }

    private async Task<int> AddWidgetTypesAsync(CancellationToken cancellationToken)
    {
        var existing = (await _target.WidgetTypes.AsNoTracking().Select(type => type.Id).ToListAsync(cancellationToken)).ToHashSet();
        var rows = new List<(int Id, Guid Key, string Name, string Description)>();
        await ReadAllAsync(LegacySql.WidgetTypes, reader => rows.Add((reader.Int("Id"), reader.GetGuid("Key"), reader.GetString("Name"), reader.GetString("Description"))), cancellationToken);

        var added = 0;
        foreach (var row in rows.Where(row => !existing.Contains(row.Id)))
        {
            await _target.Database.ExecuteSqlAsync(
                $"""
                SET IDENTITY_INSERT [design].[WidgetTypes] ON;
                INSERT INTO [design].[WidgetTypes] ([Id], [Key], [Name], [Description]) VALUES ({row.Id}, {row.Key}, {row.Name}, {row.Description});
                SET IDENTITY_INSERT [design].[WidgetTypes] OFF;
                """,
                cancellationToken);
            added++;
        }

        return added;
    }

    private async Task<int> AddTableTemplateWidgetsAsync(CancellationToken cancellationToken)
    {
        var existing = (await _target.TableTemplateWidgets.AsNoTracking().Select(widget => widget.Id).ToListAsync(cancellationToken)).ToHashSet();
        var rows = new List<(int Id, int TableTemplateId, int WidgetTypeId)>();
        await ReadAllAsync(LegacySql.TableTemplateWidgets, reader => rows.Add((reader.Int("Id"), reader.Int("TableTemplateId"), reader.Int("WidgetTypeId"))), cancellationToken);

        var added = 0;
        foreach (var row in rows.Where(row => !existing.Contains(row.Id)))
        {
            var versionId = _design.VersionOfTemplate(row.TableTemplateId);
            await _target.Database.ExecuteSqlAsync(
                $"""
                SET IDENTITY_INSERT [design].[TableTemplateWidgets] ON;
                INSERT INTO [design].[TableTemplateWidgets] ([Id], [TableTemplateVersionId], [WidgetTypeId]) VALUES ({row.Id}, {versionId}, {row.WidgetTypeId});
                SET IDENTITY_INSERT [design].[TableTemplateWidgets] OFF;
                """,
                cancellationToken);
            added++;
        }

        return added;
    }

    private async Task<int> AddSectionWidgetsAsync(CancellationToken cancellationToken)
    {
        var existing = (await _target.TemplateSectionWidgets.AsNoTracking().Select(widget => widget.Id).ToListAsync(cancellationToken)).ToHashSet();
        var rows = new List<(int Id, int GroupTemplateId, int WidgetTypeId)>();
        await ReadAllAsync(LegacySql.GroupTemplateWidgets, reader => rows.Add((reader.Int("Id"), reader.Int("GroupTemplateId"), reader.Int("WidgetTypeId"))), cancellationToken);

        var added = 0;
        foreach (var row in rows.Where(row => !existing.Contains(row.Id)))
        {
            if (!_design.HasSection(row.GroupTemplateId))
            {
                throw new MigrationException($"Group template widget {row.Id} is on group template {row.GroupTemplateId}, which isn't a template section in the target database.");
            }

            await _target.Database.ExecuteSqlAsync(
                $"""
                SET IDENTITY_INSERT [design].[TemplateSectionWidgets] ON;
                INSERT INTO [design].[TemplateSectionWidgets] ([Id], [TemplateSectionId], [WidgetTypeId]) VALUES ({row.Id}, {row.GroupTemplateId}, {row.WidgetTypeId});
                SET IDENTITY_INSERT [design].[TemplateSectionWidgets] OFF;
                """,
                cancellationToken);
            added++;
        }

        return added;
    }

    private async Task<int> AddSheetTypePropertiesAsync(CancellationToken cancellationToken)
    {
        var existing = (await _target.SheetTypeProperties.AsNoTracking().Select(property => property.Id).ToListAsync(cancellationToken)).ToHashSet();
        var rows = new List<LegacyProperty>();
        await ReadAllAsync(LegacySql.SheetTypeProperties, reader => rows.Add(new LegacyProperty
        {
            Id = reader.Int("Id"),
            SheetTypeId = reader.Int("SheetTypeId"),
            PublicId = reader.GetGuid("PublicId"),
            Key = reader.GetString("Key"),
            Name = reader.GetString("Name"),
            ValueType = reader.GetString("ValueType"),
            ControlType = reader.GetString("ControlType"),
            CanBeSetInForm = reader.Bool("CanBeSetInForm"),
            SaveValueAsProperty = reader.Bool("SaveValueAsProperty"),
            DisplayOrder = reader.Int("DisplayOrder"),
            Placeholder = reader.NullableString("Placeholder"),
            ValidatorConfiguration = reader.NullableString("ValidatorConfiguration"),
        }), cancellationToken);

        var added = 0;
        foreach (var row in rows.Where(row => !existing.Contains(row.Id)))
        {
            await _target.Database.ExecuteSqlAsync(
                $"""
                SET IDENTITY_INSERT [setup].[SheetTypeProperties] ON;
                INSERT INTO [setup].[SheetTypeProperties]
                    ([Id], [PublicId], [SheetTypeId], [Key], [Name], [ValueType], [ControlType], [CanBeSetInForm], [SaveValueAsProperty], [DisplayOrder], [Placeholder], [ValidatorConfiguration])
                VALUES
                    ({row.Id}, {row.PublicId}, {row.SheetTypeId}, {row.Key}, {row.Name}, {row.ValueType}, {row.ControlType}, {row.CanBeSetInForm}, {row.SaveValueAsProperty}, {row.DisplayOrder}, {row.Placeholder}, {row.ValidatorConfiguration});
                SET IDENTITY_INSERT [setup].[SheetTypeProperties] OFF;
                """,
                cancellationToken);
            added++;
        }

        return added;
    }

    private async Task ReadAllAsync(string sql, Action<SqlDataReader> read, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(sql, _legacy);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            read(reader);
        }
    }

    private sealed record LegacyProperty
    {
        public required int Id { get; init; }

        public required int SheetTypeId { get; init; }

        public required Guid PublicId { get; init; }

        public required string Key { get; init; }

        public required string Name { get; init; }

        public required string ValueType { get; init; }

        public required string ControlType { get; init; }

        public required bool CanBeSetInForm { get; init; }

        public required bool SaveValueAsProperty { get; init; }

        public required int DisplayOrder { get; init; }

        public string? Placeholder { get; init; }

        public string? ValidatorConfiguration { get; init; }
    }
}
