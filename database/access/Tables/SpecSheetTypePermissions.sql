CREATE TABLE [access].[SpecSheetTypePermissions] (
    [SpecSheetTypePermissionID] INT IDENTITY (1, 1) NOT NULL,
    [PermissionID]              SMALLINT NOT NULL,
    [SpecSheetTypeID]           TINYINT NOT NULL,
    CONSTRAINT [PK_SpecSheetTypePermissions] PRIMARY KEY CLUSTERED ([SpecSheetTypePermissionID] ASC),
    CONSTRAINT [FK_SpecSheetTypePermissions_Permissions] FOREIGN KEY ([PermissionID]) REFERENCES [access].[Permissions] ([PermissionID]),
    CONSTRAINT [FK_SpecSheetTypePermissions_SpecSheetTypes] FOREIGN KEY ([SpecSheetTypeID]) REFERENCES [types].[SpecSheetTypes] ([SpecSheetTypeID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTypePermissions_PermissionID]
    ON [access].[SpecSheetTypePermissions]([PermissionID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTypePermissions_SpecSheetTypeID]
    ON [access].[SpecSheetTypePermissions]([SpecSheetTypeID] ASC);

