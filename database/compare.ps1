<#
.SYNOPSIS
    Builds this project and lists what deploying it would change in a database.

.DESCRIPTION
    Runs a SqlPackage deploy report (nothing is changed in the database) from the
    built dacpac to the database, including objects that exist only in the
    database, and lists every difference. Database options, users and
    permissions are left out, as extract.ps1 leaves them out of the project.
    Exits with 1 when there are differences.

.EXAMPLE
    ./compare.ps1
#>
[CmdletBinding()]
param(
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'PUSpecSheet',
    # Defaults to Windows authentication. Pass a full connection string for anything else.
    [string]$ConnectionString = ''
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $report = Join-Path ([System.IO.Path]::GetTempPath()) "pu-spec-sheet-compare-$PID.xml"
    if ($ConnectionString -eq '') {
        $ConnectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"
    }

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet tool restore failed.'
    }

    dotnet build --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        throw 'Database project build failed.'
    }

    # Windows PowerShell turns a native command's error output into a terminating error under 'Stop', which would hide
    # all but the first line of SqlPackage's message; the exit code is checked below instead.
    $ErrorActionPreference = 'Continue'
    dotnet sqlpackage /Action:DeployReport /SourceFile:bin/Debug/PUSpecSheet.Database.dacpac "/TargetConnectionString:$ConnectionString" "/OutputPath:$report" /p:DropObjectsNotInSource=true /p:ScriptDatabaseOptions=false /p:ExcludeObjectTypes="Users;Logins;RoleMembership;Permissions" /Quiet:True
    $ErrorActionPreference = 'Stop'
    if ($LASTEXITCODE -ne 0) {
        throw 'SqlPackage deploy report failed.'
    }

    [xml]$xml = Get-Content $report
    Remove-Item $report

    $differences = @(
        foreach ($operation in $xml.DeploymentReport.Operations.Operation) {
            foreach ($item in $operation.Item) {
                '{0,-12} {1,-28} {2}' -f $operation.Name, $item.Type, $item.Value
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
}
finally {
    Pop-Location
}
