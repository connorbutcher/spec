CREATE TABLE [spec].[SheetTableNotes] (
    [SheetTableNoteID] INT           IDENTITY (1, 1) NOT NULL,
    [SheetTableID]     INT           NOT NULL,
    [SourceSystem]     VARCHAR (16)  NOT NULL,
    [CreatedByUserID]  SMALLINT      NOT NULL,
    [CreatedDateTime]  DATETIME2 (7) CONSTRAINT [DF_SheetTableNotes_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetTableNotes] PRIMARY KEY CLUSTERED ([SheetTableNoteID] ASC),
    CONSTRAINT [FK_SheetTableNotes_SheetTables_SheetTableID] FOREIGN KEY ([SheetTableID]) REFERENCES [spec].[SheetTables] ([SheetTableID]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableNotes_SheetTableID]
    ON [spec].[SheetTableNotes]([SheetTableID] ASC);
