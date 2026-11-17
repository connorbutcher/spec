CREATE TABLE [spec].[SheetTableCells] (
    [SheetTableCellID]    INT              IDENTITY (1, 1) NOT NULL,
    [SheetTableRowID]     INT              NOT NULL,
    [TableCellTemplateID] INT              NOT NULL,
    [ReferenceID]         UNIQUEIDENTIFIER CONSTRAINT [DF_SheetTableCells_ReferenceID] DEFAULT (newsequentialid()) NOT NULL,
    [CreatedDateTime]     DATETIME2 (7)    CONSTRAINT [DF_SheetTableCells_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetTableCells] PRIMARY KEY CLUSTERED ([SheetTableCellID] ASC),
    CONSTRAINT [FK_SheetTableCells_SheetTableRows_SheetTableRowID] FOREIGN KEY ([SheetTableRowID]) REFERENCES [spec].[SheetTableRows] ([SheetTableRowID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableCells_TableCellTemplates_TableCellTemplateID] FOREIGN KEY ([TableCellTemplateID]) REFERENCES [template].[TableCellTemplates] ([TableCellTemplateID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableCells_SheetTableRowID_TableCellTemplateID]
    ON [spec].[SheetTableCells]([SheetTableRowID] ASC, [TableCellTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableCells_TableCellTemplateID]
    ON [spec].[SheetTableCells]([TableCellTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableCells_ReferenceID]
    ON [spec].[SheetTableCells]([ReferenceID] ASC);

