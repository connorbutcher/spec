CREATE TABLE [access].[SpecSheetPhaseObservers] (
    [SpecSheetPhaseObserverID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetPhaseID]         SMALLINT NOT NULL,
    [UserID]                   SMALLINT NOT NULL,
    [IsApprover]               BIT NOT NULL,
    CONSTRAINT [PK_SpecSheetPhaseObservers] PRIMARY KEY CLUSTERED ([SpecSheetPhaseObserverID] ASC),
    CONSTRAINT [FK_SpecSheetPhaseObservers_SpecSheetPhases] FOREIGN KEY ([SpecSheetPhaseID]) REFERENCES [types].[SpecSheetPhases] ([SpecSheetPhaseID]),
    CONSTRAINT [FK_SpecSheetPhaseObservers_Users] FOREIGN KEY ([UserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetPhaseObservers_SpecSheetPhaseID]
    ON [access].[SpecSheetPhaseObservers]([SpecSheetPhaseID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetPhaseObservers_UserID]
    ON [access].[SpecSheetPhaseObservers]([UserID] ASC);

