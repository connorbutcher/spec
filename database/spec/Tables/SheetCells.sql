CREATE TABLE [spec].[SheetCells] (
    [Id]                 INT              IDENTITY (1, 1) NOT NULL,
    [PublicId]           UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [SheetRowId]         INT              NOT NULL,
    [TemplateCellId]     INT              NOT NULL,
    [SheetColumnBlockId] INT              NULL,
    CONSTRAINT [PK_SheetCells] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetCells_SheetColumnBlocks_SheetColumnBlockId] FOREIGN KEY ([SheetColumnBlockId]) REFERENCES [spec].[SheetColumnBlocks] ([Id]),
    CONSTRAINT [FK_SheetCells_SheetRows_SheetRowId] FOREIGN KEY ([SheetRowId]) REFERENCES [spec].[SheetRows] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetCells_TemplateCells_TemplateCellId] FOREIGN KEY ([TemplateCellId]) REFERENCES [design].[TemplateCells] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetCells_TemplateCellId]
    ON [spec].[SheetCells]([TemplateCellId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetCells_SheetRowId_TemplateCellId_SheetColumnBlockId]
    ON [spec].[SheetCells]([SheetRowId] ASC, [TemplateCellId] ASC, [SheetColumnBlockId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetCells_PublicId]
    ON [spec].[SheetCells]([PublicId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetCells_SheetColumnBlockId]
    ON [spec].[SheetCells]([SheetColumnBlockId] ASC);


GO

