CREATE TABLE [spec].[SheetProperties] (
    [Id]           INT              IDENTITY (1, 1) NOT NULL,
    [PublicId]     UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [SheetId]      INT              NOT NULL,
    [CreatedAtUtc] DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetProperties] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetProperties_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetProperties_SheetId]
    ON [spec].[SheetProperties]([SheetId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetProperties_PublicId]
    ON [spec].[SheetProperties]([PublicId] ASC);


GO

