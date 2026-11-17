CREATE TABLE [spec].[SheetLinkedTables] (
    [Id]                  INT              IDENTITY (1, 1) NOT NULL,
    [PublicId]            UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [SheetId]             INT              NOT NULL,
    [LinkedTablePublicId] UNIQUEIDENTIFIER NOT NULL,
    [LinkedByUserId]      INT              NOT NULL,
    [LinkedAtUtc]         DATETIME2 (7)    NOT NULL,
    [CreatedAtUtc]        DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetLinkedTables] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetLinkedTables_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetLinkedTables_Users_LinkedByUserId] FOREIGN KEY ([LinkedByUserId]) REFERENCES [auth].[Users] ([Id])
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetLinkedTables_PublicId]
    ON [spec].[SheetLinkedTables]([PublicId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetLinkedTables_SheetId_LinkedTablePublicId]
    ON [spec].[SheetLinkedTables]([SheetId] ASC, [LinkedTablePublicId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetLinkedTables_LinkedByUserId]
    ON [spec].[SheetLinkedTables]([LinkedByUserId] ASC);


GO
