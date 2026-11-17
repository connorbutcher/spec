CREATE TABLE [cell].[LinkedSheetValues] (
    [SheetRowRevisionId]  INT              NOT NULL,
    [SheetCellId]         INT              NOT NULL,
    [LinkedSheetTypeId]   INT              NULL,
    [LinkedPhaseId]       INT              NULL,
    [LinkedTablePublicId] UNIQUEIDENTIFIER NULL,
    [LinkedItemPublicId]  UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_LinkedSheetValues] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC),
    CONSTRAINT [FK_LinkedSheetValues_Phases_LinkedPhaseId] FOREIGN KEY ([LinkedPhaseId]) REFERENCES [setup].[Phases] ([Id]),
    CONSTRAINT [FK_LinkedSheetValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [spec].[SheetCells] ([Id]),
    CONSTRAINT [FK_LinkedSheetValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [spec].[SheetRowRevisions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LinkedSheetValues_SheetTypes_LinkedSheetTypeId] FOREIGN KEY ([LinkedSheetTypeId]) REFERENCES [setup].[SheetTypes] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_LinkedSheetValues_SheetCellId]
    ON [cell].[LinkedSheetValues]([SheetCellId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_LinkedSheetValues_LinkedPhaseId]
    ON [cell].[LinkedSheetValues]([LinkedPhaseId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_LinkedSheetValues_LinkedSheetTypeId]
    ON [cell].[LinkedSheetValues]([LinkedSheetTypeId] ASC);


GO
