CREATE TABLE [spec].[SheetExternalReferences] (
    [Id]           INT              IDENTITY (1, 1) NOT NULL,
    [PublicId]     UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [SheetId]      INT              NOT NULL,
    [ReferenceId]  UNIQUEIDENTIFIER NOT NULL,
    [CreatedAtUtc] DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetExternalReferences] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetExternalReferences_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]) ON DELETE CASCADE
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetExternalReferences_SheetId_ReferenceId]
    ON [spec].[SheetExternalReferences]([SheetId] ASC, [ReferenceId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetExternalReferences_PublicId]
    ON [spec].[SheetExternalReferences]([PublicId] ASC);


GO

