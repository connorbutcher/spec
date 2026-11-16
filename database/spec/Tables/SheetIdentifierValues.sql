CREATE TABLE [spec].[SheetIdentifierValues] (
    [SheetId]               INT           NOT NULL,
    [SheetTypeIdentifierId] INT           NOT NULL,
    [Value]                 NVARCHAR (80) NOT NULL,
    CONSTRAINT [PK_SheetIdentifierValues] PRIMARY KEY CLUSTERED ([SheetId] ASC, [SheetTypeIdentifierId] ASC),
    CONSTRAINT [FK_SheetIdentifierValues_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetIdentifierValues_SheetTypeIdentifiers_SheetTypeIdentifierId] FOREIGN KEY ([SheetTypeIdentifierId]) REFERENCES [setup].[SheetTypeIdentifiers] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetIdentifierValues_SheetTypeIdentifierId_Value]
    ON [spec].[SheetIdentifierValues]([SheetTypeIdentifierId] ASC, [Value] ASC);


GO

