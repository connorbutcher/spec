/*
    Seeds the reference data an empty database needs before the app can be used: one admin user with every
    permission, one phase, one sheet type with the default properties, every table cell type and widget type the UI
    knows, and one small table template ("Limits").

    Run it with seed.ps1, which passes the variables below. It is idempotent: each row is matched on its natural
    key (a permission's Key, a user's ObjectID, a phase's name, a type's name, a cell or widget type's key, a
    template's Type within its sheet type, a group's title within its template, a row's number within its group and
    a cell's column within its row) and only inserted when missing, so running it again changes nothing and it can
    be run against a database that already holds some of this data. Existing rows are never updated or deleted.

    It touches only the reference tables (access, types, template, configuration). Sheets, values and revisions
    stay empty.

    Variables (sqlcmd -v):
        AdminObjectID     The admin's Entra ID object id (the token's oid claim). The API finds the signed-in user
                          by it, so it must be the real one for the permissions to apply to you.
        AdminDisplayName  The admin's display name.
        AdminEmail        The admin's email address.

    Cell type keys, Type and the reference GUIDs come from the UI (app/src/app/core/models/domain/enums). ModelType
    decides which value table a cell's value goes in (RowRevisionValues.SetEmptyValue, CellValueHelpers): text,
    numeric, pair, linkedSpecSheet, textDropdown or numericDropdown. Those for the dropdown and lookup types are read
    from their UI components and properties, not from a legacy database, so check them against one when you can.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRANSACTION;

/* ---------------------------------------------------------------------------------------------------------------
   access: permissions and the admin user
--------------------------------------------------------------------------------------------------------------- */

DECLARE @Permissions TABLE ([Key] VARCHAR (64) NOT NULL, [Title] VARCHAR (256) NOT NULL, [Description] VARCHAR (512) NOT NULL);
INSERT INTO @Permissions ([Key], [Title], [Description])
VALUES
    ('admin.view', 'View admin', 'Open the admin area.'),
    ('admin.sheets', 'Manage sheets', 'Audit and manage sheets in the admin area.'),
    ('admin.types', 'Manage sheet types', 'Create and change sheet types, their properties and structure.'),
    ('admin.phases', 'Manage phases', 'Create and change phases.'),
    ('admin.users', 'Manage users', 'View users and change their permissions.'),
    ('admin.permissions', 'Manage permissions', 'Manage permissions by user and by sheet type.');

INSERT INTO [access].[Permissions] ([Title], [Description], [Key])
SELECT p.[Title], p.[Description], p.[Key]
FROM @Permissions AS p
WHERE NOT EXISTS (SELECT 1 FROM [access].[Permissions] AS e WHERE e.[Key] = p.[Key]);

DECLARE @AdminObjectID UNIQUEIDENTIFIER = CONVERT (UNIQUEIDENTIFIER, '$(AdminObjectID)');

IF NOT EXISTS (SELECT 1 FROM [access].[Users] WHERE [ObjectID] = @AdminObjectID)
BEGIN
    INSERT INTO [access].[Users] ([ObjectID], [DisplayName], [EmailAddress])
    VALUES (@AdminObjectID, '$(AdminDisplayName)', N'$(AdminEmail)');
END;

DECLARE @AdminUserID SMALLINT = (SELECT [UserID] FROM [access].[Users] WHERE [ObjectID] = @AdminObjectID);

INSERT INTO [access].[UserPermissions] ([UserID], [PermissionID])
SELECT @AdminUserID, p.[PermissionID]
FROM [access].[Permissions] AS p
WHERE p.[Key] IN (SELECT [Key] FROM @Permissions)
  AND NOT EXISTS (SELECT 1 FROM [access].[UserPermissions] AS e WHERE e.[UserID] = @AdminUserID AND e.[PermissionID] = p.[PermissionID]);

