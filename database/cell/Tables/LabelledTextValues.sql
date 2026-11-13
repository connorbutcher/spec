CREATE TABLE [cell].[LabelledTextValues] (
    [SheetRowRevisionId] INT             NOT NULL,
    [SheetCellId]        INT             NOT NULL,
    [SlotKey]            VARCHAR (20)    NOT NULL,
    [Position]           INT             NOT NULL,
    [Value]              NVARCHAR (4000) NOT NULL,
    [LookupValue]        AS              (CONVERT([nvarchar](200),left([Value],(200)))) PERSISTED,
    CONSTRAINT [PK_LabelledTextValues] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC, [SlotKey] ASC),
    CONSTRAINT [FK_LabelledTextValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [spec].[SheetCells] ([Id]),
    CONSTRAINT [FK_LabelledTextValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [spec].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_LabelledTextValues_LookupValue]
    ON [cell].[LabelledTextValues]([LookupValue] ASC)
    INCLUDE([SheetCellId], [SheetRowRevisionId]);


GO

CREATE NONCLUSTERED INDEX [IX_LabelledTextValues_SheetCellId]
    ON [cell].[LabelledTextValues]([SheetCellId] ASC);


GO

