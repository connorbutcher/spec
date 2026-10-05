CREATE TABLE [dbo].[SheetColumnBlocks] (
    [Id]                    INT              IDENTITY (1, 1) NOT NULL,
    [PublicId]              UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [SheetTableId]          INT              NOT NULL,
    [TemplateColumnBlockId] INT              NOT NULL,
    [CreatedAtUtc]          DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetColumnBlocks] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetColumnBlocks_SheetTables_SheetTableId] FOREIGN KEY ([SheetTableId]) REFERENCES [dbo].[SheetTables] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetColumnBlocks_TemplateColumnBlocks_TemplateColumnBlockId] FOREIGN KEY ([TemplateColumnBlockId]) REFERENCES [dbo].[TemplateColumnBlocks] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetColumnBlocks_SheetTableId]
    ON [dbo].[SheetColumnBlocks]([SheetTableId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetColumnBlocks_PublicId]
    ON [dbo].[SheetColumnBlocks]([PublicId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetColumnBlocks_TemplateColumnBlockId]
    ON [dbo].[SheetColumnBlocks]([TemplateColumnBlockId] ASC);


GO

