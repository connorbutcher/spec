CREATE TABLE [dbo].[SheetSections] (
    [Id]                   INT              IDENTITY (1, 1) NOT NULL,
    [PublicId]             UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [SheetTableId]         INT              NOT NULL,
    [TemplateSectionId]    INT              NOT NULL,
    [ParentSheetSectionId] INT              NULL,
    [CreatedAtUtc]         DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetSections] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetSections_SheetSections_ParentSheetSectionId] FOREIGN KEY ([ParentSheetSectionId]) REFERENCES [dbo].[SheetSections] ([Id]),
    CONSTRAINT [FK_SheetSections_SheetTables_SheetTableId] FOREIGN KEY ([SheetTableId]) REFERENCES [dbo].[SheetTables] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetSections_TemplateSections_TemplateSectionId] FOREIGN KEY ([TemplateSectionId]) REFERENCES [dbo].[TemplateSections] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSections_SheetTableId]
    ON [dbo].[SheetSections]([SheetTableId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSections_TemplateSectionId]
    ON [dbo].[SheetSections]([TemplateSectionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSections_ParentSheetSectionId]
    ON [dbo].[SheetSections]([ParentSheetSectionId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetSections_PublicId]
    ON [dbo].[SheetSections]([PublicId] ASC);


GO

