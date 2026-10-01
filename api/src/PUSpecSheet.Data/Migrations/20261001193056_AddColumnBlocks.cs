using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemplateCells_TemplateRowId_Column",
                table: "TemplateCells");

            migrationBuilder.DropIndex(
                name: "IX_SheetCells_SheetRowId_TemplateCellId",
                table: "SheetCells");

            migrationBuilder.AddColumn<int>(
                name: "TemplateColumnBlockId",
                table: "TemplateCells",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SheetColumnBlockId",
                table: "SheetCells",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TemplateColumnBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableTemplateVersionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    MinInstances = table.Column<int>(type: "int", nullable: false),
                    MaxInstances = table.Column<int>(type: "int", nullable: true),
                    InitialInstances = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateColumnBlocks", x => x.Id);
                    table.CheckConstraint("CK_TemplateColumnBlocks_Instances", "[MinInstances] >= 0 AND [InitialInstances] >= [MinInstances] AND ([MaxInstances] IS NULL OR ([MaxInstances] >= 1 AND [MaxInstances] >= [InitialInstances]))");
                    table.ForeignKey(
                        name: "FK_TemplateColumnBlocks_TableTemplateVersions_TableTemplateVersionId",
                        column: x => x.TableTemplateVersionId,
                        principalTable: "TableTemplateVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SheetColumnBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SheetTableId = table.Column<int>(type: "int", nullable: false),
                    TemplateColumnBlockId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetColumnBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SheetColumnBlocks_SheetTables_SheetTableId",
                        column: x => x.SheetTableId,
                        principalTable: "SheetTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetColumnBlocks_TemplateColumnBlocks_TemplateColumnBlockId",
                        column: x => x.TemplateColumnBlockId,
                        principalTable: "TemplateColumnBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SheetColumnBlockRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetColumnBlockId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AuthorUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupersededAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    SheetVersionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetColumnBlockRevisions", x => x.Id);
                    table.CheckConstraint("CK_SheetColumnBlockRevisions_PublishedAt", "([Status] = 0 AND [PublishedAtUtc] IS NULL) OR ([Status] = 1 AND [PublishedAtUtc] IS NOT NULL)");
                    table.CheckConstraint("CK_SheetColumnBlockRevisions_SupersededAt", "[SupersededAtUtc] IS NULL OR ([Status] = 1 AND [SupersededAtUtc] >= [PublishedAtUtc])");
                    table.ForeignKey(
                        name: "FK_SheetColumnBlockRevisions_SheetColumnBlocks_SheetColumnBlockId",
                        column: x => x.SheetColumnBlockId,
                        principalTable: "SheetColumnBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetColumnBlockRevisions_SheetVersions_SheetVersionId",
                        column: x => x.SheetVersionId,
                        principalTable: "SheetVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SheetColumnBlockRevisions_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCells_TemplateColumnBlockId",
                table: "TemplateCells",
                column: "TemplateColumnBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCells_TemplateRowId_TemplateColumnBlockId_Column",
                table: "TemplateCells",
                columns: new[] { "TemplateRowId", "TemplateColumnBlockId", "Column" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetCells_SheetColumnBlockId",
                table: "SheetCells",
                column: "SheetColumnBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetCells_SheetRowId_TemplateCellId_SheetColumnBlockId",
                table: "SheetCells",
                columns: new[] { "SheetRowId", "TemplateCellId", "SheetColumnBlockId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetColumnBlockRevisions_AuthorUserId_Status",
                table: "SheetColumnBlockRevisions",
                columns: new[] { "AuthorUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetColumnBlockRevisions_Published",
                table: "SheetColumnBlockRevisions",
                columns: new[] { "SheetColumnBlockId", "PublishedAtUtc" },
                filter: "[Status] = 1")
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetColumnBlockRevisions_SheetColumnBlockId_RevisionNumber",
                table: "SheetColumnBlockRevisions",
                columns: new[] { "SheetColumnBlockId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetColumnBlockRevisions_SheetVersionId",
                table: "SheetColumnBlockRevisions",
                column: "SheetVersionId");

            migrationBuilder.CreateIndex(
                name: "UX_SheetColumnBlockRevisions_OneCurrentPerBlock",
                table: "SheetColumnBlockRevisions",
                column: "SheetColumnBlockId",
                unique: true,
                filter: "[Status] = 1 AND [SupersededAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_SheetColumnBlockRevisions_OneDraftPerBlock",
                table: "SheetColumnBlockRevisions",
                column: "SheetColumnBlockId",
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SheetColumnBlocks_PublicId",
                table: "SheetColumnBlocks",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetColumnBlocks_SheetTableId",
                table: "SheetColumnBlocks",
                column: "SheetTableId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetColumnBlocks_TemplateColumnBlockId",
                table: "SheetColumnBlocks",
                column: "TemplateColumnBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateColumnBlocks_TableTemplateVersionId",
                table: "TemplateColumnBlocks",
                column: "TableTemplateVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SheetCells_SheetColumnBlocks_SheetColumnBlockId",
                table: "SheetCells",
                column: "SheetColumnBlockId",
                principalTable: "SheetColumnBlocks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateCells_TemplateColumnBlocks_TemplateColumnBlockId",
                table: "TemplateCells",
                column: "TemplateColumnBlockId",
                principalTable: "TemplateColumnBlocks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SheetCells_SheetColumnBlocks_SheetColumnBlockId",
                table: "SheetCells");

            migrationBuilder.DropForeignKey(
                name: "FK_TemplateCells_TemplateColumnBlocks_TemplateColumnBlockId",
                table: "TemplateCells");

            migrationBuilder.DropTable(
                name: "SheetColumnBlockRevisions");

            migrationBuilder.DropTable(
                name: "SheetColumnBlocks");

            migrationBuilder.DropTable(
                name: "TemplateColumnBlocks");

            migrationBuilder.DropIndex(
                name: "IX_TemplateCells_TemplateColumnBlockId",
                table: "TemplateCells");

            migrationBuilder.DropIndex(
                name: "IX_TemplateCells_TemplateRowId_TemplateColumnBlockId_Column",
                table: "TemplateCells");

            migrationBuilder.DropIndex(
                name: "IX_SheetCells_SheetColumnBlockId",
                table: "SheetCells");

            migrationBuilder.DropIndex(
                name: "IX_SheetCells_SheetRowId_TemplateCellId_SheetColumnBlockId",
                table: "SheetCells");

            migrationBuilder.DropColumn(
                name: "TemplateColumnBlockId",
                table: "TemplateCells");

            migrationBuilder.DropColumn(
                name: "SheetColumnBlockId",
                table: "SheetCells");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCells_TemplateRowId_Column",
                table: "TemplateCells",
                columns: new[] { "TemplateRowId", "Column" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetCells_SheetRowId_TemplateCellId",
                table: "SheetCells",
                columns: new[] { "SheetRowId", "TemplateCellId" },
                unique: true);
        }
    }
}