/* ---------------------------------------------------------------------------------------------------------------
   types: phase, sheet type, its phases, properties and structure, widget types
--------------------------------------------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM [types].[SpecSheetPhases] WHERE [Phase] = 'Development')
BEGIN
    INSERT INTO [types].[SpecSheetPhases] ([ParentSpecSheetPhaseID], [Phase], [PhaseShort], [DisplayOrder], [IsObsolete])
    VALUES (NULL, 'Development', 'DEV', 1, 0);
END;

DECLARE @PhaseID SMALLINT = (SELECT [SpecSheetPhaseID] FROM [types].[SpecSheetPhases] WHERE [Phase] = 'Development');

-- The admin approves submissions in the phase, so sheets of a type that requires approval can be published.
IF NOT EXISTS (SELECT 1 FROM [access].[SpecSheetPhaseObservers] WHERE [SpecSheetPhaseID] = @PhaseID AND [UserID] = @AdminUserID)
BEGIN
    INSERT INTO [access].[SpecSheetPhaseObservers] ([SpecSheetPhaseID], [UserID], [IsApprover])
    VALUES (@PhaseID, @AdminUserID, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [types].[SpecSheetTypes] WHERE [Type] = 'Engine Specification')
BEGIN
    INSERT INTO [types].[SpecSheetTypes] ([Type], [DisplayOrder], [TableOrientation], [CanCopyFromMigrated], [RequiresApprovalWorkflow])
    VALUES ('Engine Specification', 1, 'vertical', 0, 1);
END;

DECLARE @TypeID TINYINT = (SELECT [SpecSheetTypeID] FROM [types].[SpecSheetTypes] WHERE [Type] = 'Engine Specification');

IF NOT EXISTS (SELECT 1 FROM [types].[SpecSheetTypePhases] WHERE [SpecSheetTypeID] = @TypeID AND [SpecSheetPhaseID] = @PhaseID)
BEGIN
    INSERT INTO [types].[SpecSheetTypePhases] ([SpecSheetTypeID], [SpecSheetPhaseID])
    VALUES (@TypeID, @PhaseID);
END;

-- The properties the admin UI gives a new type (PropertyConstants.defaultProperties), with fixed ReferenceIDs so they
-- can be matched on a later run.
DECLARE @Properties TABLE (
    [ReferenceID]            UNIQUEIDENTIFIER NOT NULL,
    [ValueName]              VARCHAR (256) NOT NULL,
    [ValueKey]               VARCHAR (256) NOT NULL,
    [ValueControlType]       VARCHAR (64) NOT NULL,
    [DisplayOrder]           SMALLINT NOT NULL,
    [PlaceholderText]        VARCHAR (512) NULL,
    [ValidatorConfiguration] NVARCHAR (MAX) NULL,
    [SaveValueAsProperty]    BIT NOT NULL);
INSERT INTO @Properties
VALUES
    ('5e0d2a52-8f0b-4c4e-9a51-2f6a0c1b7d01', 'Type', 'type', 'Type', 1, 'The type of sheet',
        N'[{"validator":"Required","message":"Type is required"}]', 0),
    ('5e0d2a52-8f0b-4c4e-9a51-2f6a0c1b7d02', 'Phase', 'phase', 'Phase', 2, 'The phase of sheet',
        N'[{"validator":"Required","message":"Phase is required"}]', 0),
    ('5e0d2a52-8f0b-4c4e-9a51-2f6a0c1b7d03', 'Version', 'version', 'Version', 3, 'The version of sheet',
        N'[{"validator":"Required","message":"Version is required"},{"validator":"Min","message":"The version must be at least 1","property":{"$type":"minMaxNumber","number":1}}]', 1),
    ('5e0d2a52-8f0b-4c4e-9a51-2f6a0c1b7d04', 'Copy From', 'copyFrom', 'Copy From', 4, 'The sheet to copy from',
        NULL, 1);

INSERT INTO [types].[SpecSheetTypeProperties]
    ([SpecSheetTypeID], [ValueName], [ValueKey], [ValueType], [ValueControlType], [CanValueBeSetInForm], [DisplayOrder],
     [ReferenceID], [PlaceholderText], [ValidatorConfiguration], [SaveValueAsProperty])
SELECT @TypeID, p.[ValueName], p.[ValueKey], 'int', p.[ValueControlType], 1, p.[DisplayOrder],
       p.[ReferenceID], p.[PlaceholderText], p.[ValidatorConfiguration], p.[SaveValueAsProperty]
FROM @Properties AS p
WHERE NOT EXISTS (SELECT 1 FROM [types].[SpecSheetTypeProperties] AS e WHERE e.[SpecSheetTypeID] = @TypeID AND e.[ReferenceID] = p.[ReferenceID]);

-- One display level (StructureLevelType 1), newest first (SortOrientation 1), labelled by the version.
IF NOT EXISTS (SELECT 1 FROM [types].[SpecSheetTypeStructureLevels] WHERE [SpecSheetTypeID] = @TypeID AND [Level] = 1)
BEGIN
    INSERT INTO [types].[SpecSheetTypeStructureLevels] ([SpecSheetTypeID], [Level], [SortOrientation], [StructureLevelType])
    VALUES (@TypeID, 1, 1, 1);
END;

DECLARE @StructureLevelID INT = (SELECT [SpecSheetTypeStructureLevelID] FROM [types].[SpecSheetTypeStructureLevels] WHERE [SpecSheetTypeID] = @TypeID AND [Level] = 1);

IF NOT EXISTS (SELECT 1 FROM [types].[SpecSheetTypeStructureLevelValues] WHERE [SpecSheetTypeStructureLevelID] = @StructureLevelID)
BEGIN
    INSERT INTO [types].[SpecSheetTypeStructureLevelValues] ([SpecSheetTypeStructureLevelID], [SpecSheetTypePropertyReferenceID], [DisplayOrder])
    VALUES (@StructureLevelID, '5e0d2a52-8f0b-4c4e-9a51-2f6a0c1b7d03', 1);
END;

-- The widget types the UI knows (WidgetTypeKey).
DECLARE @WidgetTypes TABLE ([WidgetTypeKey] UNIQUEIDENTIFIER NOT NULL, [Title] VARCHAR (64) NOT NULL, [Description] VARCHAR (256) NOT NULL);
INSERT INTO @WidgetTypes
VALUES
    ('b5a68cec-cdc7-4c5c-b207-0c024e2590ab', 'Part pair identifier', 'Identifies which part of a part number pair a group holds.'),
    ('64a47646-b4dc-4ee9-9517-0f5fa5d90ebb', 'PFK nominal', 'Works out a nominal from a group''s min and max.');

INSERT INTO [types].[WidgetTypes] ([WidgetTypeKey], [Title], [Description])
SELECT w.[WidgetTypeKey], w.[Title], w.[Description]
FROM @WidgetTypes AS w
WHERE NOT EXISTS (SELECT 1 FROM [types].[WidgetTypes] AS e WHERE e.[WidgetTypeKey] = w.[WidgetTypeKey]);

/* ---------------------------------------------------------------------------------------------------------------
   template: cell types
--------------------------------------------------------------------------------------------------------------- */

