CREATE TABLE [dbo].[SpecSheetTableRows] (
    [SpecSheetTableRowID]   INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableGroupID] INT NOT NULL,
    [TableRowTemplateID]    INT NOT NULL,
    [RowNumber]             TINYINT NOT NULL,
    [ReferenceID]           UNIQUEIDENTIFIER NOT NULL,
    [IsDeleted]             BIT CONSTRAINT [DF_SpecSheetTableRows_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedByUserID]       SMALLINT NOT NULL,
    [CreatedDateTime]       DATETIME2 (0) NOT NULL,
    [LastModifiedByUserID]  SMALLINT NULL,
    [LastModifiedDateTime]  DATETIME2 (0) NULL,
    CONSTRAINT [PK_SpecSheetTableRows] PRIMARY KEY CLUSTERED ([SpecSheetTableRowID] ASC),
    CONSTRAINT [FK_SpecSheetTableRows_SpecSheetTableGroups] FOREIGN KEY ([SpecSheetTableGroupID]) REFERENCES [dbo].[SpecSheetTableGroups] ([SpecSheetTableGroupID]),
    CONSTRAINT [FK_SpecSheetTableRows_TableRowTemplates] FOREIGN KEY ([TableRowTemplateID]) REFERENCES [template].[TableRowTemplates] ([TableRowTemplateID]),
    CONSTRAINT [FK_SpecSheetTableRows_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SpecSheetTableRows_Users_LastModifiedByUserID] FOREIGN KEY ([LastModifiedByUserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableRows_SpecSheetTableGroupID]
    ON [dbo].[SpecSheetTableRows]([SpecSheetTableGroupID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableRows_TableRowTemplateID]
    ON [dbo].[SpecSheetTableRows]([TableRowTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableRows_CreatedByUserID]
    ON [dbo].[SpecSheetTableRows]([CreatedByUserID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableRows_LastModifiedByUserID]
    ON [dbo].[SpecSheetTableRows]([LastModifiedByUserID] ASC);

