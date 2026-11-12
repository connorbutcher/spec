using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLabelledTextValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabelledTextValues",
                schema: "values",
                columns: table => new
                {
                    SheetRowRevisionId = table.Column<int>(type: "int", nullable: false),
                    SheetCellId = table.Column<int>(type: "int", nullable: false),
                    SlotKey = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    LookupValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, computedColumnSql: "CONVERT(nvarchar(200), LEFT([Value], 200))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelledTextValues", x => new { x.SheetRowRevisionId, x.SheetCellId, x.SlotKey });
                    table.ForeignKey(
                        name: "FK_LabelledTextValues_SheetCells_SheetCellId",
                        column: x => x.SheetCellId,
                        principalTable: "SheetCells",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LabelledTextValues_SheetRowRevisions_SheetRowRevisionId",
                        column: x => x.SheetRowRevisionId,
                        principalTable: "SheetRowRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabelledTextValues_LookupValue",
                schema: "values",
                table: "LabelledTextValues",
                column: "LookupValue")
                .Annotation("SqlServer:Include", new[] { "SheetCellId", "SheetRowRevisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_LabelledTextValues_SheetCellId",
                schema: "values",
                table: "LabelledTextValues",
                column: "SheetCellId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabelledTextValues",
                schema: "values");
        }
    }
}
