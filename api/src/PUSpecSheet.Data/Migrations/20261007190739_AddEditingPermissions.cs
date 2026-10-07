using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEditingPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { 3, "Create, change and delete table templates and everything in them.", "templates.manage" },
                    { 4, "Create, change and delete cell types.", "cellTypes.manage" },
                    { 5, "Change a sheet: its tables, sections, rows, column blocks and cell values.", "sheets.edit" },
                    { 6, "Publish a sheet's drafts as its next version.", "sheets.publish" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
