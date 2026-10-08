using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCellInstanceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CellSettings",
                schema: "values",
                columns: table => new
                {
                    SheetRowRevisionId = table.Column<int>(type: "int", nullable: false),
                    SheetCellId = table.Column<int>(type: "int", nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellSettings", x => new { x.SheetRowRevisionId, x.SheetCellId });
                    table.ForeignKey(
                        name: "FK_CellSettings_SheetCells_SheetCellId",
                        column: x => x.SheetCellId,
                        principalTable: "SheetCells",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CellSettings_SheetRowRevisions_SheetRowRevisionId",
                        column: x => x.SheetRowRevisionId,
                        principalTable: "SheetRowRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CellSettings_SheetCellId",
                schema: "values",
                table: "CellSettings",
                column: "SheetCellId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CellSettings",
                schema: "values");
        }
    }
}
