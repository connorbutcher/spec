<#
.SYNOPSIS
    Builds this project and deploys it to a database, creating the database if it doesn't exist.

.DESCRIPTION
    This project is the source of truth for the PUSpecSheet schema (database first). The script builds the
    dacpac and publishes it with SqlPackage, which works out and runs the changes needed to make the database
    match the project. Database options, users and permissions are left alone. A change that would lose data
    (dropping a column that holds values, say) stops the deployment instead.

    Use it for a local or test database. Check what it would do to a shared one with compare.ps1 first.

    With -Legacy it deploys the legacy project (legacy/) instead: the same scripts without the revision tables (spec,
    cell, approval), for a database shaped like the legacy one. Deploying without -Legacy afterwards adds them.

.EXAMPLE
    ./deploy.ps1

.EXAMPLE
    ./deploy.ps1 -Server 'np:\\.\pipe\LOCALDB#1234ABCD\tsql\query' -Database PUSpecSheet

.EXAMPLE
    ./deploy.ps1 -Database PUSpecSheetLegacy -Legacy
#>
[CmdletBinding()]
param(
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'PUSpecSheet',
    # Defaults to Windows authentication. Pass a full connection string for anything else.
    [string]$ConnectionString = '',
    # Deploys the legacy tables only, without the revision tables.
    [switch]$Legacy
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    if ($ConnectionString -eq '') {
        $ConnectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"
    }

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet tool restore failed.'
    }

    $projectFolder = '.'
    $dacpac = 'bin/Debug/PUSpecSheet.Database.dacpac'
    if ($Legacy) {
        $projectFolder = 'legacy'
        $dacpac = 'legacy/bin/Debug/PUSpecSheet.Legacy.Database.dacpac'
    }

    dotnet build $projectFolder --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        throw 'Database project build failed.'
    }

    # Windows PowerShell turns a native command's error output into a terminating error under 'Stop', which would hide
    # all but the first line of SqlPackage's message; the exit code is checked below instead.
    $ErrorActionPreference = 'Continue'
    dotnet sqlpackage /Action:Publish "/SourceFile:$dacpac" "/TargetConnectionString:$ConnectionString" /p:BlockOnPossibleDataLoss=true /p:ScriptDatabaseOptions=false /p:ExcludeObjectTypes="Users;Logins;RoleMembership;Permissions"
    $ErrorActionPreference = 'Stop'
    if ($LASTEXITCODE -ne 0) {
        throw 'SqlPackage publish failed.'
    }

    Write-Host "Deployed $dacpac to $Database."
}
finally {
    Pop-Location
}
