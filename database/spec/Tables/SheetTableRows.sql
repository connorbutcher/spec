CREATE TABLE [spec].[SheetTableRows] (
    [SheetTableRowID]    INT              IDENTITY (1, 1) NOT NULL,
    [SheetTableGroupID]  INT              NOT NULL,
    [TableRowTemplateID] INT              NOT NULL,
    [ReferenceID]        UNIQUEIDENTIFIER CONSTRAINT [DF_SheetTableRows_ReferenceID] DEFAULT (newsequentialid()) NOT NULL,
    [CreatedDateTime]    DATETIME2 (7)    CONSTRAINT [DF_SheetTableRows_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetTableRows] PRIMARY KEY CLUSTERED ([SheetTableRowID] ASC),
    CONSTRAINT [FK_SheetTableRows_SheetTableGroups_SheetTableGroupID] FOREIGN KEY ([SheetTableGroupID]) REFERENCES [spec].[SheetTableGroups] ([SheetTableGroupID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableRows_TableRowTemplates_TableRowTemplateID] FOREIGN KEY ([TableRowTemplateID]) REFERENCES [template].[TableRowTemplates] ([TableRowTemplateID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRows_SheetTableGroupID]
    ON [spec].[SheetTableRows]([SheetTableGroupID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRows_TableRowTemplateID]
    ON [spec].[SheetTableRows]([TableRowTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRows_ReferenceID]
    ON [spec].[SheetTableRows]([ReferenceID] ASC);

