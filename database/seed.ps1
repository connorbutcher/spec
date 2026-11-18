<#
.SYNOPSIS
    Seeds the reference data an empty database needs to be usable: an admin user, a phase, a sheet type, cell and
    widget types and one table template.

.DESCRIPTION
    Runs seed.sql with sqlcmd. The script only inserts what is missing, so it is safe to run again, and against a
    database that already has some of the data. It touches the reference tables only (access, types, template,
    configuration); sheets, values and revisions stay empty.

    Auth is disabled for local running, so the API treats every request as its default user (DefaultUser in
    api/src/PUSpecSheet.Api.Common/Claims). The admin defaults to that same user, so seeding needs no Entra object id.
    With auth turned back on the API finds the signed-in user by their Entra ID object id (the token's oid claim)
    instead; then pass your own as -AdminObjectId, with -AdminName and -AdminEmail, for the admin permissions to apply
    to you.

    Uses Windows authentication.

.EXAMPLE
    ./seed.ps1 -Database PUSpecSheetLegacy

.EXAMPLE
    ./seed.ps1 -Database PUSpecSheetLegacy -AdminObjectId 00000000-0000-0000-0000-000000000000 -AdminName 'Jo Bloggs' -AdminEmail jo.bloggs@example.com

.EXAMPLE
    ./seed.ps1 -Server 'np:\\.\pipe\LOCALDB#1234ABCD\tsql\query' -Database PUSpecSheetScratch
#>
[CmdletBinding()]
param(
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'PUSpecSheetLegacy',
    # The API's default user while auth is disabled (DefaultUser); keep them in step.
    [guid]$AdminObjectId = '00000000-0000-0000-0000-000000000001',
    [string]$AdminName = 'Local Admin',
    [string]$AdminEmail = 'local.admin@localhost'
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
        throw 'sqlcmd was not found. Install the SQL Server command line tools (or run seed.sql in SSMS in SQLCMD mode).'
    }

    # sqlcmd takes scripting variables from environment variables, which avoids Windows PowerShell's quoting of native
    # arguments that hold spaces or quotes. seed.sql puts the values inside quoted literals, so a quote is doubled.
    $env:AdminObjectID = $AdminObjectId.ToString()
    $env:AdminDisplayName = $AdminName.Replace("'", "''")
    $env:AdminEmail = $AdminEmail.Replace("'", "''")
    try {
        # -b stops on the first error with a non-zero exit code; -I turns on QUOTED_IDENTIFIER.
        sqlcmd -S $Server -d $Database -E -b -I -i seed.sql
        if ($LASTEXITCODE -ne 0) {
            throw 'Seeding failed.'
        }
    }
    finally {
        Remove-Item Env:AdminObjectID, Env:AdminDisplayName, Env:AdminEmail -ErrorAction SilentlyContinue
    }

    Write-Host "Seeded $Database."
}
finally {
    Pop-Location
}
