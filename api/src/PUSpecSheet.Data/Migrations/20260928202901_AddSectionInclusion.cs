using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionInclusion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing sections keep today's behaviour: added with the table and removable.
            migrationBuilder.AddColumn<string>(
                name: "Inclusion",
                table: "TemplateSections",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Default");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inclusion",
                table: "TemplateSections");
        }
    }
}
