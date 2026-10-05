CREATE TABLE [dbo].[Users] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [UserName]     NVARCHAR (256) NOT NULL,
    [DisplayName]  NVARCHAR (200) NOT NULL,
    [Email]        NVARCHAR (256) NULL,
    [IsActive]     BIT            NOT NULL,
    [CreatedAtUtc] DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_UserName]
    ON [dbo].[Users]([UserName] ASC);


GO

