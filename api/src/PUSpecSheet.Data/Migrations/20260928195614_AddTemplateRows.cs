using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateCells_TemplateSections_TemplateSectionId",
                table: "TemplateCells");

            migrationBuilder.DropIndex(
                name: "IX_TemplateCells_TemplateSectionId_Row_Column",
                table: "TemplateCells");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateCells_Row",
                table: "TemplateCells");

            migrationBuilder.DropColumn(
                name: "Row",
                table: "TemplateCells");

            migrationBuilder.RenameColumn(
                name: "TemplateSectionId",
                table: "TemplateCells",
                newName: "TemplateRowId");

            migrationBuilder.CreateTable(
                name: "TemplateRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateSectionId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateRows_TemplateSections_TemplateSectionId",
                        column: x => x.TemplateSectionId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCells_TemplateRowId_Column",
                table: "TemplateCells",
                columns: new[] { "TemplateRowId", "Column" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateRows_TemplateSectionId",
                table: "TemplateRows",
                column: "TemplateSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateCells_TemplateRows_TemplateRowId",
                table: "TemplateCells",
                column: "TemplateRowId",
                principalTable: "TemplateRows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateCells_TemplateRows_TemplateRowId",
                table: "TemplateCells");

            migrationBuilder.DropTable(
                name: "TemplateRows");

            migrationBuilder.DropIndex(
                name: "IX_TemplateCells_TemplateRowId_Column",
                table: "TemplateCells");

            migrationBuilder.RenameColumn(
                name: "TemplateRowId",
                table: "TemplateCells",
                newName: "TemplateSectionId");

            migrationBuilder.AddColumn<int>(
                name: "Row",
                table: "TemplateCells",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCells_TemplateSectionId_Row_Column",
                table: "TemplateCells",
                columns: new[] { "TemplateSectionId", "Row", "Column" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateCells_Row",
                table: "TemplateCells",
                sql: "[Row] >= 1");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateCells_TemplateSections_TemplateSectionId",
                table: "TemplateCells",
                column: "TemplateSectionId",
                principalTable: "TemplateSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
