using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class HardenSheetVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SheetTableRevisions_Published",
                table: "SheetTableRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetSectionRevisions_Published",
                table: "SheetSectionRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetRowRevisions_Published",
                table: "SheetRowRevisions");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SheetTableRevisions",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupersededAtUtc",
                table: "SheetTableRevisions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SheetSectionRevisions",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupersededAtUtc",
                table: "SheetSectionRevisions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Sheets",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SheetRowRevisions",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupersededAtUtc",
                table: "SheetRowRevisions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetTableRevisions_Published",
                table: "SheetTableRevisions",
                columns: new[] { "SheetTableId", "PublishedAtUtc" },
                filter: "[Status] = 1")
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_SheetTableRevisions_OneCurrentPerTable",
                table: "SheetTableRevisions",
                column: "SheetTableId",
                unique: true,
                filter: "[Status] = 1 AND [SupersededAtUtc] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetTableRevisions_SupersededAt",
                table: "SheetTableRevisions",
                sql: "[SupersededAtUtc] IS NULL OR ([Status] = 1 AND [SupersededAtUtc] >= [PublishedAtUtc])");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSectionRevisions_Published",
                table: "SheetSectionRevisions",
                columns: new[] { "SheetSectionId", "PublishedAtUtc" },
                filter: "[Status] = 1")
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_SheetSectionRevisions_OneCurrentPerSection",
                table: "SheetSectionRevisions",
                column: "SheetSectionId",
                unique: true,
                filter: "[Status] = 1 AND [SupersededAtUtc] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetSectionRevisions_SupersededAt",
                table: "SheetSectionRevisions",
                sql: "[SupersededAtUtc] IS NULL OR ([Status] = 1 AND [SupersededAtUtc] >= [PublishedAtUtc])");

            migrationBuilder.CreateIndex(
                name: "IX_SheetRowRevisions_Published",
                table: "SheetRowRevisions",
                columns: new[] { "SheetRowId", "PublishedAtUtc" },
                filter: "[Status] = 1")
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_SheetRowRevisions_OneCurrentPerRow",
                table: "SheetRowRevisions",
                column: "SheetRowId",
                unique: true,
                filter: "[Status] = 1 AND [SupersededAtUtc] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetRowRevisions_SupersededAt",
                table: "SheetRowRevisions",
                sql: "[SupersededAtUtc] IS NULL OR ([Status] = 1 AND [SupersededAtUtc] >= [PublishedAtUtc])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SheetTableRevisions_Published",
                table: "SheetTableRevisions");

            migrationBuilder.DropIndex(
                name: "UX_SheetTableRevisions_OneCurrentPerTable",
                table: "SheetTableRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetTableRevisions_SupersededAt",
                table: "SheetTableRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetSectionRevisions_Published",
                table: "SheetSectionRevisions");

            migrationBuilder.DropIndex(
                name: "UX_SheetSectionRevisions_OneCurrentPerSection",
                table: "SheetSectionRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetSectionRevisions_SupersededAt",
                table: "SheetSectionRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetRowRevisions_Published",
                table: "SheetRowRevisions");

            migrationBuilder.DropIndex(
                name: "UX_SheetRowRevisions_OneCurrentPerRow",
                table: "SheetRowRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetRowRevisions_SupersededAt",
                table: "SheetRowRevisions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SheetTableRevisions");

            migrationBuilder.DropColumn(
                name: "SupersededAtUtc",
                table: "SheetTableRevisions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SheetSectionRevisions");

            migrationBuilder.DropColumn(
                name: "SupersededAtUtc",
                table: "SheetSectionRevisions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SheetRowRevisions");

            migrationBuilder.DropColumn(
                name: "SupersededAtUtc",
                table: "SheetRowRevisions");

            migrationBuilder.CreateIndex(
                name: "IX_SheetTableRevisions_Published",
                table: "SheetTableRevisions",
                columns: new[] { "SheetTableId", "PublishedAtUtc" },
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSectionRevisions_Published",
                table: "SheetSectionRevisions",
                columns: new[] { "SheetSectionId", "PublishedAtUtc" },
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SheetRowRevisions_Published",
                table: "SheetRowRevisions",
                columns: new[] { "SheetRowId", "PublishedAtUtc" },
                filter: "[Status] = 1");
        }
    }
}