DECLARE @DefaultStyling NVARCHAR (MAX) = N'{"backgroundColor":"#FFFFFF","fontWeight":400,"fontSize":"inherit","width":null,"isSticky":false}';
DECLARE @HeaderStyling NVARCHAR (MAX) = N'{"backgroundColor":"#F3F4F6","fontWeight":600,"fontSize":"inherit","width":null,"isSticky":false}';

DECLARE @CellTypes TABLE (
    [TableCellTypeKey]          UNIQUEIDENTIFIER NOT NULL,
    [Title]                     VARCHAR (64) NOT NULL,
    [Description]               VARCHAR (256) NOT NULL,
    [Type]                      VARCHAR (64) NOT NULL,
    [ModelType]                 VARCHAR (32) NOT NULL,
    [IsHeader]                  BIT NOT NULL,
    [RequiresUserConfiguration] BIT NOT NULL);
INSERT INTO @CellTypes
VALUES
    ('a4ba29b2-e5f3-4eb3-b9f9-50fd0709249f', 'Header', 'A column heading.', 'header', 'text', 1, 0),
    ('c5bd9472-049f-45ef-b0f5-314a43ccf558', 'Description', 'What the row is.', 'description', 'text', 0, 0),
    ('ee8dcd87-a7fa-4f4e-b7a8-bb5d97c5bc56', 'Group', 'A group heading.', 'group', 'text', 1, 0),
    ('895b2dc0-a763-4eee-ab2e-113526710e1d', 'Min (no OK)', 'A lower limit, not acceptable at the limit.', 'minNoOk', 'numeric', 0, 0),
    ('de3bfe3a-0068-48c0-b83d-f4a1a197890f', 'Min', 'A lower limit.', 'min', 'numeric', 0, 0),
    ('69913b9e-fdfb-44d5-ae97-923c98fb47f1', 'Max', 'An upper limit.', 'max', 'numeric', 0, 0),
    ('24dbf226-0caf-4177-aad7-2c2ca8e466d0', 'Max (no OK)', 'An upper limit, not acceptable at the limit.', 'maxNoOk', 'numeric', 0, 0),
    ('a2f0f5f1-9f0c-48d5-bfa4-15661234f691', 'Numeric value', 'A number.', 'value', 'numeric', 0, 0),
    ('4af997a4-26a0-4fb4-8279-1f93e0267032', 'Nominal', 'A nominal value.', 'nom', 'numeric', 0, 0),
    ('b485a18e-b833-40fe-bca9-cf753e5e6c9d', 'Grade (numeric)', 'A numeric grade.', 'grade', 'numeric', 0, 0),
    ('53469bc1-8aba-4082-bf3a-2cecd52d11de', 'Grade (text)', 'A text grade.', 'grade', 'text', 0, 0),
    ('53b1358f-c4a0-47b4-b293-ae972e32a6fa', 'Text value', 'Free text.', 'value', 'text', 0, 0),
    ('7256607b-450a-4c51-88eb-c04a495aa318', 'PFK part number', 'A PFK part number.', 'partNumber', 'text', 0, 0),
    ('9c0e6f39-9b43-4174-a25b-aff1160c9c84', 'Part number (upper)', 'The upper part number.', 'partNumberUpper', 'text', 0, 0),
    ('6804cd07-0042-4822-933d-7c6f63f75f45', 'Part number (lower)', 'The lower part number.', 'partNumberLower', 'text', 0, 0),
    ('c1156c8f-ad0d-4718-b86a-2b596a21fa8a', 'Part number', 'A part number.', 'partNumber', 'text', 0, 0),
    ('3288b57f-54f8-4229-914e-f940eb3bcac6', 'Parts part number', 'A part number from the parts list.', 'partNumber', 'text', 0, 0),
    ('f190ee74-66ca-4b5c-a7a2-8f341937ed2f', 'Engine phase', 'A phase.', 'value', 'numeric', 0, 0),
    ('8fc238a1-1d47-4405-be2a-fb9f5de64cfd', 'Part number pair', 'Two part numbers side by side.', 'partNumberPair', 'pair', 0, 0),
    ('3c19eb02-dcfd-452b-a12e-12989ddc38d5', 'Text dropdown', 'A choice from a list of text options.', 'value', 'textDropdown', 0, 1),
    ('57e59871-a020-449f-b7d2-55267fe52210', 'Numeric dropdown', 'A choice from a list of numbers.', 'value', 'numericDropdown', 0, 1),
    ('2c0d4021-e546-42f0-b27d-02b4c976b2a0', 'Table values dropdown', 'A choice from another table''s values.', 'value', 'linkedSpecSheet', 0, 1),
    ('2daa9ed8-1370-46f2-8b3e-064aa67a055b', 'Linked sheet values dropdown', 'A choice from another sheet''s table values.', 'value', 'linkedSpecSheet', 0, 1),
    ('1ec7345b-d514-4b6a-bb00-479a733cf89b', 'Engine spec part dropdown', 'A part from the engine specification.', 'value', 'linkedSpecSheet', 0, 1),
    ('1f939ee2-215d-4345-a0d2-0f794789f11a', 'End gap', 'A ring end gap.', 'value', 'numeric', 0, 0),
    ('d95a054a-c2b2-4f58-bd8e-1ca2e6b3f7e1', 'Thickness', 'A thickness.', 'value', 'numeric', 0, 0);

