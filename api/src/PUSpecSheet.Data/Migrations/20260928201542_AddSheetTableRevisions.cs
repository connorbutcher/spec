using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSheetTableRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SheetTableRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetTableId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AuthorUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SheetVersionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetTableRevisions", x => x.Id);
                    table.CheckConstraint("CK_SheetTableRevisions_PublishedAt", "([Status] = 0 AND [PublishedAtUtc] IS NULL) OR ([Status] = 1 AND [PublishedAtUtc] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_SheetTableRevisions_SheetTables_SheetTableId",
                        column: x => x.SheetTableId,
                        principalTable: "SheetTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetTableRevisions_SheetVersions_SheetVersionId",
                        column: x => x.SheetVersionId,
                        principalTable: "SheetVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SheetTableRevisions_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SheetTableRevisions_AuthorUserId_Status",
                table: "SheetTableRevisions",
                columns: new[] { "AuthorUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetTableRevisions_Published",
                table: "SheetTableRevisions",
                columns: new[] { "SheetTableId", "PublishedAtUtc" },
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SheetTableRevisions_SheetTableId_RevisionNumber",
                table: "SheetTableRevisions",
                columns: new[] { "SheetTableId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetTableRevisions_SheetVersionId",
                table: "SheetTableRevisions",
                column: "SheetVersionId");

            migrationBuilder.CreateIndex(
                name: "UX_SheetTableRevisions_OneDraftPerTable",
                table: "SheetTableRevisions",
                column: "SheetTableId",
                unique: true,
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SheetTableRevisions");
        }
    }
}
