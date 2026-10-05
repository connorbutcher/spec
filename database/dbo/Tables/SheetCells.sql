CREATE TABLE [dbo].[SheetCells] (
    [Id]                 INT              IDENTITY (1, 1) NOT NULL,
    [PublicId]           UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [SheetRowId]         INT              NOT NULL,
    [TemplateCellId]     INT              NOT NULL,
    [SheetColumnBlockId] INT              NULL,
    CONSTRAINT [PK_SheetCells] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetCells_SheetColumnBlocks_SheetColumnBlockId] FOREIGN KEY ([SheetColumnBlockId]) REFERENCES [dbo].[SheetColumnBlocks] ([Id]),
    CONSTRAINT [FK_SheetCells_SheetRows_SheetRowId] FOREIGN KEY ([SheetRowId]) REFERENCES [dbo].[SheetRows] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetCells_TemplateCells_TemplateCellId] FOREIGN KEY ([TemplateCellId]) REFERENCES [dbo].[TemplateCells] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetCells_TemplateCellId]
    ON [dbo].[SheetCells]([TemplateCellId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetCells_SheetRowId_TemplateCellId_SheetColumnBlockId]
    ON [dbo].[SheetCells]([SheetRowId] ASC, [TemplateCellId] ASC, [SheetColumnBlockId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetCells_PublicId]
    ON [dbo].[SheetCells]([PublicId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetCells_SheetColumnBlockId]
    ON [dbo].[SheetCells]([SheetColumnBlockId] ASC);


GO

