using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LookupKey",
                table: "TemplateCells",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LookupValue",
                schema: "values",
                table: "TextValues",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                computedColumnSql: "CONVERT(nvarchar(200), LEFT([Value], 200))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_TextValues_LookupValue",
                schema: "values",
                table: "TextValues",
                column: "LookupValue")
                .Annotation("SqlServer:Include", new[] { "SheetCellId", "SheetRowRevisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCells_LookupKey",
                table: "TemplateCells",
                column: "LookupKey",
                filter: "[LookupKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TextValues_LookupValue",
                schema: "values",
                table: "TextValues");

            migrationBuilder.DropIndex(
                name: "IX_TemplateCells_LookupKey",
                table: "TemplateCells");

            migrationBuilder.DropColumn(
                name: "LookupValue",
                schema: "values",
                table: "TextValues");

            migrationBuilder.DropColumn(
                name: "LookupKey",
                table: "TemplateCells");
        }
    }
}
