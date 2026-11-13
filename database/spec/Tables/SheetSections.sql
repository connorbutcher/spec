CREATE TABLE [spec].[SheetSections] (
    [Id]                   INT              IDENTITY (1, 1) NOT NULL,
    [PublicId]             UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [SheetTableId]         INT              NOT NULL,
    [TemplateSectionId]    INT              NOT NULL,
    [ParentSheetSectionId] INT              NULL,
    [CreatedAtUtc]         DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetSections] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetSections_SheetSections_ParentSheetSectionId] FOREIGN KEY ([ParentSheetSectionId]) REFERENCES [spec].[SheetSections] ([Id]),
    CONSTRAINT [FK_SheetSections_SheetTables_SheetTableId] FOREIGN KEY ([SheetTableId]) REFERENCES [spec].[SheetTables] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetSections_TemplateSections_TemplateSectionId] FOREIGN KEY ([TemplateSectionId]) REFERENCES [design].[TemplateSections] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSections_SheetTableId]
    ON [spec].[SheetSections]([SheetTableId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSections_TemplateSectionId]
    ON [spec].[SheetSections]([TemplateSectionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSections_ParentSheetSectionId]
    ON [spec].[SheetSections]([ParentSheetSectionId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetSections_PublicId]
    ON [spec].[SheetSections]([PublicId] ASC);


GO

