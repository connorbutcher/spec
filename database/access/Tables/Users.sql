CREATE TABLE [access].[Users] (
    [UserID]       SMALLINT IDENTITY (1, 1) NOT NULL,
    [ObjectID]     UNIQUEIDENTIFIER NOT NULL,
    [DisplayName]  VARCHAR (256) NOT NULL,
    [EmailAddress] NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

