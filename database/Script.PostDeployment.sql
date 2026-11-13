/*
    Reference data every PUSpecSheet database starts with. Runs after every deployment of this project, so each
    block only adds what is missing and never overwrites rows people may have edited since (cell types, sheet types).
    Permissions are the exception: they are defined by the code (PUSpecSheet.Domain.Users.PermissionKeys), so their
    keys and descriptions are kept in step with it on every deployment. One row per PermissionKeys constant.
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Sheet types --------------------------------------------------------------------------------------------------------

SET IDENTITY_INSERT [setup].[SheetTypes] ON;

INSERT INTO [setup].[SheetTypes] ([Id], [Name], [DisplayOrder])
SELECT [source].[Id], [source].[Name], [source].[DisplayOrder]
FROM (VALUES
    (1, N'Specification', 1),
    (2, N'Parts', 2),
    (3, N'PFKs', 3),
    (4, N'Engine Specifications', 4),
    (5, N'Confirmation', 5),
    (6, N'Torque Sheet', 6)
) AS [source] ([Id], [Name], [DisplayOrder])
WHERE NOT EXISTS (SELECT 1 FROM [setup].[SheetTypes] AS [target] WHERE [target].[Id] = [source].[Id]);

SET IDENTITY_INSERT [setup].[SheetTypes] OFF;

-- Cell types: one per kind plus a sample dropdown. Users can edit or add to these. ------------------------------------

SET IDENTITY_INSERT [template].[CellTypes] ON;

INSERT INTO [template].[CellTypes] ([Id], [Name], [Kind], [Description], [DisplayOrder], [Configuration], [Style])
SELECT [source].[Id], [source].[Name], [source].[Kind], [source].[Description], [source].[DisplayOrder], [source].[Configuration], [source].[Style]
FROM (VALUES
    (1, N'Heading', N'Heading', N'Fixed text such as a column header.', 1, N'{"kind":"Heading"}', N'{"bold":true}'),
    (2, N'Text', N'Text', NULL, 2, N'{"kind":"Text","maxLength":200}', N'{}'),
    (3, N'Number', N'Number', NULL, 3, N'{"kind":"Number","decimalPlaces":2}', N'{"align":"Right"}'),
    (4, N'Date', N'Date', NULL, 4, N'{"kind":"Date"}', N'{}'),
    (5, N'Checkbox', N'Checkbox', NULL, 5, N'{"kind":"Checkbox"}', N'{"align":"Center"}'),
    (6, N'Pass / Fail', N'TextDropdown', NULL, 6, N'{"kind":"TextDropdown"}', N'{}'),
    (7, N'Group', N'Group', N'A caption that groups the cells around it.', 7, N'{"kind":"Group"}', N'{"bold":true,"backgroundColor":"#f1f5f9"}')
) AS [source] ([Id], [Name], [Kind], [Description], [DisplayOrder], [Configuration], [Style])
WHERE NOT EXISTS (SELECT 1 FROM [template].[CellTypes] AS [target] WHERE [target].[Id] = [source].[Id]);

SET IDENTITY_INSERT [template].[CellTypes] OFF;

SET IDENTITY_INSERT [template].[CellTypeOptions] ON;

INSERT INTO [template].[CellTypeOptions] ([Id], [CellTypeId], [Value], [DisplayOrder])
SELECT [source].[Id], [source].[CellTypeId], [source].[Value], [source].[DisplayOrder]
FROM (VALUES
    (1, 6, N'Pass', 1),
    (2, 6, N'Fail', 2),
    (3, 6, N'N/A', 3)
) AS [source] ([Id], [CellTypeId], [Value], [DisplayOrder])
WHERE NOT EXISTS (SELECT 1 FROM [template].[CellTypeOptions] AS [target] WHERE [target].[Id] = [source].[Id])
    AND EXISTS (SELECT 1 FROM [template].[CellTypes] AS [cellType] WHERE [cellType].[Id] = [source].[CellTypeId]);

SET IDENTITY_INSERT [template].[CellTypeOptions] OFF;

-- Permissions: one row per key in PermissionKeys ---------------------------------------------------------------------

SET IDENTITY_INSERT [auth].[Permissions] ON;

MERGE INTO [auth].[Permissions] AS [target]
USING (VALUES
    (1, N'phases.manage', N'Add phases and choose which sheet types each phase has.'),
    (2, N'sheetTypes.manage', N'Add sheet types.'),
    (3, N'templates.manage', N'Create, change and delete table templates and everything in them.'),
    (4, N'cellTypes.manage', N'Create, change and delete cell types.'),
    (5, N'sheets.edit', N'Change a sheet: its tables, sections, rows, column blocks and cell values.'),
    (6, N'sheets.publish', N'Publish a sheet''s drafts as its next version, once the chosen approvers approve.'),
    (7, N'sheets.approve', N'Approve or reject the changes someone else submitted for publishing.')
) AS [source] ([Id], [Key], [Description])
ON [target].[Id] = [source].[Id]
WHEN MATCHED AND ([target].[Key] <> [source].[Key] OR [target].[Description] <> [source].[Description]) THEN
    UPDATE SET [Key] = [source].[Key], [Description] = [source].[Description]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Key], [Description]) VALUES ([source].[Id], [source].[Key], [source].[Description]);

SET IDENTITY_INSERT [auth].[Permissions] OFF;

-- Roles: the administrator role has no rows in RolePermissions, holding it implies every permission ------------------

SET IDENTITY_INSERT [auth].[Roles] ON;

INSERT INTO [auth].[Roles] ([Id], [Name], [Description])
SELECT 1, N'Administrator', N'Can do everything. Every permission is implied, including ones added later.'
WHERE NOT EXISTS (SELECT 1 FROM [auth].[Roles] WHERE [Id] = 1);

SET IDENTITY_INSERT [auth].[Roles] OFF;

-- Users: everything runs as the developer user, an administrator, until sign-in is added ---------------------------

SET IDENTITY_INSERT [auth].[Users] ON;

INSERT INTO [auth].[Users] ([Id], [UserName], [DisplayName], [IsActive], [CreatedAtUtc])
SELECT 1, N'developer', N'Developer', 1, '2026-09-28T00:00:00'
WHERE NOT EXISTS (SELECT 1 FROM [auth].[Users] WHERE [Id] = 1);

SET IDENTITY_INSERT [auth].[Users] OFF;

INSERT INTO [auth].[UserRoles] ([UserId], [RoleId])
SELECT 1, 1
WHERE NOT EXISTS (SELECT 1 FROM [auth].[UserRoles] WHERE [UserId] = 1 AND [RoleId] = 1);

COMMIT TRANSACTION;
GO

-- The cell-value tables used to live in a [values] schema; once they have moved to [cell], drop it if it is empty.
IF SCHEMA_ID(N'values') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.objects WHERE [schema_id] = SCHEMA_ID(N'values'))
BEGIN
    EXEC (N'DROP SCHEMA [values]');
END
GO
