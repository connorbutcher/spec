CREATE TABLE [spec].[SheetProperties] (
    [Id]           INT              IDENTITY (1, 1) NOT NULL,
    [PublicId]     UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [SheetId]      INT              NOT NULL,
    [SheetTypePropertyId] INT       NOT NULL,
    [CreatedAtUtc] DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetProperties] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetProperties_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetProperties_SheetTypeProperties_SheetTypePropertyId] FOREIGN KEY ([SheetTypePropertyId]) REFERENCES [setup].[SheetTypeProperties] ([Id])
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetProperties_SheetId_SheetTypePropertyId]
    ON [spec].[SheetProperties]([SheetId] ASC, [SheetTypePropertyId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetProperties_SheetTypePropertyId]
    ON [spec].[SheetProperties]([SheetTypePropertyId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetProperties_PublicId]
    ON [spec].[SheetProperties]([PublicId] ASC);


GO

