using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateColumnBlocks_StickyColumnCount",
                table: "TemplateColumnBlocks",
                sql: "[StickyColumnCount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TableTemplateVersions_StickyColumnCount",
                table: "TableTemplateVersions",
                sql: "[StickyColumnCount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetVersions_VersionNumber",
                table: "SheetVersions",
                sql: "[VersionNumber] >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetTableRevisions_RevisionNumber",
                table: "SheetTableRevisions",
                sql: "[RevisionNumber] >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetTableRevisions_Status",
                table: "SheetTableRevisions",
                sql: "[Status] IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetSectionRevisions_RevisionNumber",
                table: "SheetSectionRevisions",
                sql: "[RevisionNumber] >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetSectionRevisions_Status",
                table: "SheetSectionRevisions",
                sql: "[Status] IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetRowRevisions_RevisionNumber",
                table: "SheetRowRevisions",
                sql: "[RevisionNumber] >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetRowRevisions_Status",
                table: "SheetRowRevisions",
                sql: "[Status] IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetColumnBlockRevisions_RevisionNumber",
                table: "SheetColumnBlockRevisions",
                sql: "[RevisionNumber] >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SheetColumnBlockRevisions_Status",
                table: "SheetColumnBlockRevisions",
                sql: "[Status] IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Phases_NotItsOwnParent",
                table: "Phases",
                sql: "[ParentPhaseId] <> [Id]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateColumnBlocks_StickyColumnCount",
                table: "TemplateColumnBlocks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TableTemplateVersions_StickyColumnCount",
                table: "TableTemplateVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetVersions_VersionNumber",
                table: "SheetVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetTableRevisions_RevisionNumber",
                table: "SheetTableRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetTableRevisions_Status",
                table: "SheetTableRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetSectionRevisions_RevisionNumber",
                table: "SheetSectionRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetSectionRevisions_Status",
                table: "SheetSectionRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetRowRevisions_RevisionNumber",
                table: "SheetRowRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetRowRevisions_Status",
                table: "SheetRowRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetColumnBlockRevisions_RevisionNumber",
                table: "SheetColumnBlockRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SheetColumnBlockRevisions_Status",
                table: "SheetColumnBlockRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Phases_NotItsOwnParent",
                table: "Phases");
        }
    }
}
