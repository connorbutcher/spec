CREATE TABLE [spec].[Sheets] (
    [Id]           INT              IDENTITY (1, 1) NOT NULL,
    [PhaseId]      INT              NOT NULL,
    [SheetTypeId]  INT              NOT NULL,
    [CreatedAtUtc] DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    [PublicId]     UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [RowVersion]   ROWVERSION       NOT NULL,
    CONSTRAINT [PK_Sheets] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Sheets_Phases_PhaseId] FOREIGN KEY ([PhaseId]) REFERENCES [setup].[Phases] ([Id]),
    CONSTRAINT [FK_Sheets_SheetTypes_SheetTypeId] FOREIGN KEY ([SheetTypeId]) REFERENCES [setup].[SheetTypes] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_Sheets_SheetTypeId]
    ON [spec].[Sheets]([SheetTypeId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Sheets_PublicId]
    ON [spec].[Sheets]([PublicId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Sheets_PhaseId_SheetTypeId]
    ON [spec].[Sheets]([PhaseId] ASC, [SheetTypeId] ASC);


GO

