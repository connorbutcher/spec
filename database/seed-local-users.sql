/*
    Adds the extra local users the API can act as while auth is disabled (LocalUsers in
    api/src/PUSpecSheet.Api.Common/Claims), so checkouts and approvals can be tried with more than one user. Run it
    after seed.sql:

        sqlcmd -S "(localdb)\MSSQLLocalDB" -d PUSpecSheetLegacy -E -b -I -i seed-local-users.sql

    Local Engineer and Local Approver get the default user's (Local Admin's) permissions, and all three are made
    approvers of every phase so each can approve the others' changes. Data only, and idempotent: rows are inserted when
    missing and nothing is updated or deleted. Keep the object ids in step with LocalUsers.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRANSACTION;

DECLARE @LocalUsers TABLE ([ObjectID] UNIQUEIDENTIFIER NOT NULL, [DisplayName] NVARCHAR (256) NOT NULL, [EmailAddress] NVARCHAR (256) NOT NULL);
INSERT INTO @LocalUsers ([ObjectID], [DisplayName], [EmailAddress])
VALUES
    ('00000000-0000-0000-0000-000000000001', N'Local Admin', N'local.admin@localhost'),
    ('00000000-0000-0000-0000-000000000002', N'Local Engineer', N'local.engineer@localhost'),
    ('00000000-0000-0000-0000-000000000003', N'Local Approver', N'local.approver@localhost');

INSERT INTO [access].[Users] ([ObjectID], [DisplayName], [EmailAddress])
SELECT u.[ObjectID], u.[DisplayName], u.[EmailAddress]
FROM @LocalUsers AS u
WHERE NOT EXISTS (SELECT 1 FROM [access].[Users] AS e WHERE e.[ObjectID] = u.[ObjectID]);

DECLARE @AdminUserID SMALLINT = (SELECT [UserID] FROM [access].[Users] WHERE [ObjectID] = '00000000-0000-0000-0000-000000000001');

INSERT INTO [access].[UserPermissions] ([UserID], [PermissionID])
SELECT users.[UserID], admin.[PermissionID]
FROM [access].[Users] AS users
CROSS JOIN [access].[UserPermissions] AS admin
WHERE admin.[UserID] = @AdminUserID
  AND users.[ObjectID] IN (SELECT [ObjectID] FROM @LocalUsers)
  AND users.[UserID] <> @AdminUserID
  AND NOT EXISTS (SELECT 1 FROM [access].[UserPermissions] AS e WHERE e.[UserID] = users.[UserID] AND e.[PermissionID] = admin.[PermissionID]);

INSERT INTO [access].[SpecSheetPhaseObservers] ([SpecSheetPhaseID], [UserID], [IsApprover])
SELECT phases.[SpecSheetPhaseID], users.[UserID], 1
FROM [types].[SpecSheetPhases] AS phases
CROSS JOIN [access].[Users] AS users
WHERE users.[ObjectID] IN (SELECT [ObjectID] FROM @LocalUsers)
  AND NOT EXISTS (SELECT 1 FROM [access].[SpecSheetPhaseObservers] AS e WHERE e.[SpecSheetPhaseID] = phases.[SpecSheetPhaseID] AND e.[UserID] = users.[UserID]);

COMMIT TRANSACTION;

PRINT 'Seeded the local users.';
