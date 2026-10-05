<#
.SYNOPSIS
    Builds this project and compares it with an EF-migrated database.

.DESCRIPTION
    Runs a SqlPackage deploy report (nothing is changed in the database) from the
    built dacpac to the database, including objects that exist only in the
    database, and lists every difference. The EF migrations history table is left
    out of the project on purpose, so it is ignored here. Exits with 1 when there
    are differences.

.EXAMPLE
    ./compare.ps1
#>
[CmdletBinding()]
param(
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'PUSpecSheet'
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$report = Join-Path ([System.IO.Path]::GetTempPath()) "pu-spec-sheet-compare-$PID.xml"
$connection = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"

dotnet tool restore
if ($LASTEXITCODE -ne 0) {
    throw 'dotnet tool restore failed.'
}

dotnet build --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    throw 'Database project build failed.'
}

dotnet sqlpackage /Action:DeployReport /SourceFile:bin/Debug/PUSpecSheet.Database.dacpac "/TargetConnectionString:$connection" "/OutputPath:$report" /p:DropObjectsNotInSource=true /Quiet:True
if ($LASTEXITCODE -ne 0) {
    throw 'SqlPackage deploy report failed.'
}

[xml]$xml = Get-Content $report
Remove-Item $report

$differences = @(
    foreach ($operation in $xml.DeploymentReport.Operations.Operation) {
        foreach ($item in $operation.Item) {
            if ($item.Value -ne '[dbo].[__EFMigrationsHistory]') {
                '{0,-12} {1,-28} {2}' -f $operation.Name, $item.Type, $item.Value
            }
        }
    }
)

if ($differences.Count -eq 0) {
    Write-Host "No differences between the project and $Database."
    exit 0
}

Write-Host "Differences (operation needed to make $Database match the project):"
$differences | ForEach-Object { Write-Host $_ }
exit 1
