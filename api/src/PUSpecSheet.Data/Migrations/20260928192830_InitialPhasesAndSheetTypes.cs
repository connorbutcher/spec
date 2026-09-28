using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPhasesAndSheetTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Phases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ParentPhaseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Phases_Phases_ParentPhaseId",
                        column: x => x.ParentPhaseId,
                        principalTable: "Phases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SheetTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhaseSheetTypes",
                columns: table => new
                {
                    PhaseId = table.Column<int>(type: "int", nullable: false),
                    SheetTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseSheetTypes", x => new { x.PhaseId, x.SheetTypeId });
                    table.ForeignKey(
                        name: "FK_PhaseSheetTypes_Phases_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "Phases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhaseSheetTypes_SheetTypes_SheetTypeId",
                        column: x => x.SheetTypeId,
                        principalTable: "SheetTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SheetTypes",
                columns: new[] { "Id", "DisplayOrder", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Specification" },
                    { 2, 2, "Parts" },
                    { 3, 3, "PFKs" },
                    { 4, 4, "Engine Specifications" },
                    { 5, 5, "Confirmation" },
                    { 6, 6, "Torque Sheet" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Phases_Code",
                table: "Phases",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Phases_ParentPhaseId",
                table: "Phases",
                column: "ParentPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseSheetTypes_SheetTypeId",
                table: "PhaseSheetTypes",
                column: "SheetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetTypes_Name",
                table: "SheetTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhaseSheetTypes");

            migrationBuilder.DropTable(
                name: "Phases");

            migrationBuilder.DropTable(
                name: "SheetTypes");
        }
    }
}
