using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCellTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "TemplateCells",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            // Cells that already exist become the seeded "Text" cell type (id 2), inserted below
            // before the foreign key is added.
            migrationBuilder.AddColumn<int>(
                name: "CellTypeId",
                table: "TemplateCells",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "TemplateCells",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CellTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: true),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CellTypeOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CellTypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellTypeOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CellTypeOptions_CellTypes_CellTypeId",
                        column: x => x.CellTypeId,
                        principalTable: "CellTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CellTypes",
                columns: new[] { "Id", "DecimalPlaces", "Description", "DisplayOrder", "Kind", "MaxLength", "MaxValue", "MinValue", "Name", "Unit" },
                values: new object[,]
                {
                    { 1, null, "Fixed text such as a header.", 1, "Label", null, null, null, "Label", null },
                    { 2, null, null, 2, "Text", 200, null, null, "Text", null },
                    { 3, 2, null, 3, "Number", null, null, null, "Number", null },
                    { 4, null, null, 4, "Date", null, null, null, "Date", null },
                    { 5, null, null, 5, "Checkbox", null, null, null, "Checkbox", null },
                    { 6, null, null, 6, "Dropdown", null, null, null, "Pass / Fail", null }
                });

            migrationBuilder.InsertData(
                table: "CellTypeOptions",
                columns: new[] { "Id", "CellTypeId", "DisplayOrder", "Value" },
                values: new object[,]
                {
                    { 1, 6, 1, "Pass" },
                    { 2, 6, 2, "Fail" },
                    { 3, 6, 3, "N/A" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCells_CellTypeId",
                table: "TemplateCells",
                column: "CellTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CellTypeOptions_CellTypeId_Value",
                table: "CellTypeOptions",
                columns: new[] { "CellTypeId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CellTypes_Name",
                table: "CellTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateCells_CellTypes_CellTypeId",
                table: "TemplateCells",
                column: "CellTypeId",
                principalTable: "CellTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateCells_CellTypes_CellTypeId",
                table: "TemplateCells");

            migrationBuilder.DropTable(
                name: "CellTypeOptions");

            migrationBuilder.DropTable(
                name: "CellTypes");

            migrationBuilder.DropIndex(
                name: "IX_TemplateCells_CellTypeId",
                table: "TemplateCells");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "TemplateCells");

            migrationBuilder.DropColumn(
                name: "CellTypeId",
                table: "TemplateCells");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "TemplateCells");
        }
    }
}
