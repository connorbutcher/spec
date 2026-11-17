CREATE TABLE [spec].[SheetTableGroups] (
    [SheetTableGroupID]    INT              IDENTITY (1, 1) NOT NULL,
    [SheetTableID]         INT              NOT NULL,
    [TableGroupTemplateID] INT              NOT NULL,
    [ReferenceID]          UNIQUEIDENTIFIER CONSTRAINT [DF_SheetTableGroups_ReferenceID] DEFAULT (newsequentialid()) NOT NULL,
    [CreatedDateTime]      DATETIME2 (7)    CONSTRAINT [DF_SheetTableGroups_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetTableGroups] PRIMARY KEY CLUSTERED ([SheetTableGroupID] ASC),
    CONSTRAINT [FK_SheetTableGroups_SheetTables_SheetTableID] FOREIGN KEY ([SheetTableID]) REFERENCES [spec].[SheetTables] ([SheetTableID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableGroups_TableGroupTemplates_TableGroupTemplateID] FOREIGN KEY ([TableGroupTemplateID]) REFERENCES [template].[TableGroupTemplates] ([TableGroupTemplateID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroups_SheetTableID]
    ON [spec].[SheetTableGroups]([SheetTableID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroups_TableGroupTemplateID]
    ON [spec].[SheetTableGroups]([TableGroupTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroups_ReferenceID]
    ON [spec].[SheetTableGroups]([ReferenceID] ASC);

