CREATE TABLE [dbo].[SheetTypes] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (100) NOT NULL,
    [DisplayOrder] INT            NOT NULL,
    CONSTRAINT [PK_SheetTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetTypes_Name]
    ON [dbo].[SheetTypes]([Name] ASC);


GO

