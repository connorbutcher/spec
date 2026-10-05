CREATE TABLE [dbo].[PhaseSheetTypes] (
    [PhaseId]     INT NOT NULL,
    [SheetTypeId] INT NOT NULL,
    CONSTRAINT [PK_PhaseSheetTypes] PRIMARY KEY CLUSTERED ([PhaseId] ASC, [SheetTypeId] ASC),
    CONSTRAINT [FK_PhaseSheetTypes_Phases_PhaseId] FOREIGN KEY ([PhaseId]) REFERENCES [dbo].[Phases] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PhaseSheetTypes_SheetTypes_SheetTypeId] FOREIGN KEY ([SheetTypeId]) REFERENCES [dbo].[SheetTypes] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_PhaseSheetTypes_SheetTypeId]
    ON [dbo].[PhaseSheetTypes]([SheetTypeId] ASC);


GO

