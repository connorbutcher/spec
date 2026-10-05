namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>Every tag in the API documentation, grouped and ordered as the sidebar shows them.</summary>
public static class ApiTagCatalog
{
    public static IReadOnlyList<ApiTagGroup> Groups { get; } =
    [
        new ApiTagGroup(
            "For other applications",
            [
                new ApiTagDescription(
                    ApiTags.PublishedSheets,
                    "Read-only published values for other applications. Nothing here returns drafts, locks or templates, "
                    + "and nothing depends on who is asking.\n\n"
                    + "A sheet is addressed by its public identifier, which never changes. Find it once from a phase code "
                    + "and sheet type, then read it at a version number, at the newest version, or as it stood at a moment. "
                    + "Tables, sections, rows and cells also keep one identifier through every version, so a caller can "
                    + "store the identifiers of the cells it needs and ask for just those.\n\n"
                    + "Every read returns an `ETag`. Send it back in `If-None-Match` to get `304 Not Modified` when the "
                    + "answer is unchanged. A published version never changes, so a read of a version number can be kept "
                    + "for good."),
            ]),
        new ApiTagGroup(
            "Sheets",
            [
                new ApiTagDescription(
                    ApiTags.Sheets,
                    "Opening, publishing and discarding sheets. A sheet is shown live (the latest published state with "
                    + "your own drafts on top), as of a version number, or as of a date and time. Past views are read-only."),
                new ApiTagDescription(
                    ApiTags.SheetTables,
                    "Changes to one table on a sheet. Every change is a draft until the sheet is published, and returns "
                    + "the refreshed live view of the sheet."),
                new ApiTagDescription(
                    ApiTags.SheetSections,
                    "Changes to one section copy on a sheet table, and adding rows to it. Returns the refreshed live view of the sheet."),
                new ApiTagDescription(
                    ApiTags.SheetRows,
                    "Changes to one row. Changing a row starts your draft on it, which locks it to you until you publish "
                    + "or discard. Returns the refreshed live view of the sheet."),
                new ApiTagDescription(
                    ApiTags.SheetColumnBlocks,
                    "Changes to the column block copies on a horizontal sheet table. Returns the refreshed live view of the sheet."),
                new ApiTagDescription(
                    ApiTags.SheetItems,
                    "Finds any sheet, table, section, column block, row or cell from its public identifier."),
            ]),
        new ApiTagGroup(
            "Templates",
            [
                new ApiTagDescription(
                    ApiTags.TableTemplates,
                    "The table templates of a sheet type and their versions. A version that a sheet uses can no longer "
                    + "be edited; create a new version instead."),
                new ApiTagDescription(
                    ApiTags.TemplateSections,
                    "Sections of a template version. Every change returns the whole updated template."),
                new ApiTagDescription(
                    ApiTags.TemplateRows,
                    "Rows of a template section. Every change returns the whole updated template."),
                new ApiTagDescription(
                    ApiTags.TemplateCells,
                    "Cells of a template row, and what each changes from its cell type's defaults. Every change returns "
                    + "the whole updated template."),
                new ApiTagDescription(
                    ApiTags.TemplateColumnBlocks,
                    "Column blocks of a horizontal template version. Every change returns the whole updated template."),
            ]),
        new ApiTagGroup(
            "Reference data",
            [
                new ApiTagDescription(
                    ApiTags.Phases,
                    "The phases sheets belong to, as a flat list that forms a tree, and which sheet types each phase has."),
                new ApiTagDescription(ApiTags.SheetTypes, "The kinds of sheet a phase can have, such as Specification, PFKs or Parts."),
                new ApiTagDescription(
                    ApiTags.CellTypes,
                    "The cell types templates build their cells from: a kind (text, number, date, checkbox, dropdown) "
                    + "with a default configuration and style."),
            ]),
        new ApiTagGroup(
            "Access",
            [
                new ApiTagDescription(
                    ApiTags.CurrentUser,
                    "Who a request runs as and what their roles allow. Changes that need a permission say which in "
                    + "their description, and answer `403` to a user without it. The Administrator role has every permission."),
            ]),
        new ApiTagGroup(
            "Operations",
            [
                new ApiTagDescription(ApiTags.Health, "Whether the API and its database are reachable."),
            ]),
    ];
}
