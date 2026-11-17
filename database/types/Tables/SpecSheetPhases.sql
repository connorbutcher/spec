CREATE TABLE [types].[SpecSheetPhases] (
    [SpecSheetPhaseID]       SMALLINT IDENTITY (1, 1) NOT NULL,
    [ParentSpecSheetPhaseID] SMALLINT NULL,
    [Phase]                  VARCHAR (64) NOT NULL,
    [PhaseShort]             VARCHAR (8) NOT NULL,
    [DisplayOrder]           SMALLINT NOT NULL,
    [IsObsolete]             BIT CONSTRAINT [DF_SpecSheetPhases_IsObsolete] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_SpecSheetPhases] PRIMARY KEY CLUSTERED ([SpecSheetPhaseID] ASC),
    CONSTRAINT [FK_SpecSheetPhases_SpecSheetPhases_ParentSpecSheetPhaseID] FOREIGN KEY ([ParentSpecSheetPhaseID]) REFERENCES [types].[SpecSheetPhases] ([SpecSheetPhaseID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetPhases_ParentSpecSheetPhaseID]
    ON [types].[SpecSheetPhases]([ParentSpecSheetPhaseID] ASC);

