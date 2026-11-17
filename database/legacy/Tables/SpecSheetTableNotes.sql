CREATE TABLE [legacy].[SpecSheetTableNotes] (
    [SpecSheetTableNoteID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableID]     INT NOT NULL,
    [Note]                 NVARCHAR (MAX) NOT NULL,
    [SourceSystem]         VARCHAR (16) NOT NULL,
    [IsObsolete]           BIT NOT NULL,
    [CreatedByUserID]      SMALLINT NOT NULL,
    [CreatedDateTime]      DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_SpecSheetTableNotes] PRIMARY KEY CLUSTERED ([SpecSheetTableNoteID] ASC),
    CONSTRAINT [FK_SpecSheetTableNotes_SpecSheetTables] FOREIGN KEY ([SpecSheetTableID]) REFERENCES [legacy].[SpecSheetTables] ([SpecSheetTableID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableNotes_SpecSheetTableID]
    ON [legacy].[SpecSheetTableNotes]([SpecSheetTableID] ASC);

