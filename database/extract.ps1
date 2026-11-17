<#
.SYNOPSIS
    Writes the schema of a database into this project, one script per object.

.DESCRIPTION
    Runs a SqlPackage extract of the database's schema (no data, no users or permissions) and puts each object's
    script under <schema>/<object type>/<name>.sql, replacing what is there. Review the changes with git before
    committing them: they are the differences between the database and the project.

.EXAMPLE
    ./extract.ps1 -Server 'myserver.database.windows.net' -Database PUSpecSheet
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
    if ($ConnectionString -eq '') {
        $ConnectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"
    }

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet tool restore failed.'
    }

    $output = Join-Path ([System.IO.Path]::GetTempPath()) "pu-spec-sheet-extract-$PID"

    # Windows PowerShell turns a native command's error output into a terminating error under 'Stop', which would hide
    # all but the first line of SqlPackage's message; the exit code is checked below instead.
    $ErrorActionPreference = 'Continue'
    dotnet sqlpackage /Action:Extract "/SourceConnectionString:$ConnectionString" "/TargetFile:$output" /p:ExtractTarget=SchemaObjectType /p:IgnorePermissions=true /p:IgnoreUserLoginMappings=true /p:ExtractApplicationScopedObjectsOnly=true
    $ErrorActionPreference = 'Stop'
    if ($LASTEXITCODE -ne 0) {
        throw 'SqlPackage extract failed.'
    }

    # Replace every schema folder the extract wrote, so an object dropped from the database goes from the project too.
    foreach ($folder in Get-ChildItem $output -Directory) {
        if (Test-Path $folder.Name) {
            Remove-Item $folder.Name -Recurse -Force
        }

        Move-Item $folder.FullName $folder.Name
    }

    Remove-Item $output -Recurse -Force
    Write-Host "Extracted $Database into the project. Review the changes with git before committing them."
}
finally {
    Pop-Location
}
