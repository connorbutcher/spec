using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <summary>
    /// Every table version has exactly one header section; every other section is an addable section.
    /// The role values Fixed and Repeating become Header and Addable: in each version the first
    /// top-level fixed section becomes the header (always one copy), a version without a fixed section
    /// gets a new header at the top, and all other sections become addable with their counts kept.
    /// </summary>
    public partial class AddSectionHeaderAndAddableRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateSections_Instances",
                table: "TemplateSections");

            // Sections inside another section are addable sections, so they're no longer fixed.
            migrationBuilder.Sql(
                "UPDATE [TemplateSections] SET [Role] = 'Addable' "
                + "WHERE [ParentSectionId] IS NOT NULL AND [Role] = 'Fixed';");

            // The first top-level fixed section of each version is its header.
            migrationBuilder.Sql(
                "UPDATE s SET s.[Role] = 'Header', s.[MinInstances] = 1, s.[MaxInstances] = 1, s.[InitialInstances] = 1 "
                + "FROM [TemplateSections] s "
                + "WHERE s.[Role] = 'Fixed' AND s.[ParentSectionId] IS NULL AND s.[Id] = ("
                + "SELECT TOP 1 h.[Id] FROM [TemplateSections] h "
                + "WHERE h.[TableTemplateVersionId] = s.[TableTemplateVersionId] "
                + "AND h.[ParentSectionId] IS NULL AND h.[Role] = 'Fixed' "
                + "ORDER BY h.[DisplayOrder], h.[Id]);");

            // Every other section becomes addable, keeping its instance counts.
            migrationBuilder.Sql(
                "UPDATE [TemplateSections] SET [Role] = 'Addable' WHERE [Role] IN ('Fixed', 'Repeating');");

            // A version without a header gets one at the top, after the others shift down a place.
            migrationBuilder.Sql(
                "UPDATE s SET s.[DisplayOrder] = s.[DisplayOrder] + 1 FROM [TemplateSections] s "
                + "WHERE s.[ParentSectionId] IS NULL AND NOT EXISTS ("
                + "SELECT 1 FROM [TemplateSections] h "
                + "WHERE h.[TableTemplateVersionId] = s.[TableTemplateVersionId] AND h.[Role] = 'Header');");

            migrationBuilder.Sql(
                "INSERT INTO [TemplateSections] "
                + "([TableTemplateVersionId], [ParentSectionId], [Name], [DisplayOrder], [Role], "
                + "[MinInstances], [MaxInstances], [InitialInstances]) "
                + "SELECT v.[Id], NULL, N'Header', 1, 'Header', 1, 1, 1 FROM [TableTemplateVersions] v "
                + "WHERE NOT EXISTS (SELECT 1 FROM [TemplateSections] h "
                + "WHERE h.[TableTemplateVersionId] = v.[Id] AND h.[Role] = 'Header');");

            migrationBuilder.CreateIndex(
                name: "UX_TemplateSections_OneHeaderPerVersion",
                table: "TemplateSections",
                column: "TableTemplateVersionId",
                unique: true,
                filter: "[Role] = 'Header'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateSections_Header",
                table: "TemplateSections",
                sql: "[Role] <> 'Header' OR ([ParentSectionId] IS NULL AND [MinInstances] = 1 AND [MaxInstances] = 1 AND [InitialInstances] = 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateSections_Instances",
                table: "TemplateSections",
                sql: "[MinInstances] >= 0 AND [InitialInstances] >= [MinInstances] AND ([MaxInstances] IS NULL OR ([MaxInstances] >= 1 AND [MaxInstances] >= [InitialInstances]))");
        }

        /// <inheritdoc />
        /// <remarks>
        /// Headers go back to fixed sections and addable sections to repeating ones. A header that was
        /// added by this migration stays as a fixed section.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_TemplateSections_OneHeaderPerVersion",
                table: "TemplateSections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateSections_Header",
                table: "TemplateSections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateSections_Instances",
                table: "TemplateSections");

            migrationBuilder.Sql("UPDATE [TemplateSections] SET [Role] = 'Fixed' WHERE [Role] = 'Header';");
            migrationBuilder.Sql("UPDATE [TemplateSections] SET [Role] = 'Repeating' WHERE [Role] = 'Addable';");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateSections_Instances",
                table: "TemplateSections",
                sql: "[MinInstances] >= 0 AND [InitialInstances] >= [MinInstances] AND ([MaxInstances] IS NULL OR ([MaxInstances] >= 1 AND [MaxInstances] >= [InitialInstances])) AND ([Role] <> 'Fixed' OR [MaxInstances] = 1)");
        }
    }
}
