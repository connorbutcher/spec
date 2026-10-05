<#
.SYNOPSIS
    Regenerates the schema scripts in this project from an EF-migrated database.

.DESCRIPTION
    Runs a SqlPackage extract (read-only) into a temporary folder using the
    schema/object-type layout, drops the EF migrations history table from the
    result, and replaces the script folders of this project with it. Scripts are
    written exactly as SqlPackage emits them so repeat runs produce no diff
    unless the schema changed.

.EXAMPLE
    ./refresh.ps1

.EXAMPLE
    ./refresh.ps1 -Server 'np:\\.\pipe\LOCALDB#1234ABCD\tsql\query' -Database PUSpecSheet
#>
[CmdletBinding()]
param(
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'PUSpecSheet'
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {

    $staging = Join-Path ([System.IO.Path]::GetTempPath()) "pu-spec-sheet-extract-$PID"
    $connection = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True;ApplicationIntent=ReadOnly"

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet tool restore failed.'
    }

    dotnet sqlpackage /Action:Extract "/SourceConnectionString:$connection" "/TargetFile:$staging" /p:ExtractTarget=SchemaObjectType
    if ($LASTEXITCODE -ne 0) {
        throw 'SqlPackage extract failed.'
    }

    # The migrations history table belongs to EF, not to the schema this project documents.
    Get-ChildItem $staging -Recurse -Filter '__EFMigrationsHistory.sql' | Remove-Item

    # Replace every script folder so objects dropped by a migration disappear here too.
    Get-ChildItem $PSScriptRoot -Directory |
        Where-Object { $_.Name -notin 'bin', 'obj' } |
        Remove-Item -Recurse -Force

    Get-ChildItem $staging | Copy-Item -Destination $PSScriptRoot -Recurse
    Remove-Item $staging -Recurse -Force

    Write-Host "Schema scripts refreshed from $Database. Review with 'git status' and build with 'dotnet build'."
}
finally {
    Pop-Location
}
