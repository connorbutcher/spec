using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTableTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Orientation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableTemplates_SheetTypes_SheetTypeId",
                        column: x => x.SheetTypeId,
                        principalTable: "SheetTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TemplateSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableTemplateId = table.Column<int>(type: "int", nullable: false),
                    ParentSectionId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateSections_TableTemplates_TableTemplateId",
                        column: x => x.TableTemplateId,
                        principalTable: "TableTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TemplateSections_TemplateSections_ParentSectionId",
                        column: x => x.ParentSectionId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TemplateCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateSectionId = table.Column<int>(type: "int", nullable: false),
                    Row = table.Column<int>(type: "int", nullable: false),
                    Column = table.Column<int>(type: "int", nullable: false),
                    RowSpan = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ColumnSpan = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateCells", x => x.Id);
                    table.CheckConstraint("CK_TemplateCells_Column", "[Column] >= 1");
                    table.CheckConstraint("CK_TemplateCells_ColumnSpan", "[ColumnSpan] >= 1");
                    table.CheckConstraint("CK_TemplateCells_Row", "[Row] >= 1");
                    table.CheckConstraint("CK_TemplateCells_RowSpan", "[RowSpan] >= 1");
                    table.ForeignKey(
                        name: "FK_TemplateCells_TemplateSections_TemplateSectionId",
                        column: x => x.TemplateSectionId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableTemplates_SheetTypeId_Name",
                table: "TableTemplates",
                columns: new[] { "SheetTypeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCells_TemplateSectionId_Row_Column",
                table: "TemplateCells",
                columns: new[] { "TemplateSectionId", "Row", "Column" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSections_ParentSectionId",
                table: "TemplateSections",
                column: "ParentSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSections_TableTemplateId",
                table: "TemplateSections",
                column: "TableTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateCells");

            migrationBuilder.DropTable(
                name: "TemplateSections");

            migrationBuilder.DropTable(
                name: "TableTemplates");
        }
    }
}
