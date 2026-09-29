using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <summary>
    /// Versions table templates and gives sections a role with instance counts.
    /// Every existing template becomes version 1 (keeping its orientation); its sections and the sheet
    /// tables built from it move onto that version. Section inclusion maps onto fixed roles:
    /// Required = 1..1 starting at 1, Default = 0..1 starting at 1, Optional = 0..1 starting at 0.
    /// </summary>
    public partial class AddSectionRolesAndTemplateVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableTemplateVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableTemplateId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Orientation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableTemplateVersions", x => x.Id);
                    table.CheckConstraint("CK_TableTemplateVersions_VersionNumber", "[VersionNumber] >= 1");
                    table.ForeignKey(
                        name: "FK_TableTemplateVersions_TableTemplates_TableTemplateId",
                        column: x => x.TableTemplateId,
                        principalTable: "TableTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableTemplateVersions_TableTemplateId_VersionNumber",
                table: "TableTemplateVersions",
                columns: new[] { "TableTemplateId", "VersionNumber" },
                unique: true);

            // Version 1 of every existing template, keeping its orientation.
            migrationBuilder.Sql(
                "INSERT INTO [TableTemplateVersions] ([TableTemplateId], [VersionNumber], [Orientation]) "
                + "SELECT [Id], 1, [Orientation] FROM [TableTemplates];");

            migrationBuilder.DropForeignKey(
                name: "FK_SheetTables_TableTemplates_TableTemplateId",
                table: "SheetTables");

            migrationBuilder.DropForeignKey(
                name: "FK_TemplateSections_TableTemplates_TableTemplateId",
                table: "TemplateSections");

            migrationBuilder.RenameColumn(
                name: "TableTemplateId",
                table: "TemplateSections",
                newName: "TableTemplateVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateSections_TableTemplateId",
                table: "TemplateSections",
                newName: "IX_TemplateSections_TableTemplateVersionId");

            migrationBuilder.RenameColumn(
                name: "TableTemplateId",
                table: "SheetTables",
                newName: "TableTemplateVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_SheetTables_TableTemplateId",
                table: "SheetTables",
                newName: "IX_SheetTables_TableTemplateVersionId");

            // The renamed columns still hold template ids; point them at each template's version 1.
            migrationBuilder.Sql(
                "UPDATE s SET s.[TableTemplateVersionId] = v.[Id] FROM [TemplateSections] s "
                + "JOIN [TableTemplateVersions] v ON v.[TableTemplateId] = s.[TableTemplateVersionId];");

            migrationBuilder.Sql(
                "UPDATE t SET t.[TableTemplateVersionId] = v.[Id] FROM [SheetTables] t "
                + "JOIN [TableTemplateVersions] v ON v.[TableTemplateId] = t.[TableTemplateVersionId];");

            migrationBuilder.DropColumn(
                name: "Orientation",
                table: "TableTemplates");

            migrationBuilder.AddColumn<int>(
                name: "MinInstances",
                table: "TemplateSections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxInstances",
                table: "TemplateSections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InitialInstances",
                table: "TemplateSections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE [TemplateSections] SET "
                + "[MinInstances] = CASE [Inclusion] WHEN 'Required' THEN 1 ELSE 0 END, "
                + "[MaxInstances] = 1, "
                + "[InitialInstances] = CASE [Inclusion] WHEN 'Optional' THEN 0 ELSE 1 END, "
                + "[Inclusion] = 'Fixed';");

            // AddSectionInclusion gave the column a 'Default' default; a role has no sensible default.
            migrationBuilder.Sql(
                "DECLARE @constraint nvarchar(max); "
                + "SELECT @constraint = QUOTENAME(d.[name]) FROM [sys].[default_constraints] d "
                + "JOIN [sys].[columns] c ON d.[parent_column_id] = c.[column_id] AND d.[parent_object_id] = c.[object_id] "
                + "WHERE d.[parent_object_id] = OBJECT_ID(N'[TemplateSections]') AND c.[name] = N'Inclusion'; "
                + "IF @constraint IS NOT NULL EXEC(N'ALTER TABLE [TemplateSections] DROP CONSTRAINT ' + @constraint + ';');");

            migrationBuilder.RenameColumn(
                name: "Inclusion",
                table: "TemplateSections",
                newName: "Role");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateSections_Instances",
                table: "TemplateSections",
                sql: "[MinInstances] >= 0 AND [InitialInstances] >= [MinInstances] AND ([MaxInstances] IS NULL OR ([MaxInstances] >= 1 AND [MaxInstances] >= [InitialInstances])) AND ([Role] <> 'Fixed' OR [MaxInstances] = 1)");

            migrationBuilder.AddForeignKey(
                name: "FK_SheetTables_TableTemplateVersions_TableTemplateVersionId",
                table: "SheetTables",
                column: "TableTemplateVersionId",
                principalTable: "TableTemplateVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateSections_TableTemplateVersions_TableTemplateVersionId",
                table: "TemplateSections",
                column: "TableTemplateVersionId",
                principalTable: "TableTemplateVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Only version 1 of each template survives going back: later versions' sections are deleted,
        /// and sheet tables are pointed back at their template.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SheetTables_TableTemplateVersions_TableTemplateVersionId",
                table: "SheetTables");

            migrationBuilder.DropForeignKey(
                name: "FK_TemplateSections_TableTemplateVersions_TableTemplateVersionId",
                table: "TemplateSections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateSections_Instances",
                table: "TemplateSections");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "TemplateSections",
                newName: "Inclusion");

            migrationBuilder.Sql(
                "UPDATE [TemplateSections] SET [Inclusion] = CASE "
                + "WHEN [MinInstances] >= 1 THEN 'Required' "
                + "WHEN [InitialInstances] >= 1 THEN 'Default' ELSE 'Optional' END;");

            migrationBuilder.DropColumn(
                name: "InitialInstances",
                table: "TemplateSections");

            migrationBuilder.DropColumn(
                name: "MaxInstances",
                table: "TemplateSections");

            migrationBuilder.DropColumn(
                name: "MinInstances",
                table: "TemplateSections");

            migrationBuilder.AddColumn<string>(
                name: "Orientation",
                table: "TableTemplates",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Horizontal");

            migrationBuilder.Sql(
                "UPDATE t SET t.[Orientation] = v.[Orientation] FROM [TableTemplates] t "
                + "JOIN [TableTemplateVersions] v ON v.[TableTemplateId] = t.[Id] AND v.[VersionNumber] = 1;");

            // Later versions' sections go leaf-first, since a parent can't be deleted before its children.
            migrationBuilder.Sql(
                "WHILE EXISTS (SELECT 1 FROM [TemplateSections] s JOIN [TableTemplateVersions] v "
                + "ON v.[Id] = s.[TableTemplateVersionId] WHERE v.[VersionNumber] > 1) "
                + "DELETE s FROM [TemplateSections] s JOIN [TableTemplateVersions] v "
                + "ON v.[Id] = s.[TableTemplateVersionId] WHERE v.[VersionNumber] > 1 "
                + "AND NOT EXISTS (SELECT 1 FROM [TemplateSections] c WHERE c.[ParentSectionId] = s.[Id]);");

            migrationBuilder.Sql(
                "UPDATE s SET s.[TableTemplateVersionId] = v.[TableTemplateId] FROM [TemplateSections] s "
                + "JOIN [TableTemplateVersions] v ON v.[Id] = s.[TableTemplateVersionId];");

            migrationBuilder.Sql(
                "UPDATE t SET t.[TableTemplateVersionId] = v.[TableTemplateId] FROM [SheetTables] t "
                + "JOIN [TableTemplateVersions] v ON v.[Id] = t.[TableTemplateVersionId];");

            migrationBuilder.DropTable(
                name: "TableTemplateVersions");

            migrationBuilder.RenameColumn(
                name: "TableTemplateVersionId",
                table: "TemplateSections",
                newName: "TableTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateSections_TableTemplateVersionId",
                table: "TemplateSections",
                newName: "IX_TemplateSections_TableTemplateId");

            migrationBuilder.RenameColumn(
                name: "TableTemplateVersionId",
                table: "SheetTables",
                newName: "TableTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_SheetTables_TableTemplateVersionId",
                table: "SheetTables",
                newName: "IX_SheetTables_TableTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_SheetTables_TableTemplates_TableTemplateId",
                table: "SheetTables",
                column: "TableTemplateId",
                principalTable: "TableTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateSections_TableTemplates_TableTemplateId",
                table: "TemplateSections",
                column: "TableTemplateId",
                principalTable: "TableTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
