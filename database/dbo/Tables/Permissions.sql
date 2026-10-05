CREATE TABLE [dbo].[Permissions] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Key]         NVARCHAR (100) NOT NULL,
    [Description] NVARCHAR (500) NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Permissions_Key]
    ON [dbo].[Permissions]([Key] ASC);


GO

