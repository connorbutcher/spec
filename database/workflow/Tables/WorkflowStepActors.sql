CREATE TABLE [workflow].[WorkflowStepActors] (
    [WorkflowStepActorID] INT IDENTITY (1, 1) NOT NULL,
    [WorkflowStepID]      INT NOT NULL,
    [ActorUserID]         SMALLINT NULL,
    [Comment]             NVARCHAR (MAX) NULL,
    [State]               TINYINT NOT NULL,
    [ActionedDateTime]    DATETIME2 (0) NULL,
    CONSTRAINT [PK_WorkflowStepActors] PRIMARY KEY CLUSTERED ([WorkflowStepActorID] ASC),
    CONSTRAINT [FK_WorkflowStepActors_WorkflowSteps] FOREIGN KEY ([WorkflowStepID]) REFERENCES [workflow].[WorkflowSteps] ([WorkflowStepID]),
    CONSTRAINT [FK_WorkflowStepActors_Users] FOREIGN KEY ([ActorUserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_WorkflowStepActors_WorkflowStepID]
    ON [workflow].[WorkflowStepActors]([WorkflowStepID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_WorkflowStepActors_ActorUserID]
    ON [workflow].[WorkflowStepActors]([ActorUserID] ASC);

