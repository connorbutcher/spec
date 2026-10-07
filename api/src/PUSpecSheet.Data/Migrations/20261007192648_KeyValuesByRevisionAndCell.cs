using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PUSpecSheet.Data.Migrations
{
    /// <inheritdoc />
    public partial class KeyValuesByRevisionAndCell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TextValues",
                schema: "values",
                table: "TextValues");

            migrationBuilder.DropIndex(
                name: "IX_TextValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "TextValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OptionValues",
                schema: "values",
                table: "OptionValues");

            migrationBuilder.DropIndex(
                name: "IX_OptionValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "OptionValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NumericValues",
                schema: "values",
                table: "NumericValues");

            migrationBuilder.DropIndex(
                name: "IX_NumericValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "NumericValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DateValues",
                schema: "values",
                table: "DateValues");

            migrationBuilder.DropIndex(
                name: "IX_DateValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "DateValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BooleanValues",
                schema: "values",
                table: "BooleanValues");

            migrationBuilder.DropIndex(
                name: "IX_BooleanValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "BooleanValues");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "values",
                table: "TextValues");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "values",
                table: "OptionValues");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "values",
                table: "NumericValues");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "values",
                table: "DateValues");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "values",
                table: "BooleanValues");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TextValues",
                schema: "values",
                table: "TextValues",
                columns: new[] { "SheetRowRevisionId", "SheetCellId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_OptionValues",
                schema: "values",
                table: "OptionValues",
                columns: new[] { "SheetRowRevisionId", "SheetCellId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_NumericValues",
                schema: "values",
                table: "NumericValues",
                columns: new[] { "SheetRowRevisionId", "SheetCellId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DateValues",
                schema: "values",
                table: "DateValues",
                columns: new[] { "SheetRowRevisionId", "SheetCellId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_BooleanValues",
                schema: "values",
                table: "BooleanValues",
                columns: new[] { "SheetRowRevisionId", "SheetCellId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TextValues",
                schema: "values",
                table: "TextValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OptionValues",
                schema: "values",
                table: "OptionValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NumericValues",
                schema: "values",
                table: "NumericValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DateValues",
                schema: "values",
                table: "DateValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BooleanValues",
                schema: "values",
                table: "BooleanValues");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "values",
                table: "TextValues",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "values",
                table: "OptionValues",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "values",
                table: "NumericValues",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "values",
                table: "DateValues",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "values",
                table: "BooleanValues",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TextValues",
                schema: "values",
                table: "TextValues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OptionValues",
                schema: "values",
                table: "OptionValues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NumericValues",
                schema: "values",
                table: "NumericValues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DateValues",
                schema: "values",
                table: "DateValues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BooleanValues",
                schema: "values",
                table: "BooleanValues",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TextValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "TextValues",
                columns: new[] { "SheetRowRevisionId", "SheetCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "OptionValues",
                columns: new[] { "SheetRowRevisionId", "SheetCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumericValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "NumericValues",
                columns: new[] { "SheetRowRevisionId", "SheetCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DateValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "DateValues",
                columns: new[] { "SheetRowRevisionId", "SheetCellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BooleanValues_SheetRowRevisionId_SheetCellId",
                schema: "values",
                table: "BooleanValues",
                columns: new[] { "SheetRowRevisionId", "SheetCellId" },
                unique: true);
        }
    }
}
