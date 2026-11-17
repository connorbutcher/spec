CREATE TABLE [types].[SpecSheetTypePhases] (
    [SpecSheetTypeID]  TINYINT NOT NULL,
    [SpecSheetPhaseID] SMALLINT NOT NULL,
    CONSTRAINT [PK_SpecSheetTypePhases] PRIMARY KEY CLUSTERED ([SpecSheetTypeID] ASC, [SpecSheetPhaseID] ASC),
    CONSTRAINT [FK_SpecSheetTypePhases_SpecSheetTypes] FOREIGN KEY ([SpecSheetTypeID]) REFERENCES [types].[SpecSheetTypes] ([SpecSheetTypeID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SpecSheetTypePhases_SpecSheetPhases] FOREIGN KEY ([SpecSheetPhaseID]) REFERENCES [types].[SpecSheetPhases] ([SpecSheetPhaseID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTypePhases_SpecSheetPhaseID]
    ON [types].[SpecSheetTypePhases]([SpecSheetPhaseID] ASC);

