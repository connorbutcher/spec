CREATE TABLE [access].[Permissions] (
    [PermissionID] SMALLINT IDENTITY (1, 1) NOT NULL,
    [Title]        VARCHAR (256) NOT NULL,
    [Description]  VARCHAR (512) NOT NULL,
    [Key]          VARCHAR (64) NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([PermissionID] ASC)
);