INSERT INTO [template].[TableCellTypes] ([TableCellTypeKey], [Title], [Description], [Type], [ModelType], [DefaultStyling], [RequiresUserConfiguration])
SELECT c.[TableCellTypeKey], c.[Title], c.[Description], c.[Type], c.[ModelType],
       CASE WHEN c.[IsHeader] = 1 THEN @HeaderStyling ELSE @DefaultStyling END, c.[RequiresUserConfiguration]
FROM @CellTypes AS c
WHERE NOT EXISTS (SELECT 1 FROM [template].[TableCellTypes] AS e WHERE e.[TableCellTypeKey] = c.[TableCellTypeKey]);

/* ---------------------------------------------------------------------------------------------------------------
   template: the "Limits" table template

   A header group (one row of column headings) and a "Limit" group under it (one row of cells), both default, so a
   new table starts with the headings and one limit and more limits are added from the header's add menu.

       Description   | Min     | Max     | Unit
       text          | numeric | numeric | text dropdown
--------------------------------------------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM [template].[TableTemplates] WHERE [SpecSheetTypeID] = @TypeID AND [Type] = 'Limits')
BEGIN
    INSERT INTO [template].[TableTemplates] ([Type], [Description], [SpecSheetTypeID], [RequiresUserConfiguration], [DefaultConfiguration], [ShowNotes], [IsLegacy])
    VALUES ('Limits', 'A list of limits, each with a description, a min, a max and a unit.', @TypeID, 0, NULL, 1, 0);
END;

DECLARE @TemplateID TINYINT = (SELECT [TableTemplateID] FROM [template].[TableTemplates] WHERE [SpecSheetTypeID] = @TypeID AND [Type] = 'Limits');

IF NOT EXISTS (SELECT 1 FROM [template].[TableGroupTemplates] WHERE [TableTemplateID] = @TemplateID AND [Title] = 'Header')
BEGIN
    INSERT INTO [template].[TableGroupTemplates]
        ([TableTemplateID], [ParentTableGroupTemplateID], [Title], [GroupKey], [GroupType], [DisplayOrder], [IsDefault],
         [CanUserAddToTable], [ShowInContextMenu], [CanBeReordered], [CopyFromPrevious], [ShowContextMenu])
    VALUES (@TemplateID, NULL, 'Header', NULL, 'header', 1, 1, 0, 0, 0, 0, 0);
END;

DECLARE @HeaderGroupID INT = (SELECT [TableGroupTemplateID] FROM [template].[TableGroupTemplates] WHERE [TableTemplateID] = @TemplateID AND [Title] = 'Header');

IF NOT EXISTS (SELECT 1 FROM [template].[TableGroupTemplates] WHERE [TableTemplateID] = @TemplateID AND [Title] = 'Limit')
BEGIN
    INSERT INTO [template].[TableGroupTemplates]
        ([TableTemplateID], [ParentTableGroupTemplateID], [Title], [GroupKey], [GroupType], [DisplayOrder], [IsDefault],
         [CanUserAddToTable], [ShowInContextMenu], [CanBeReordered], [CopyFromPrevious], [ShowContextMenu])
    VALUES (@TemplateID, @HeaderGroupID, 'Limit', NULL, 'row', 2, 1, 1, 1, 1, 0, 1);
END;

DECLARE @LimitGroupID INT = (SELECT [TableGroupTemplateID] FROM [template].[TableGroupTemplates] WHERE [TableTemplateID] = @TemplateID AND [Title] = 'Limit');

IF NOT EXISTS (SELECT 1 FROM [template].[TableRowTemplates] WHERE [TableGroupTemplateID] = @HeaderGroupID AND [RowNumber] = 1)
BEGIN
    INSERT INTO [template].[TableRowTemplates] ([TableGroupTemplateID], [RowNumber], [RowValidators], [ShowContextMenu], [CanBeReordered])
    VALUES (@HeaderGroupID, 1, NULL, 0, 0);
END;

IF NOT EXISTS (SELECT 1 FROM [template].[TableRowTemplates] WHERE [TableGroupTemplateID] = @LimitGroupID AND [RowNumber] = 1)
BEGIN
    INSERT INTO [template].[TableRowTemplates] ([TableGroupTemplateID], [RowNumber], [RowValidators], [ShowContextMenu], [CanBeReordered])
    VALUES (@LimitGroupID, 1, NULL, 1, 1);
END;

DECLARE @HeaderRowID INT = (SELECT [TableRowTemplateID] FROM [template].[TableRowTemplates] WHERE [TableGroupTemplateID] = @HeaderGroupID AND [RowNumber] = 1);
DECLARE @LimitRowID INT = (SELECT [TableRowTemplateID] FROM [template].[TableRowTemplates] WHERE [TableGroupTemplateID] = @LimitGroupID AND [RowNumber] = 1);

-- Every cell gets its own configuration. The vertical table sizes its columns from the widths (in em) of the widest
-- default row, so every cell has one.
DECLARE @Cells TABLE (
    [TableRowTemplateID] INT NOT NULL,
    [ColumnNumber]       TINYINT NOT NULL,
    [TableCellTypeKey]   UNIQUEIDENTIFIER NOT NULL,
    [Styling]            NVARCHAR (MAX) NOT NULL,
    [Properties]         NVARCHAR (MAX) NOT NULL,
    [IsIdentifier]       BIT NOT NULL);
INSERT INTO @Cells
VALUES
    (@HeaderRowID, 1, 'a4ba29b2-e5f3-4eb3-b9f9-50fd0709249f',
        N'{"backgroundColor":"#F3F4F6","fontWeight":600,"fontSize":"inherit","width":16,"isSticky":false}',
        N'{"$type":"text","defaultValue":"Description"}', 0),
    (@HeaderRowID, 2, 'a4ba29b2-e5f3-4eb3-b9f9-50fd0709249f',
        N'{"backgroundColor":"#F3F4F6","fontWeight":600,"fontSize":"inherit","width":6,"isSticky":false}',
        N'{"$type":"text","defaultValue":"Min"}', 0),
    (@HeaderRowID, 3, 'a4ba29b2-e5f3-4eb3-b9f9-50fd0709249f',
        N'{"backgroundColor":"#F3F4F6","fontWeight":600,"fontSize":"inherit","width":6,"isSticky":false}',
        N'{"$type":"text","defaultValue":"Max"}', 0),
    (@HeaderRowID, 4, 'a4ba29b2-e5f3-4eb3-b9f9-50fd0709249f',
        N'{"backgroundColor":"#F3F4F6","fontWeight":600,"fontSize":"inherit","width":6,"isSticky":false}',
        N'{"$type":"text","defaultValue":"Unit"}', 0),
    (@LimitRowID, 1, 'c5bd9472-049f-45ef-b0f5-314a43ccf558',
        N'{"backgroundColor":"#FFFFFF","fontWeight":400,"fontSize":"inherit","width":16,"isSticky":false}',
        N'{"$type":"text","defaultValue":null}', 1),
    (@LimitRowID, 2, 'de3bfe3a-0068-48c0-b83d-f4a1a197890f',
        N'{"backgroundColor":"#FFFFFF","fontWeight":400,"fontSize":"inherit","width":6,"isSticky":false}',
        N'{"$type":"numeric","decimalPlaces":2,"defaultValue":null}', 0),
    (@LimitRowID, 3, '69913b9e-fdfb-44d5-ae97-923c98fb47f1',
        N'{"backgroundColor":"#FFFFFF","fontWeight":400,"fontSize":"inherit","width":6,"isSticky":false}',
        N'{"$type":"numeric","decimalPlaces":2,"defaultValue":null}', 0),
    (@LimitRowID, 4, '3c19eb02-dcfd-452b-a12e-12989ddc38d5',
        N'{"backgroundColor":"#FFFFFF","fontWeight":400,"fontSize":"inherit","width":6,"isSticky":false}',
        N'{"$type":"textDropdown","options":["mm","um","N.m","bar","degC"],"defaultValue":"mm"}', 0);

DECLARE @RowID INT, @ColumnNumber TINYINT, @CellTypeKey UNIQUEIDENTIFIER, @Styling NVARCHAR (MAX), @CellProperties NVARCHAR (MAX), @IsIdentifier BIT;
DECLARE @ConfigurationID INT;

DECLARE CellCursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT c.[TableRowTemplateID], c.[ColumnNumber], c.[TableCellTypeKey], c.[Styling], c.[Properties], c.[IsIdentifier]
    FROM @Cells AS c
    WHERE NOT EXISTS (
        SELECT 1 FROM [template].[TableCellTemplates] AS e
        WHERE e.[TableRowTemplateID] = c.[TableRowTemplateID] AND e.[ColumnNumber] = c.[ColumnNumber])
    ORDER BY c.[TableRowTemplateID], c.[ColumnNumber];

OPEN CellCursor;
FETCH NEXT FROM CellCursor INTO @RowID, @ColumnNumber, @CellTypeKey, @Styling, @CellProperties, @IsIdentifier;
WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO [configuration].[CellConfigurations] ([Styling], [Properties], [Validators], [IsIdentifier])
    VALUES (@Styling, @CellProperties, NULL, @IsIdentifier);
    SET @ConfigurationID = SCOPE_IDENTITY();

    INSERT INTO [template].[TableCellTemplates] ([TableRowTemplateID], [TableCellTypeID], [CellConfigurationID], [ColumnNumber], [ColSpan], [RowSpan], [GroupKey])
    SELECT @RowID, t.[TableCellTypeID], @ConfigurationID, @ColumnNumber, 1, 1, NULL
    FROM [template].[TableCellTypes] AS t
    WHERE t.[TableCellTypeKey] = @CellTypeKey;

    FETCH NEXT FROM CellCursor INTO @RowID, @ColumnNumber, @CellTypeKey, @Styling, @CellProperties, @IsIdentifier;
END;
CLOSE CellCursor;
DEALLOCATE CellCursor;

COMMIT TRANSACTION;

PRINT 'Seeded: admin user ' + CONVERT (VARCHAR (36), @AdminObjectID) + ' (UserID ' + CONVERT (VARCHAR (6), @AdminUserID)
    + '), phase Development, type Engine Specification, template Limits.';
