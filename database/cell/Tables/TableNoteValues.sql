CREATE TABLE [cell].[TableNoteValues] (
    [SheetTableRevisionID] INT            NOT NULL,
    [SheetTableNoteID]     INT            NOT NULL,
    [Note]                 NVARCHAR (MAX) NOT NULL,
    [IsObsolete]           BIT            CONSTRAINT [DF_TableNoteValues_IsObsolete] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TableNoteValues] PRIMARY KEY CLUSTERED ([SheetTableRevisionID] ASC, [SheetTableNoteID] ASC),
    CONSTRAINT [FK_TableNoteValues_SheetTableRevisions_SheetTableRevisionID] FOREIGN KEY ([SheetTableRevisionID]) REFERENCES [spec].[SheetTableRevisions] ([SheetTableRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_TableNoteValues_SheetTableNotes_SheetTableNoteID] FOREIGN KEY ([SheetTableNoteID]) REFERENCES [spec].[SheetTableNotes] ([SheetTableNoteID])
);

GO
CREATE NONCLUSTERED INDEX [IX_TableNoteValues_SheetTableNoteID]
    ON [cell].[TableNoteValues]([SheetTableNoteID] ASC);
