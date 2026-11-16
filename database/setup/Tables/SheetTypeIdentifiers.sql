CREATE TABLE [setup].[SheetTypeIdentifiers] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [SheetTypeId]  INT            NOT NULL,
    [Key]          NVARCHAR (50)  NOT NULL,
    [Name]         NVARCHAR (100) NOT NULL,
    [Kind]         INT            NOT NULL,
    [IsRequired]   BIT            NOT NULL,
    [DisplayOrder] INT            NOT NULL,
    [Options]      NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_SheetTypeIdentifiers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetTypeIdentifiers_Kind] CHECK ([Kind]>=(0) AND [Kind]<=(3)),
    CONSTRAINT [CK_SheetTypeIdentifiers_Options] CHECK ([Options] IS NULL OR isjson([Options])=(1)),
    CONSTRAINT [FK_SheetTypeIdentifiers_SheetTypes_SheetTypeId] FOREIGN KEY ([SheetTypeId]) REFERENCES [setup].[SheetTypes] ([Id]) ON DELETE CASCADE
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetTypeIdentifiers_SheetTypeId_Key]
    ON [setup].[SheetTypeIdentifiers]([SheetTypeId] ASC, [Key] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetTypeIdentifiers_SheetTypeId_Name]
    ON [setup].[SheetTypeIdentifiers]([SheetTypeId] ASC, [Name] ASC);


GO

