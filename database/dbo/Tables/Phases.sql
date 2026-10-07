CREATE TABLE [dbo].[Phases] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Code]          NVARCHAR (50)  NOT NULL,
    [Description]   NVARCHAR (500) NULL,
    [DisplayOrder]  INT            NOT NULL,
    [ParentPhaseId] INT            NULL,
    CONSTRAINT [PK_Phases] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_Phases_NotItsOwnParent] CHECK ([ParentPhaseId]<>[Id]),
    CONSTRAINT [FK_Phases_Phases_ParentPhaseId] FOREIGN KEY ([ParentPhaseId]) REFERENCES [dbo].[Phases] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_Phases_ParentPhaseId]
    ON [dbo].[Phases]([ParentPhaseId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Phases_Code]
    ON [dbo].[Phases]([Code] ASC);


GO

