CREATE TABLE [legacy].[SpecSheetTableGroups] (
    [SpecSheetTableGroupID]       INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableID]            INT NOT NULL,
    [LinkedSpecSheetTableGroupID] INT NULL,
    [TableGroupTemplateID]        INT NOT NULL,
    [Order]                       INT NOT NULL,
    [IsDeleted]                   BIT CONSTRAINT [DF_SpecSheetTableGroups_IsDeleted] DEFAULT ((0)) NOT NULL,
    [ReferenceID]                 UNIQUEIDENTIFIER NOT NULL,
    [CreatedByUserID]             SMALLINT NOT NULL,
    [CreatedDateTime]             DATETIME2 (0) NOT NULL,
    [LastModifiedByUserID]        SMALLINT NULL,
    [LastModifiedDateTime]        DATETIME2 (0) NULL,
    CONSTRAINT [PK_SpecSheetTableGroups] PRIMARY KEY CLUSTERED ([SpecSheetTableGroupID] ASC),
    CONSTRAINT [FK_SpecSheetTableGroups_SpecSheetTables] FOREIGN KEY ([SpecSheetTableID]) REFERENCES [legacy].[SpecSheetTables] ([SpecSheetTableID]),
    CONSTRAINT [FK_SpecSheetTableGroups_SpecSheetTableGroups_LinkedSpecSheetTableGroupID] FOREIGN KEY ([LinkedSpecSheetTableGroupID]) REFERENCES [legacy].[SpecSheetTableGroups] ([SpecSheetTableGroupID]),
    CONSTRAINT [FK_SpecSheetTableGroups_TableGroupTemplates] FOREIGN KEY ([TableGroupTemplateID]) REFERENCES [template].[TableGroupTemplates] ([TableGroupTemplateID]),
    CONSTRAINT [FK_SpecSheetTableGroups_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SpecSheetTableGroups_Users_LastModifiedByUserID] FOREIGN KEY ([LastModifiedByUserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableGroups_SpecSheetTableID]
    ON [legacy].[SpecSheetTableGroups]([SpecSheetTableID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableGroups_LinkedSpecSheetTableGroupID]
    ON [legacy].[SpecSheetTableGroups]([LinkedSpecSheetTableGroupID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableGroups_TableGroupTemplateID]
    ON [legacy].[SpecSheetTableGroups]([TableGroupTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableGroups_CreatedByUserID]
    ON [legacy].[SpecSheetTableGroups]([CreatedByUserID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableGroups_LastModifiedByUserID]
    ON [legacy].[SpecSheetTableGroups]([LastModifiedByUserID] ASC);

