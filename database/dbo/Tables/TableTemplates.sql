CREATE TABLE [dbo].[TableTemplates] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [SheetTypeId]  INT            NOT NULL,
    [Name]         NVARCHAR (100) NOT NULL,
    [DisplayOrder] INT            NOT NULL,
    CONSTRAINT [PK_TableTemplates] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TableTemplates_SheetTypes_SheetTypeId] FOREIGN KEY ([SheetTypeId]) REFERENCES [dbo].[SheetTypes] ([Id])
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_TableTemplates_SheetTypeId_Name]
    ON [dbo].[TableTemplates]([SheetTypeId] ASC, [Name] ASC);


GO

