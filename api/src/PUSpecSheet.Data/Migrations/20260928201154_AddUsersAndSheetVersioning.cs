using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndSheetVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhaseId = table.Column<int>(type: "int", nullable: false),
                    SheetTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sheets_Phases_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "Phases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sheets_SheetTypes_SheetTypeId",
                        column: x => x.SheetTypeId,
                        principalTable: "SheetTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SheetTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetId = table.Column<int>(type: "int", nullable: false),
                    TableTemplateId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SheetTables_Sheets_SheetId",
                        column: x => x.SheetId,
                        principalTable: "Sheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetTables_TableTemplates_TableTemplateId",
                        column: x => x.TableTemplateId,
                        principalTable: "TableTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SheetVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedByUserId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SheetVersions_Sheets_SheetId",
                        column: x => x.SheetId,
                        principalTable: "Sheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetVersions_Users_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SheetRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetTableId = table.Column<int>(type: "int", nullable: false),
                    TemplateRowId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SheetRows_SheetTables_SheetTableId",
                        column: x => x.SheetTableId,
                        principalTable: "SheetTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetRows_TemplateRows_TemplateRowId",
                        column: x => x.TemplateRowId,
                        principalTable: "TemplateRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SheetRowRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetRowId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AuthorUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SheetVersionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetRowRevisions", x => x.Id);
                    table.CheckConstraint("CK_SheetRowRevisions_PublishedAt", "([Status] = 0 AND [PublishedAtUtc] IS NULL) OR ([Status] = 1 AND [PublishedAtUtc] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_SheetRowRevisions_SheetRows_SheetRowId",
                        column: x => x.SheetRowId,
                        principalTable: "SheetRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetRowRevisions_SheetVersions_SheetVersionId",
                        column: x => x.SheetVersionId,
                        principalTable: "SheetVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SheetRowRevisions_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAtUtc", "DisplayName", "Email", "IsActive", "UserName" },
                values: new object[] { 1, new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Developer", null, true, "developer" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetCellValues_SheetRowRevisionId_TemplateCellId",
                table: "SheetCellValues",
                columns: new[] { "SheetRowRevisionId", "TemplateCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetCellValues_TemplateCellId",
                table: "SheetCellValues",
                column: "TemplateCellId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetRowRevisions_AuthorUserId_Status",
                table: "SheetRowRevisions",
                columns: new[] { "AuthorUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetRowRevisions_Published",
                table: "SheetRowRevisions",
                columns: new[] { "SheetRowId", "PublishedAtUtc" },
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SheetRowRevisions_SheetRowId_RevisionNumber",
                table: "SheetRowRevisions",
                columns: new[] { "SheetRowId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetRowRevisions_SheetVersionId",
                table: "SheetRowRevisions",
                column: "SheetVersionId");

            migrationBuilder.CreateIndex(
                name: "UX_SheetRowRevisions_OneDraftPerRow",
                table: "SheetRowRevisions",
                column: "SheetRowId",
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SheetRows_SheetTableId_DisplayOrder",
                table: "SheetRows",
                columns: new[] { "SheetTableId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetRows_TemplateRowId",
                table: "SheetRows",
                column: "TemplateRowId");

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_PhaseId_SheetTypeId",
                table: "Sheets",
                columns: new[] { "PhaseId", "SheetTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_SheetTypeId",
                table: "Sheets",
                column: "SheetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetTables_SheetId_DisplayOrder",
                table: "SheetTables",
                columns: new[] { "SheetId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetTables_TableTemplateId",
                table: "SheetTables",
                column: "TableTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetVersions_PublishedByUserId",
                table: "SheetVersions",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetVersions_SheetId_PublishedAtUtc",
                table: "SheetVersions",
                columns: new[] { "SheetId", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetVersions_SheetId_VersionNumber",
                table: "SheetVersions",
                columns: new[] { "SheetId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SheetCellValues");

            migrationBuilder.DropTable(
                name: "SheetRowRevisions");

            migrationBuilder.DropTable(
                name: "SheetRows");

            migrationBuilder.DropTable(
                name: "SheetVersions");

            migrationBuilder.DropTable(
                name: "SheetTables");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Sheets");
        }
    }
}
