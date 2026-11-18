CREATE TABLE [cell].[SheetTableNoteValues] (
    [SheetTableRevisionID] INT            NOT NULL,
    [SheetTableNoteID]     INT            NOT NULL,
    [Note]                 NVARCHAR (MAX) NOT NULL,
    [IsObsolete]           BIT            CONSTRAINT [DF_SheetTableNoteValues_IsObsolete] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_SheetTableNoteValues] PRIMARY KEY CLUSTERED ([SheetTableRevisionID] ASC, [SheetTableNoteID] ASC),
    CONSTRAINT [FK_SheetTableNoteValues_SheetTableRevisions_SheetTableRevisionID] FOREIGN KEY ([SheetTableRevisionID]) REFERENCES [spec].[SheetTableRevisions] ([SheetTableRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableNoteValues_SheetTableNotes_SheetTableNoteID] FOREIGN KEY ([SheetTableNoteID]) REFERENCES [spec].[SheetTableNotes] ([SheetTableNoteID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableNoteValues_SheetTableNoteID]
    ON [cell].[SheetTableNoteValues]([SheetTableNoteID] ASC);
