CREATE TABLE [dbo].[SheetRows] (
    [Id]             INT              IDENTITY (1, 1) NOT NULL,
    [SheetSectionId] INT              NOT NULL,
    [TemplateRowId]  INT              NOT NULL,
    [CreatedAtUtc]   DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    [PublicId]       UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    CONSTRAINT [PK_SheetRows] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetRows_SheetSections_SheetSectionId] FOREIGN KEY ([SheetSectionId]) REFERENCES [dbo].[SheetSections] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetRows_TemplateRows_TemplateRowId] FOREIGN KEY ([TemplateRowId]) REFERENCES [dbo].[TemplateRows] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetRows_SheetSectionId]
    ON [dbo].[SheetRows]([SheetSectionId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetRows_PublicId]
    ON [dbo].[SheetRows]([PublicId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetRows_TemplateRowId]
    ON [dbo].[SheetRows]([TemplateRowId] ASC);


GO

