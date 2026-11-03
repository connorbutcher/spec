using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSheetIdToRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SheetId",
                table: "SheetTableRevisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SheetId",
                table: "SheetSectionRevisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SheetId",
                table: "SheetRowRevisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SheetId",
                table: "SheetColumnBlockRevisions",
                type: "int",
                nullable: true);

            // Each revision takes the sheet its item is on, through the item's parents. The column is added
            // empty, filled, then made required, so no default is left behind on the table.
            migrationBuilder.Sql(
                """
                UPDATE revision SET SheetId = sheetTable.SheetId
                FROM dbo.SheetTableRevisions AS revision
                INNER JOIN dbo.SheetTables AS sheetTable ON sheetTable.Id = revision.SheetTableId;

                UPDATE revision SET SheetId = sheetTable.SheetId
                FROM dbo.SheetSectionRevisions AS revision
                INNER JOIN dbo.SheetSections AS section ON section.Id = revision.SheetSectionId
                INNER JOIN dbo.SheetTables AS sheetTable ON sheetTable.Id = section.SheetTableId;

                UPDATE revision SET SheetId = sheetTable.SheetId
                FROM dbo.SheetRowRevisions AS revision
                INNER JOIN dbo.SheetRows AS sheetRow ON sheetRow.Id = revision.SheetRowId
                INNER JOIN dbo.SheetSections AS section ON section.Id = sheetRow.SheetSectionId
                INNER JOIN dbo.SheetTables AS sheetTable ON sheetTable.Id = section.SheetTableId;

                UPDATE revision SET SheetId = sheetTable.SheetId
                FROM dbo.SheetColumnBlockRevisions AS revision
                INNER JOIN dbo.SheetColumnBlocks AS block ON block.Id = revision.SheetColumnBlockId
                INNER JOIN dbo.SheetTables AS sheetTable ON sheetTable.Id = block.SheetTableId;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "SheetId",
                table: "SheetTableRevisions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SheetId",
                table: "SheetSectionRevisions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SheetId",
                table: "SheetRowRevisions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SheetId",
                table: "SheetColumnBlockRevisions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_SheetTableRevisions_SheetId_Status",
                table: "SheetTableRevisions",
                columns: new[] { "SheetId", "Status" })
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc", "SheetTableId", "AuthorUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetTableRevisions_SheetPublished",
                table: "SheetTableRevisions",
                columns: new[] { "SheetId", "PublishedAtUtc" },
                filter: "[Status] = 1")
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc", "IsDeleted", "DisplayOrder", "SheetTableId" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetSectionRevisions_SheetId_Status",
                table: "SheetSectionRevisions",
                columns: new[] { "SheetId", "Status" })
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc", "SheetSectionId", "AuthorUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetSectionRevisions_SheetPublished",
                table: "SheetSectionRevisions",
                columns: new[] { "SheetId", "PublishedAtUtc" },
                filter: "[Status] = 1")
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc", "IsDeleted", "DisplayOrder", "SheetSectionId" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetRowRevisions_SheetId_Status",
                table: "SheetRowRevisions",
                columns: new[] { "SheetId", "Status" })
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc", "SheetRowId", "AuthorUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetRowRevisions_SheetPublished",
                table: "SheetRowRevisions",
                columns: new[] { "SheetId", "PublishedAtUtc" },
                filter: "[Status] = 1")
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc", "IsDeleted", "DisplayOrder", "SheetRowId" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetColumnBlockRevisions_SheetId_Status",
                table: "SheetColumnBlockRevisions",
                columns: new[] { "SheetId", "Status" })
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc", "SheetColumnBlockId", "AuthorUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetColumnBlockRevisions_SheetPublished",
                table: "SheetColumnBlockRevisions",
                columns: new[] { "SheetId", "PublishedAtUtc" },
                filter: "[Status] = 1")
                .Annotation("SqlServer:Include", new[] { "SupersededAtUtc", "IsDeleted", "DisplayOrder", "SheetColumnBlockId" });

            migrationBuilder.AddForeignKey(
                name: "FK_SheetColumnBlockRevisions_Sheets_SheetId",
                table: "SheetColumnBlockRevisions",
                column: "SheetId",
                principalTable: "Sheets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SheetRowRevisions_Sheets_SheetId",
                table: "SheetRowRevisions",
                column: "SheetId",
                principalTable: "Sheets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SheetSectionRevisions_Sheets_SheetId",
                table: "SheetSectionRevisions",
                column: "SheetId",
                principalTable: "Sheets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SheetTableRevisions_Sheets_SheetId",
                table: "SheetTableRevisions",
                column: "SheetId",
                principalTable: "Sheets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SheetColumnBlockRevisions_Sheets_SheetId",
                table: "SheetColumnBlockRevisions");

            migrationBuilder.DropForeignKey(
                name: "FK_SheetRowRevisions_Sheets_SheetId",
                table: "SheetRowRevisions");

            migrationBuilder.DropForeignKey(
                name: "FK_SheetSectionRevisions_Sheets_SheetId",
                table: "SheetSectionRevisions");

            migrationBuilder.DropForeignKey(
                name: "FK_SheetTableRevisions_Sheets_SheetId",
                table: "SheetTableRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetTableRevisions_SheetId_Status",
                table: "SheetTableRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetTableRevisions_SheetPublished",
                table: "SheetTableRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetSectionRevisions_SheetId_Status",
                table: "SheetSectionRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetSectionRevisions_SheetPublished",
                table: "SheetSectionRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetRowRevisions_SheetId_Status",
                table: "SheetRowRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetRowRevisions_SheetPublished",
                table: "SheetRowRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetColumnBlockRevisions_SheetId_Status",
                table: "SheetColumnBlockRevisions");

            migrationBuilder.DropIndex(
                name: "IX_SheetColumnBlockRevisions_SheetPublished",
                table: "SheetColumnBlockRevisions");

            migrationBuilder.DropColumn(
                name: "SheetId",
                table: "SheetTableRevisions");

            migrationBuilder.DropColumn(
                name: "SheetId",
                table: "SheetSectionRevisions");

            migrationBuilder.DropColumn(
                name: "SheetId",
                table: "SheetRowRevisions");

            migrationBuilder.DropColumn(
                name: "SheetId",
                table: "SheetColumnBlockRevisions");
        }
    }
}
