using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSheetSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SheetSubmissionId",
                table: "SheetTableRevisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SheetSubmissionId",
                table: "SheetSectionRevisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SheetSubmissionId",
                table: "SheetRowRevisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SheetSubmissionId",
                table: "SheetColumnBlockRevisions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SheetSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedByUserId = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SheetVersionId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetSubmissions", x => x.Id);
                    table.CheckConstraint("CK_SheetSubmissions_DecidedAt", "([Status] = 0 AND [DecidedAtUtc] IS NULL) OR ([Status] <> 0 AND [DecidedAtUtc] IS NOT NULL)");
                    table.CheckConstraint("CK_SheetSubmissions_Status", "[Status] IN (0, 1, 2, 3)");
                    table.CheckConstraint("CK_SheetSubmissions_Version", "[SheetVersionId] IS NULL OR [Status] = 1");
                    table.ForeignKey(
                        name: "FK_SheetSubmissions_SheetVersions_SheetVersionId",
                        column: x => x.SheetVersionId,
                        principalTable: "SheetVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SheetSubmissions_Sheets_SheetId",
                        column: x => x.SheetId,
                        principalTable: "Sheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetSubmissions_Users_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SheetSubmissionApprovers",
                columns: table => new
                {
                    SheetSubmissionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetSubmissionApprovers", x => new { x.SheetSubmissionId, x.UserId });
                    table.CheckConstraint("CK_SheetSubmissionApprovers_Decision", "[Decision] IN (0, 1, 2)");
                    table.CheckConstraint("CK_SheetSubmissionApprovers_Reason", "[Decision] <> 2 OR (LEN([Reason]) > 0)");
                    table.ForeignKey(
                        name: "FK_SheetSubmissionApprovers_SheetSubmissions_SheetSubmissionId",
                        column: x => x.SheetSubmissionId,
                        principalTable: "SheetSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetSubmissionApprovers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "Publish a sheet's drafts as its next version, once the chosen approvers approve.");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { 7, "Approve or reject the changes someone else submitted for publishing.", "sheets.approve" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetTableRevisions_SheetSubmissionId",
                table: "SheetTableRevisions",
                column: "SheetSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSectionRevisions_SheetSubmissionId",
                table: "SheetSectionRevisions",
                column: "SheetSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetRowRevisions_SheetSubmissionId",
                table: "SheetRowRevisions",
                column: "SheetSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetColumnBlockRevisions_SheetSubmissionId",
                table: "SheetColumnBlockRevisions",
                column: "SheetSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSubmissionApprovers_UserId_Decision",
                table: "SheetSubmissionApprovers",
                columns: new[] { "UserId", "Decision" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetSubmissions_SheetId_Status",
                table: "SheetSubmissions",
                columns: new[] { "SheetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetSubmissions_SheetVersionId",
                table: "SheetSubmissions",
                column: "SheetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSubmissions_SubmittedByUserId_Status",
                table: "SheetSubmissions",
                columns: new[] { "SubmittedByUserId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_SheetColumnBlockRevisions_SheetSubmissions_SheetSubmissionId",
                table: "SheetColumnBlockRevisions",
                column: "SheetSubmissionId",
                principalTable: "SheetSubmissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SheetRowRevisions_SheetSubmissions_SheetSubmissionId",
                table: "SheetRowRevisions",
                column: "SheetSubmissionId",
                principalTable: "SheetSubmissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SheetSectionRevisions_SheetSubmissions_SheetSubmissionId",
                table: "SheetSectionRevisions",
                column: "SheetSubmissionId",
                principalTable: "SheetSubmissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SheetTableRevisions_SheetSubmissions_SheetSubmissionId",
                table: "SheetTableRevisions",
                column: "SheetSubmissionId",
                principalTable: "SheetSubmissions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SheetColumnBlockRevisions_SheetSubmissions_SheetSubmissionId",
                table: "SheetColumnBlockRevisions");

            migrationBuilder.DropForeignKey(
                name: "FK_SheetRowRevisions_SheetSubmissions_SheetSubmissionId",
                table: "SheetRowRevisions");

            migrationBuilder.DropForeignKey(
                name: "FK_SheetSectionRevisions_SheetSubmissions_SheetSubmissionId",
                table: "SheetSectionRevisions");

            migrationBuilder.DropForeignKey(
                name: "FK_SheetTableRevisions_SheetSubmissions_SheetSubmissionId",
                table: "SheetTableRevisions");

            migrationBuilder.DropTable(
                name: "SheetSubmissionApprovers");

            migrationBuilder.DropTable(
                name: "SheetSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_SheetTableRevisions_SheetSubmissionId",
                table: "SheetTableRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetSectionRevisions_SheetSubmissionId",
                table: "SheetSectionRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetRowRevisions_SheetSubmissionId",
                table: "SheetRowRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetColumnBlockRevisions_SheetSubmissionId",
                table: "SheetColumnBlockRevisions");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "SheetSubmissionId",
                table: "SheetTableRevisions");

            migrationBuilder.DropColumn(
                name: "SheetSubmissionId",
                table: "SheetSectionRevisions");

            migrationBuilder.DropColumn(
                name: "SheetSubmissionId",
                table: "SheetRowRevisions");

            migrationBuilder.DropColumn(
                name: "SheetSubmissionId",
                table: "SheetColumnBlockRevisions");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "Publish a sheet's drafts as its next version.");
        }
    }
}
