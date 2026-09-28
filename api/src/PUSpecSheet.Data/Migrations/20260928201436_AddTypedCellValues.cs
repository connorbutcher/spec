using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTypedCellValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SheetCellValues");

            migrationBuilder.EnsureSchema(
                name: "values");

            migrationBuilder.CreateTable(
                name: "BooleanValues",
                schema: "values",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetRowRevisionId = table.Column<int>(type: "int", nullable: false),
                    TemplateCellId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BooleanValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BooleanValues_SheetRowRevisions_SheetRowRevisionId",
                        column: x => x.SheetRowRevisionId,
                        principalTable: "SheetRowRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BooleanValues_TemplateCells_TemplateCellId",
                        column: x => x.TemplateCellId,
                        principalTable: "TemplateCells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DateValues",
                schema: "values",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetRowRevisionId = table.Column<int>(type: "int", nullable: false),
                    TemplateCellId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DateValues_SheetRowRevisions_SheetRowRevisionId",
                        column: x => x.SheetRowRevisionId,
                        principalTable: "SheetRowRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DateValues_TemplateCells_TemplateCellId",
                        column: x => x.TemplateCellId,
                        principalTable: "TemplateCells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NumericValues",
                schema: "values",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetRowRevisionId = table.Column<int>(type: "int", nullable: false),
                    TemplateCellId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumericValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NumericValues_SheetRowRevisions_SheetRowRevisionId",
                        column: x => x.SheetRowRevisionId,
                        principalTable: "SheetRowRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NumericValues_TemplateCells_TemplateCellId",
                        column: x => x.TemplateCellId,
                        principalTable: "TemplateCells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptionValues",
                schema: "values",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetRowRevisionId = table.Column<int>(type: "int", nullable: false),
                    TemplateCellId = table.Column<int>(type: "int", nullable: false),
                    CellTypeOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionValues_CellTypeOptions_CellTypeOptionId",
                        column: x => x.CellTypeOptionId,
                        principalTable: "CellTypeOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptionValues_SheetRowRevisions_SheetRowRevisionId",
                        column: x => x.SheetRowRevisionId,
                        principalTable: "SheetRowRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptionValues_TemplateCells_TemplateCellId",
                        column: x => x.TemplateCellId,
                        principalTable: "TemplateCells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TextValues",
                schema: "values",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetRowRevisionId = table.Column<int>(type: "int", nullable: false),
                    TemplateCellId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextValues_SheetRowRevisions_SheetRowRevisionId",
                        column: x => x.SheetRowRevisionId,
                        principalTable: "SheetRowRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TextValues_TemplateCells_TemplateCellId",
                        column: x => x.TemplateCellId,
                        principalTable: "TemplateCells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BooleanValues_SheetRowRevisionId_TemplateCellId",
                schema: "values",
                table: "BooleanValues",
                columns: new[] { "SheetRowRevisionId", "TemplateCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BooleanValues_TemplateCellId",
                schema: "values",
                table: "BooleanValues",
                column: "TemplateCellId");

            migrationBuilder.CreateIndex(
                name: "IX_DateValues_SheetRowRevisionId_TemplateCellId",
                schema: "values",
                table: "DateValues",
                columns: new[] { "SheetRowRevisionId", "TemplateCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DateValues_TemplateCellId",
                schema: "values",
                table: "DateValues",
                column: "TemplateCellId");

            migrationBuilder.CreateIndex(
                name: "IX_NumericValues_SheetRowRevisionId_TemplateCellId",
                schema: "values",
                table: "NumericValues",
                columns: new[] { "SheetRowRevisionId", "TemplateCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumericValues_TemplateCellId",
                schema: "values",
                table: "NumericValues",
                column: "TemplateCellId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionValues_CellTypeOptionId",
                schema: "values",
                table: "OptionValues",
                column: "CellTypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionValues_SheetRowRevisionId_TemplateCellId",
                schema: "values",
                table: "OptionValues",
                columns: new[] { "SheetRowRevisionId", "TemplateCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionValues_TemplateCellId",
                schema: "values",
                table: "OptionValues",
                column: "TemplateCellId");

            migrationBuilder.CreateIndex(
                name: "IX_TextValues_SheetRowRevisionId_TemplateCellId",
                schema: "values",
                table: "TextValues",
                columns: new[] { "SheetRowRevisionId", "TemplateCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TextValues_TemplateCellId",
                schema: "values",
                table: "TextValues",
                column: "TemplateCellId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BooleanValues",
                schema: "values");

            migrationBuilder.DropTable(
                name: "DateValues",
                schema: "values");

            migrationBuilder.DropTable(
                name: "NumericValues",
                schema: "values");

            migrationBuilder.DropTable(
                name: "OptionValues",
                schema: "values");

            migrationBuilder.DropTable(
                name: "TextValues",
                schema: "values");

            migrationBuilder.CreateTable(
                name: "SheetCellValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetRowRevisionId = table.Column<int>(type: "int", nullable: false),
                    TemplateCellId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetCellValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SheetCellValues_SheetRowRevisions_SheetRowRevisionId",
                        column: x => x.SheetRowRevisionId,
                        principalTable: "SheetRowRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetCellValues_TemplateCells_TemplateCellId",
                        column: x => x.TemplateCellId,
                        principalTable: "TemplateCells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SheetCellValues_SheetRowRevisionId_TemplateCellId",
                table: "SheetCellValues",
                columns: new[] { "SheetRowRevisionId", "TemplateCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetCellValues_TemplateCellId",
                table: "SheetCellValues",
                column: "TemplateCellId");
        }
    }
}
