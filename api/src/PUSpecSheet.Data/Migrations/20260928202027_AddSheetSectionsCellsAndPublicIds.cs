using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSheetSectionsCellsAndPublicIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BooleanValues_TemplateCells_TemplateCellId",
                schema: "values",
                table: "BooleanValues");

            migrationBuilder.DropForeignKey(
                name: "FK_DateValues_TemplateCells_TemplateCellId",
                schema: "values",
                table: "DateValues");

            migrationBuilder.DropForeignKey(
                name: "FK_NumericValues_TemplateCells_TemplateCellId",
                schema: "values",
                table: "NumericValues");

            migrationBuilder.DropForeignKey(
                name: "FK_OptionValues_TemplateCells_TemplateCellId",
                schema: "values",
                table: "OptionValues");

            migrationBuilder.DropForeignKey(
                name: "FK_SheetRows_SheetTables_SheetTableId",
                table: "SheetRows");

            migrationBuilder.DropForeignKey(
                name: "FK_TextValues_TemplateCells_TemplateCellId",
                schema: "values",
                table: "TextValues");

            migrationBuilder.DropIndex(
                name: "IX_SheetTables_SheetId_DisplayOrder",
                table: "SheetTables");

            migrationBuilder.DropIndex(
                name: "IX_SheetRows_SheetTableId_DisplayOrder",
                table: "SheetRows");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SheetTables");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SheetRows");

            migrationBuilder.RenameColumn(
                name: "TemplateCellId",
                schema: "values",
                table: "TextValues",
                newName: "SheetCellId");

            migrationBuilder.RenameIndex(
                name: "IX_TextValues_TemplateCellId",
                schema: "values",
                table: "TextValues",
                newName: "IX_TextValues_SheetCellId");

            migrationBuilder.RenameIndex(
                name: "IX_TextValues_SheetRowRevisionId_TemplateCellId",
                schema: "values",
                table: "TextValues",
                newName: "IX_TextValues_SheetRowRevisionId_SheetCellId");

            migrationBuilder.RenameColumn(
                name: "SheetTableId",
                table: "SheetRows",
                newName: "SheetSectionId");

            migrationBuilder.RenameColumn(
                name: "TemplateCellId",
                schema: "values",
                table: "OptionValues",
                newName: "SheetCellId");

            migrationBuilder.RenameIndex(
                name: "IX_OptionValues_TemplateCellId",
                schema: "values",
                table: "OptionValues",
                newName: "IX_OptionValues_SheetCellId");

            migrationBuilder.RenameIndex(
                name: "IX_OptionValues_SheetRowRevisionId_TemplateCellId",
                schema: "values",
                table: "OptionValues",
                newName: "IX_OptionValues_SheetRowRevisionId_SheetCellId");

            migrationBuilder.RenameColumn(
                name: "TemplateCellId",
                schema: "values",
                table: "NumericValues",
                newName: "SheetCellId");

            migrationBuilder.RenameIndex(
                name: "IX_NumericValues_TemplateCellId",
                schema: "values",
                table: "NumericValues",
                newName: "IX_NumericValues_SheetCellId");

            migrationBuilder.RenameIndex(
                name: "IX_NumericValues_SheetRowRevisionId_TemplateCellId",
                schema: "values",
                table: "NumericValues",
                newName: "IX_NumericValues_SheetRowRevisionId_SheetCellId");

            migrationBuilder.RenameColumn(
                name: "TemplateCellId",
                schema: "values",
                table: "DateValues",
                newName: "SheetCellId");

            migrationBuilder.RenameIndex(
                name: "IX_DateValues_TemplateCellId",
                schema: "values",
                table: "DateValues",
                newName: "IX_DateValues_SheetCellId");

            migrationBuilder.RenameIndex(
                name: "IX_DateValues_SheetRowRevisionId_TemplateCellId",
                schema: "values",
                table: "DateValues",
                newName: "IX_DateValues_SheetRowRevisionId_SheetCellId");

            migrationBuilder.RenameColumn(
                name: "TemplateCellId",
                schema: "values",
                table: "BooleanValues",
                newName: "SheetCellId");

            migrationBuilder.RenameIndex(
                name: "IX_BooleanValues_TemplateCellId",
                schema: "values",
                table: "BooleanValues",
                newName: "IX_BooleanValues_SheetCellId");

            migrationBuilder.RenameIndex(
                name: "IX_BooleanValues_SheetRowRevisionId_TemplateCellId",
                schema: "values",
                table: "BooleanValues",
                newName: "IX_BooleanValues_SheetRowRevisionId_SheetCellId");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "SheetTables",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SheetTableRevisions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Sheets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "SheetRows",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SheetRowRevisions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SheetCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SheetRowId = table.Column<int>(type: "int", nullable: false),
                    TemplateCellId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetCells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SheetCells_SheetRows_SheetRowId",
                        column: x => x.SheetRowId,
                        principalTable: "SheetRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetCells_TemplateCells_TemplateCellId",
                        column: x => x.TemplateCellId,
                        principalTable: "TemplateCells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SheetSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SheetTableId = table.Column<int>(type: "int", nullable: false),
                    TemplateSectionId = table.Column<int>(type: "int", nullable: false),
                    ParentSheetSectionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SheetSections_SheetSections_ParentSheetSectionId",
                        column: x => x.ParentSheetSectionId,
                        principalTable: "SheetSections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SheetSections_SheetTables_SheetTableId",
                        column: x => x.SheetTableId,
                        principalTable: "SheetTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetSections_TemplateSections_TemplateSectionId",
                        column: x => x.TemplateSectionId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SheetSectionRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetSectionId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AuthorUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SheetVersionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetSectionRevisions", x => x.Id);
                    table.CheckConstraint("CK_SheetSectionRevisions_PublishedAt", "([Status] = 0 AND [PublishedAtUtc] IS NULL) OR ([Status] = 1 AND [PublishedAtUtc] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_SheetSectionRevisions_SheetSections_SheetSectionId",
                        column: x => x.SheetSectionId,
                        principalTable: "SheetSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SheetSectionRevisions_SheetVersions_SheetVersionId",
                        column: x => x.SheetVersionId,
                        principalTable: "SheetVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SheetSectionRevisions_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SheetTables_PublicId",
                table: "SheetTables",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetTables_SheetId",
                table: "SheetTables",
                column: "SheetId");

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_PublicId",
                table: "Sheets",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetRows_PublicId",
                table: "SheetRows",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetRows_SheetSectionId",
                table: "SheetRows",
                column: "SheetSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetCells_PublicId",
                table: "SheetCells",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetCells_SheetRowId_TemplateCellId",
                table: "SheetCells",
                columns: new[] { "SheetRowId", "TemplateCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetCells_TemplateCellId",
                table: "SheetCells",
                column: "TemplateCellId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSectionRevisions_AuthorUserId_Status",
                table: "SheetSectionRevisions",
                columns: new[] { "AuthorUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetSectionRevisions_Published",
                table: "SheetSectionRevisions",
                columns: new[] { "SheetSectionId", "PublishedAtUtc" },
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSectionRevisions_SheetSectionId_RevisionNumber",
                table: "SheetSectionRevisions",
                columns: new[] { "SheetSectionId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetSectionRevisions_SheetVersionId",
                table: "SheetSectionRevisions",
                column: "SheetVersionId");

            migrationBuilder.CreateIndex(
                name: "UX_SheetSectionRevisions_OneDraftPerSection",
                table: "SheetSectionRevisions",
                column: "SheetSectionId",
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSections_ParentSheetSectionId",
                table: "SheetSections",
                column: "ParentSheetSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSections_PublicId",
                table: "SheetSections",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SheetSections_SheetTableId",
                table: "SheetSections",
                column: "SheetTableId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSections_TemplateSectionId",
                table: "SheetSections",
                column: "TemplateSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_BooleanValues_SheetCells_SheetCellId",
                schema: "values",
                table: "BooleanValues",
                column: "SheetCellId",
                principalTable: "SheetCells",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DateValues_SheetCells_SheetCellId",
                schema: "values",
                table: "DateValues",
                column: "SheetCellId",
                principalTable: "SheetCells",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NumericValues_SheetCells_SheetCellId",
                schema: "values",
                table: "NumericValues",
                column: "SheetCellId",
                principalTable: "SheetCells",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OptionValues_SheetCells_SheetCellId",
                schema: "values",
                table: "OptionValues",
                column: "SheetCellId",
                principalTable: "SheetCells",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SheetRows_SheetSections_SheetSectionId",
                table: "SheetRows",
                column: "SheetSectionId",
                principalTable: "SheetSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TextValues_SheetCells_SheetCellId",
                schema: "values",
                table: "TextValues",
                column: "SheetCellId",
                principalTable: "SheetCells",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BooleanValues_SheetCells_SheetCellId",
                schema: "values",
                table: "BooleanValues");

            migrationBuilder.DropForeignKey(
                name: "FK_DateValues_SheetCells_SheetCellId",
                schema: "values",
                table: "DateValues");

            migrationBuilder.DropForeignKey(
                name: "FK_NumericValues_SheetCells_SheetCellId",
                schema: "values",
                table: "NumericValues");

            migrationBuilder.DropForeignKey(
                name: "FK_OptionValues_SheetCells_SheetCellId",
                schema: "values",
                table: "OptionValues");

            migrationBuilder.DropForeignKey(
                name: "FK_SheetRows_SheetSections_SheetSectionId",
                table: "SheetRows");

            migrationBuilder.DropForeignKey(
                name: "FK_TextValues_SheetCells_SheetCellId",
                schema: "values",
                table: "TextValues");

            migrationBuilder.DropTable(
                name: "SheetCells");

            migrationBuilder.DropTable(
                name: "SheetSectionRevisions");

            migrationBuilder.DropTable(
                name: "SheetSections");

            migrationBuilder.DropIndex(
                name: "IX_SheetTables_PublicId",
                table: "SheetTables");

            migrationBuilder.DropIndex(
                name: "IX_SheetTables_SheetId",
                table: "SheetTables");

            migrationBuilder.DropIndex(
                name: "IX_Sheets_PublicId",
                table: "Sheets");

            migrationBuilder.DropIndex(
                name: "IX_SheetRows_PublicId",
                table: "SheetRows");

            migrationBuilder.DropIndex(
                name: "IX_SheetRows_SheetSectionId",
                table: "SheetRows");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "SheetTables");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SheetTableRevisions");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "SheetRows");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SheetRowRevisions");

            migrationBuilder.RenameColumn(
                name: "SheetCellId",
                schema: "values",
                table: "TextValues",
                newName: "TemplateCellId");

            migrationBuilder.RenameIndex(
                name: "IX_TextValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "TextValues",
                newName: "IX_TextValues_SheetRowRevisionId_TemplateCellId");

            migrationBuilder.RenameIndex(
                name: "IX_TextValues_SheetCellId",
                schema: "values",
                table: "TextValues",
                newName: "IX_TextValues_TemplateCellId");

            migrationBuilder.RenameColumn(
                name: "SheetSectionId",
                table: "SheetRows",
                newName: "SheetTableId");

            migrationBuilder.RenameColumn(
                name: "SheetCellId",
                schema: "values",
                table: "OptionValues",
                newName: "TemplateCellId");

            migrationBuilder.RenameIndex(
                name: "IX_OptionValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "OptionValues",
                newName: "IX_OptionValues_SheetRowRevisionId_TemplateCellId");

            migrationBuilder.RenameIndex(
                name: "IX_OptionValues_SheetCellId",
                schema: "values",
                table: "OptionValues",
                newName: "IX_OptionValues_TemplateCellId");

            migrationBuilder.RenameColumn(
                name: "SheetCellId",
                schema: "values",
                table: "NumericValues",
                newName: "TemplateCellId");

            migrationBuilder.RenameIndex(
                name: "IX_NumericValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "NumericValues",
                newName: "IX_NumericValues_SheetRowRevisionId_TemplateCellId");

            migrationBuilder.RenameIndex(
                name: "IX_NumericValues_SheetCellId",
                schema: "values",
                table: "NumericValues",
                newName: "IX_NumericValues_TemplateCellId");

            migrationBuilder.RenameColumn(
                name: "SheetCellId",
                schema: "values",
                table: "DateValues",
                newName: "TemplateCellId");

            migrationBuilder.RenameIndex(
                name: "IX_DateValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "DateValues",
                newName: "IX_DateValues_SheetRowRevisionId_TemplateCellId");

            migrationBuilder.RenameIndex(
                name: "IX_DateValues_SheetCellId",
                schema: "values",
                table: "DateValues",
                newName: "IX_DateValues_TemplateCellId");

            migrationBuilder.RenameColumn(
                name: "SheetCellId",
                schema: "values",
                table: "BooleanValues",
                newName: "TemplateCellId");

            migrationBuilder.RenameIndex(
                name: "IX_BooleanValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "BooleanValues",
                newName: "IX_BooleanValues_SheetRowRevisionId_TemplateCellId");

            migrationBuilder.RenameIndex(
                name: "IX_BooleanValues_SheetCellId",
                schema: "values",
                table: "BooleanValues",
                newName: "IX_BooleanValues_TemplateCellId");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SheetTables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SheetRows",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SheetTables_SheetId_DisplayOrder",
                table: "SheetTables",
                columns: new[] { "SheetId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SheetRows_SheetTableId_DisplayOrder",
                table: "SheetRows",
                columns: new[] { "SheetTableId", "DisplayOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_BooleanValues_TemplateCells_TemplateCellId",
                schema: "values",
                table: "BooleanValues",
                column: "TemplateCellId",
                principalTable: "TemplateCells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DateValues_TemplateCells_TemplateCellId",
                schema: "values",
                table: "DateValues",
                column: "TemplateCellId",
                principalTable: "TemplateCells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NumericValues_TemplateCells_TemplateCellId",
                schema: "values",
                table: "NumericValues",
                column: "TemplateCellId",
                principalTable: "TemplateCells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OptionValues_TemplateCells_TemplateCellId",
                schema: "values",
                table: "OptionValues",
                column: "TemplateCellId",
                principalTable: "TemplateCells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SheetRows_SheetTables_SheetTableId",
                table: "SheetRows",
                column: "SheetTableId",
                principalTable: "SheetTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TextValues_TemplateCells_TemplateCellId",
                schema: "values",
                table: "TextValues",
                column: "TemplateCellId",
                principalTable: "TemplateCells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
