CREATE TABLE [access].[UserPermissions] (
    [UserPermissionID] INT IDENTITY (1, 1) NOT NULL,
    [UserID]           SMALLINT NOT NULL,
    [PermissionID]     SMALLINT NOT NULL,
    CONSTRAINT [PK_UserPermissions] PRIMARY KEY CLUSTERED ([UserPermissionID] ASC),
    CONSTRAINT [FK_UserPermissions_Users] FOREIGN KEY ([UserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_UserPermissions_Permissions] FOREIGN KEY ([PermissionID]) REFERENCES [access].[Permissions] ([PermissionID])
);

GO
CREATE NONCLUSTERED INDEX [IX_UserPermissions_UserID]
    ON [access].[UserPermissions]([UserID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_UserPermissions_PermissionID]
    ON [access].[UserPermissions]([PermissionID] ASC);

