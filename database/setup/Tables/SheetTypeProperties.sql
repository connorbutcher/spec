CREATE TABLE [setup].[SheetTypeProperties] (
    [Id]                     INT              IDENTITY (1, 1) NOT NULL,
    [PublicId]               UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [SheetTypeId]            INT              NOT NULL,
    [Key]                    NVARCHAR (256)   NOT NULL,
    [Name]                   NVARCHAR (256)   NOT NULL,
    [ValueType]              NVARCHAR (64)    NOT NULL,
    [ControlType]            NVARCHAR (64)    NOT NULL,
    [CanBeSetInForm]         BIT              DEFAULT ((1)) NOT NULL,
    [SaveValueAsProperty]    BIT              DEFAULT ((1)) NOT NULL,
    [DisplayOrder]           INT              NOT NULL,
    [Placeholder]            NVARCHAR (512)   NULL,
    [ValidatorConfiguration] NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_SheetTypeProperties] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SheetTypeProperties_SheetTypes_SheetTypeId] FOREIGN KEY ([SheetTypeId]) REFERENCES [setup].[SheetTypes] ([Id]) ON DELETE CASCADE
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetTypeProperties_PublicId]
    ON [setup].[SheetTypeProperties]([PublicId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetTypeProperties_SheetTypeId_Key]
    ON [setup].[SheetTypeProperties]([SheetTypeId] ASC, [Key] ASC);


GO
