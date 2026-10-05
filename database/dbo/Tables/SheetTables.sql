CREATE TABLE [dbo].[SheetTables] (
    [Id]                     INT              IDENTITY (1, 1) NOT NULL,
    [SheetId]                INT              NOT NULL,
    [TableTemplateVersionId] INT              NOT NULL,
    [CreatedAtUtc]           DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    [PublicId]               UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    CONSTRAINT [PK_SheetTables] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetTables_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [dbo].[Sheets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTables_TableTemplateVersions_TableTemplateVersionId] FOREIGN KEY ([TableTemplateVersionId]) REFERENCES [dbo].[TableTemplateVersions] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetTables_TableTemplateVersionId]
    ON [dbo].[SheetTables]([TableTemplateVersionId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetTables_PublicId]
    ON [dbo].[SheetTables]([PublicId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetTables_SheetId]
    ON [dbo].[SheetTables]([SheetId] ASC);


GO

