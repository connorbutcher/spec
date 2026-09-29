using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <summary>
    /// Moves a cell type's flat settings (max length, decimals, min, max, unit) into a JSON
    /// configuration, adds a JSON style, and lets template cells override both. Renames the Label kind to
    /// Heading and Dropdown to TextDropdown, and seeds a Group cell type.
    /// </summary>
    public partial class CellTypeConfigurationAndStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigurationOverride",
                table: "TemplateCells",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StyleOverride",
                table: "TemplateCells",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Configuration",
                table: "CellTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Style",
                table: "CellTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.Sql("UPDATE [CellTypes] SET [Kind] = 'Heading' WHERE [Kind] = 'Label';");
            migrationBuilder.Sql("UPDATE [CellTypes] SET [Kind] = 'TextDropdown' WHERE [Kind] = 'Dropdown';");

            // Build each type's configuration from its own flat settings, so edits users made to the
            // seeded types survive. Unset settings are left out of the JSON.
            migrationBuilder.Sql(
                """
                UPDATE [CellTypes] SET [Configuration] = CASE [Kind]
                    WHEN 'Text' THEN CONCAT(
                        '{"kind":"Text"',
                        CASE WHEN [MaxLength] IS NOT NULL THEN CONCAT(',"maxLength":', [MaxLength]) END,
                        '}')
                    WHEN 'Number' THEN CONCAT(
                        '{"kind":"Number"',
                        CASE WHEN [DecimalPlaces] IS NOT NULL THEN CONCAT(',"decimalPlaces":', [DecimalPlaces]) END,
                        CASE WHEN [MinValue] IS NOT NULL THEN CONCAT(',"minValue":', CAST([MinValue] AS nvarchar(40))) END,
                        CASE WHEN [MaxValue] IS NOT NULL THEN CONCAT(',"maxValue":', CAST([MaxValue] AS nvarchar(40))) END,
                        CASE WHEN [Unit] IS NOT NULL THEN CONCAT(',"unit":"', STRING_ESCAPE([Unit], 'json'), '"') END,
                        '}')
                    ELSE CONCAT('{"kind":"', [Kind], '"}')
                END;
                """);

            migrationBuilder.UpdateData(
                table: "CellTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Style" },
                values: new object[] { "Fixed text such as a column header.", "Heading", "{\"bold\":true}" });

            migrationBuilder.UpdateData(
                table: "CellTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Style",
                value: "{\"align\":\"Right\"}");

            migrationBuilder.UpdateData(
                table: "CellTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Style",
                value: "{\"align\":\"Center\"}");

            migrationBuilder.InsertData(
                table: "CellTypes",
                columns: new[] { "Id", "Configuration", "Description", "DisplayOrder", "Kind", "Name", "Style" },
                values: new object[] { 7, "{\"kind\":\"Group\"}", "A caption that groups the cells around it.", 7, "Group", "Group", "{\"bold\":true,\"backgroundColor\":\"#f1f5f9\"}" });

            migrationBuilder.DropColumn(
                name: "DecimalPlaces",
                table: "CellTypes");

            migrationBuilder.DropColumn(
                name: "MaxLength",
                table: "CellTypes");

            migrationBuilder.DropColumn(
                name: "MaxValue",
                table: "CellTypes");

            migrationBuilder.DropColumn(
                name: "MinValue",
                table: "CellTypes");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "CellTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DecimalPlaces",
                table: "CellTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxLength",
                table: "CellTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxValue",
                table: "CellTypes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinValue",
                table: "CellTypes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "CellTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [CellTypes] SET
                    [MaxLength] = CASE WHEN [Kind] = 'Text' THEN TRY_CAST(JSON_VALUE([Configuration], '$.maxLength') AS int) END,
                    [DecimalPlaces] = CASE WHEN [Kind] = 'Number' THEN TRY_CAST(JSON_VALUE([Configuration], '$.decimalPlaces') AS int) END,
                    [MinValue] = CASE WHEN [Kind] = 'Number' THEN TRY_CAST(JSON_VALUE([Configuration], '$.minValue') AS decimal(18,4)) END,
                    [MaxValue] = CASE WHEN [Kind] = 'Number' THEN TRY_CAST(JSON_VALUE([Configuration], '$.maxValue') AS decimal(18,4)) END,
                    [Unit] = CASE WHEN [Kind] = 'Number' THEN LEFT(JSON_VALUE([Configuration], '$.unit'), 20) END;
                """);

            migrationBuilder.Sql("UPDATE [CellTypes] SET [Kind] = 'Label' WHERE [Kind] IN ('Heading', 'Group');");
            migrationBuilder.Sql("UPDATE [CellTypes] SET [Kind] = 'Dropdown' WHERE [Kind] IN ('TextDropdown', 'NumberDropdown');");

            migrationBuilder.Sql("UPDATE [TemplateCells] SET [CellTypeId] = 1 WHERE [CellTypeId] = 7;");
            migrationBuilder.DeleteData(
                table: "CellTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.UpdateData(
                table: "CellTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Fixed text such as a header.", "Label" });

            migrationBuilder.DropColumn(
                name: "ConfigurationOverride",
                table: "TemplateCells");

            migrationBuilder.DropColumn(
                name: "StyleOverride",
                table: "TemplateCells");

            migrationBuilder.DropColumn(
                name: "Configuration",
                table: "CellTypes");

            migrationBuilder.DropColumn(
                name: "Style",
                table: "CellTypes");
        }
    }
}
