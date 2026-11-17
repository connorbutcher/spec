<#
.SYNOPSIS
    Copies a whole database, schema and data, into a new database.

.DESCRIPTION
    Exports the source database to a bacpac with SqlPackage, then imports the bacpac as the target database. The
    copy has every schema, table, constraint, index and row of the source, so it is the way to set up a database
    identical to another one: a copy of the legacy database to migrate, or a second copy of a local one to try
    something on. The source is only read. The target must not exist yet; SqlPackage refuses to import into a
    database that has objects in it.

    Users, logins and permissions travel with the bacpac only as far as SqlPackage takes them; on LocalDB, Windows
    authentication needs none.

.EXAMPLE
    ./copy.ps1 -SourceDatabase PUSpecSheet -TargetDatabase PUSpecSheetCopy

.EXAMPLE
    ./copy.ps1 -SourceConnectionString 'Server=test-sql;Database=PUSpecSheet;Trusted_Connection=True;TrustServerCertificate=True' -TargetDatabase PUSpecSheetLegacy
#>
[CmdletBinding()]
param(
    [string]$SourceServer = '(localdb)\MSSQLLocalDB',
    [string]$SourceDatabase = 'PUSpecSheet',
    [string]$TargetServer = '(localdb)\MSSQLLocalDB',
    [Parameter(Mandatory = $true)]
    [string]$TargetDatabase,
    # Defaults to Windows authentication. Pass full connection strings for anything else.
    [string]$SourceConnectionString = '',
    [string]$TargetConnectionString = '',
    # Where the bacpac is written. Defaults to a temporary file, deleted afterwards.
    [string]$BacpacPath = ''
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
$keepBacpac = $BacpacPath -ne ''
try {
    if ($SourceConnectionString -eq '') {
        $SourceConnectionString = "Server=$SourceServer;Database=$SourceDatabase;Trusted_Connection=True;TrustServerCertificate=True"
    }

    if ($TargetConnectionString -eq '') {
        $TargetConnectionString = "Server=$TargetServer;Database=$TargetDatabase;Trusted_Connection=True;TrustServerCertificate=True"
    }

    if (-not $keepBacpac) {
        $BacpacPath = Join-Path ([System.IO.Path]::GetTempPath()) "pu-spec-sheet-copy-$PID.bacpac"
    }

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet tool restore failed.'
    }

    # Windows PowerShell turns a native command's error output into a terminating error under 'Stop', which would hide
    # all but the first line of SqlPackage's message; the exit codes are checked below instead.
    $ErrorActionPreference = 'Continue'
    dotnet sqlpackage /Action:Export "/SourceConnectionString:$SourceConnectionString" "/TargetFile:$BacpacPath"
    $exportExitCode = $LASTEXITCODE
    $ErrorActionPreference = 'Stop'
    if ($exportExitCode -ne 0) {
        throw 'SqlPackage export failed.'
    }

    $ErrorActionPreference = 'Continue'
    dotnet sqlpackage /Action:Import "/SourceFile:$BacpacPath" "/TargetConnectionString:$TargetConnectionString"
    $importExitCode = $LASTEXITCODE
    $ErrorActionPreference = 'Stop'
    if ($importExitCode -ne 0) {
        throw 'SqlPackage import failed.'
    }

    Write-Host "Copied the database into $TargetDatabase."
}
finally {
    if (-not $keepBacpac -and $BacpacPath -ne '' -and (Test-Path $BacpacPath)) {
        Remove-Item $BacpacPath
    }

    Pop-Location
}
