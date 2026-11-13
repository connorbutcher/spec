<#
.SYNOPSIS
    Builds this project and deploys it to a database, creating the database if it doesn't exist.

.DESCRIPTION
    This project is the source of truth for the PUSpecSheet schema (database first). The script builds the
    dacpac and publishes it with SqlPackage, which works out and runs the changes needed to make the database
    match the project, then runs Script.PostDeployment.sql to add the reference data. A change that would lose
    data (dropping a column that holds values, say) stops the deployment instead.

.EXAMPLE
    ./deploy.ps1

.EXAMPLE
    ./deploy.ps1 -Server 'np:\\.\pipe\LOCALDB#1234ABCD\tsql\query' -Database PUSpecSheet
#>
[CmdletBinding()]
param(
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'PUSpecSheet'
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet tool restore failed.'
    }

    dotnet build --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        throw 'Database project build failed.'
    }

    dotnet sqlpackage /Action:Publish /SourceFile:bin/Debug/PUSpecSheet.Database.dacpac "/TargetServerName:$Server" "/TargetDatabaseName:$Database" /TargetTrustServerCertificate:True /p:BlockOnPossibleDataLoss=true
    if ($LASTEXITCODE -ne 0) {
        throw 'SqlPackage publish failed.'
    }

    Write-Host "Deployed the project to $Database."
}
finally {
    Pop-Location
}
