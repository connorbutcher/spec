CREATE TABLE [access].[SpecSheetTablePermissions] (
    [SpecSheetTablePermissionID] INT IDENTITY (1, 1) NOT NULL,
    [UserID]                     SMALLINT NOT NULL,
    [SpecSheetTableReferenceID]  UNIQUEIDENTIFIER NOT NULL,
    [IsAdmin]                    BIT CONSTRAINT [DF_SpecSheetTablePermissions_IsAdmin] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_SpecSheetTablePermissions] PRIMARY KEY CLUSTERED ([SpecSheetTablePermissionID] ASC),
    CONSTRAINT [FK_SpecSheetTablePermissions_Users] FOREIGN KEY ([UserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTablePermissions_UserID]
    ON [access].[SpecSheetTablePermissions]([UserID] ASC);

