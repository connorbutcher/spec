CREATE TABLE [legacy].[WorkflowSteps] (
    [WorkflowStepID]  INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetID]     INT NOT NULL,
    [StartedDateTime] DATETIME2 (0) NOT NULL,
    [State]           TINYINT NOT NULL,
    CONSTRAINT [PK_WorkflowSteps] PRIMARY KEY CLUSTERED ([WorkflowStepID] ASC),
    CONSTRAINT [FK_WorkflowSteps_SpecSheets] FOREIGN KEY ([SpecSheetID]) REFERENCES [legacy].[SpecSheets] ([SpecSheetID])
);

GO
CREATE NONCLUSTERED INDEX [IX_WorkflowSteps_SpecSheetID]
    ON [legacy].[WorkflowSteps]([SpecSheetID] ASC);

