CREATE TABLE [dbo].[CellTypes] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (100) NOT NULL,
    [Kind]          NVARCHAR (20)  NOT NULL,
    [Description]   NVARCHAR (500) NULL,
    [DisplayOrder]  INT            NOT NULL,
    [Configuration] NVARCHAR (MAX) DEFAULT (N'') NOT NULL,
    [Style]         NVARCHAR (MAX) DEFAULT (N'') NOT NULL,
    CONSTRAINT [PK_CellTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_CellTypes_Name]
    ON [dbo].[CellTypes]([Name] ASC);


GO

