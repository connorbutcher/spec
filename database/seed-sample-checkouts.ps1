<#
.SYNOPSIS
    Seeds a sample sheet with several published versions, changes awaiting approval, a rejected submission and open
    checkouts by different users, for trying out row and table checkout.

.DESCRIPTION
    Drives the running API as the three local users (the X-Local-User header, see LocalUsers in
    api/src/PUSpecSheet.Api.Common/Claims), so the data goes through the same code as the screens. Run seed.ps1 and
    seed-local-users.sql first, and have the API running (auth disabled, as for local running).

    For the phase and sheet type given it creates the sheet with two tables, then:

        v1  Local Admin's first version, approved by Local Approver.
        v2  Local Engineer's changes to the first table, approved by Local Admin.
        v3  Local Admin's rename of the second table and change to one of its rows, approved by Local Approver.

    and leaves the sheet with:

        - Local Approver's change to a row of the second table, awaiting Local Admin's approval.
        - Local Admin's change to the first row of the first table, rejected by Local Approver and so checked out to
          Local Admin again (with the rejection reason in the history).
        - Local Engineer's open checkouts: a changed row and a row they added to the first table.

    The sheet type should need approval (as the seeded Engine Specification does); for one that doesn't, each submit
    publishes straight away, so every change becomes a version and nothing is left pending.

    A sheet that already has a published version is left alone: pass another phase or sheet type. A sheet with no
    published version yet is cleared first (each local user's checkouts are undone) and made again.

.EXAMPLE
    ./seed-sample-checkouts.ps1

.EXAMPLE
    ./seed-sample-checkouts.ps1 -ApiUrl https://localhost:8888 -PhaseId 1 -SheetTypeId 1
#>
[CmdletBinding()]
param(
    [string]$ApiUrl = 'https://localhost:8888',
    [int]$PhaseId = 1,
    [int]$SheetTypeId = 1
)

$ErrorActionPreference = 'Stop'

# The local users the API acts as, by the number sent in X-Local-User (keep in step with LocalUsers).
$Admin = 1
$Engineer = 2
$Approver = 3

function Invoke-Api {
    param([int]$User, [string]$Method, [string]$Path, $Body = $null)

    $headers = @{ 'X-Local-User' = "$User" }
    $arguments = @{ Uri = "$ApiUrl$Path"; Method = $Method; Headers = $headers; UseBasicParsing = $true }
    if ($null -ne $Body) {
        $arguments.ContentType = 'application/json'
        $arguments.Body = [Text.Encoding]::UTF8.GetBytes(($Body | ConvertTo-Json -Depth 20))
    }

    try {
        $response = Invoke-WebRequest @arguments
    }
    catch {
        $detail = $_.Exception.Message
        if ($_.Exception.Response) {
            $reader = New-Object IO.StreamReader($_.Exception.Response.GetResponseStream())
            $text = $reader.ReadToEnd()
            try { $detail = ($text | ConvertFrom-Json).detail } catch { $detail = $text }
        }

        throw "$Method $Path as local user $User failed: $detail"
    }

    if ($response.Content) {
        return $response.Content | ConvertFrom-Json
    }

    return $null
}

function Get-Sheet {
    param([int]$SheetId)

    return Invoke-Api $Admin GET "/sheets/$SheetId/draft/detail"
}

# The table's rows in order, first group first: the rows the screens show for it.
function Get-Rows {
    param($Sheet, [string]$Title)

    $table = $Sheet.sheet.tables | Where-Object { $_.title -eq $Title } | Select-Object -First 1
    if (-not $table) {
        throw "Table '$Title' not found on the sheet."
    }

    return @($table.groups | Sort-Object displayOrder | ForEach-Object { $_.rows | Sort-Object displayOrder })
}

# The first cell of the row whose value is of the model type (text or numeric), or null.
function Get-Cell {
    param($Row, [string]$ModelType)

    return $Row.cells |
        Where-Object { $_.tableCellType.modelType -eq $ModelType } |
        Sort-Object columnNumber |
        Select-Object -First 1
}

function New-TextUpdate {
    param($Cell, [string]$Value)

    return [ordered]@{ '$type' = 'UpdateTextValueOp'; id = $Cell.id; value = @{ value = $Value; hasChanged = $true } }
}

function New-NumberUpdate {
    param($Cell, [double]$Value)

    return [ordered]@{ '$type' = 'UpdateNumberValueOp'; id = $Cell.id; value = @{ value = $Value; hasChanged = $true } }
}

function Save-Changes {
    param([int]$User, [int]$SheetId, [object[]]$Operations)

    Invoke-Api $User PATCH "/sheets/$SheetId/draft" @{ operations = $Operations } | Out-Null
}

function Submit-Changes {
    param([int]$User, [int]$SheetId, [string]$ChangeLog, [int[]]$ApproverIds)

    Invoke-Api $User PATCH "/sheets/$SheetId/publish" @{ changeLog = $ChangeLog; approverUserIds = $ApproverIds } | Out-Null

    $pending = (Get-Sheet $SheetId).submissions |
        Where-Object { $_.status -eq 0 -and $_.submittedByUserId -eq $UserIds[$User] } |
        Sort-Object id -Descending |
        Select-Object -First 1
    return $pending
}

function Approve-Submission {
    param([int]$User, [int]$SheetId, $Submission)

    Invoke-Api $User PATCH "/sheets/$SheetId/submissions/$($Submission.id)/decision" @{ decision = 1 } | Out-Null
}

function Reject-Submission {
    param([int]$User, [int]$SheetId, $Submission, [string]$Reason)

    Invoke-Api $User PATCH "/sheets/$SheetId/submissions/$($Submission.id)/decision" @{ decision = 2; comment = $Reason } | Out-Null
}

# Operations that add a table as the screens do: a group for each default group template, the later ones linked to
# the first, and the extra rows given as further copies of the last default group, with a cell for each template cell
# and the values given by column.
function New-TableOperations {
    param($TableTemplate, [string]$Title, [int]$DisplayOrder, [int]$RowCount, [scriptblock]$ValueOf)

    $script:nextTempId = $script:nextTempId - 1
    $tableTempId = $script:nextTempId
    $operations = @([ordered]@{
        '$type' = 'AddSpecSheetTableOp'; tempId = $tableTempId; tableTemplateId = $TableTemplate.id; title = $Title
        displayOrder = $DisplayOrder; referenceId = [guid]::NewGuid().ToString()
    })

    $defaultGroups = @($TableTemplate.tableGroupTemplates | Where-Object { $_.isDefault })
    if ($defaultGroups.Count -eq 0) {
        throw "Table template '$($TableTemplate.type)' has no default group."
    }

    # The first default group once (the header), then the last one once for each row asked for.
    $groups = @()
    if ($defaultGroups.Count -gt 1) {
        $groups += $defaultGroups[0]
    }

    for ($i = 0; $i -lt [Math]::Max(1, $RowCount); $i++) {
        $groups += $defaultGroups[-1]
    }

    $firstGroupTempId = $null
    $order = 0
    $rowIndex = 0
    foreach ($groupTemplate in $groups) {
        $order++
        $script:nextTempId = $script:nextTempId - 1
        $groupTempId = $script:nextTempId
        $groupOperation = [ordered]@{
            '$type' = 'AddSpecSheetTableGroupOp'; tempId = $groupTempId; specSheetTableId = $tableTempId
            tableGroupTemplateId = $groupTemplate.id; order = $order; referenceId = [guid]::NewGuid().ToString()
        }
        if ($null -ne $firstGroupTempId) {
            $groupOperation.linkedSpecSheetTableGroupId = $firstGroupTempId
        }
        else {
            $firstGroupTempId = $groupTempId
        }

        $operations += $groupOperation

        foreach ($rowTemplate in $groupTemplate.rowTemplates) {
            $script:nextTempId = $script:nextTempId - 1
            $rowTempId = $script:nextTempId
            $operations += [ordered]@{
                '$type' = 'AddSpecSheetTableRowOp'; tempId = $rowTempId; specSheetTableGroupId = $groupTempId
                tableRowTemplateId = $rowTemplate.id; rowNumber = $rowTemplate.rowNumber; referenceId = [guid]::NewGuid().ToString()
            }

            foreach ($cellTemplate in ($rowTemplate.cellTemplates | Sort-Object columnNumber)) {
                $script:nextTempId = $script:nextTempId - 1
                $cellTempId = $script:nextTempId
                $operations += [ordered]@{
                    '$type' = 'AddSpecSheetTableCellOp'; tempId = $cellTempId; specSheetTableRowId = $rowTempId
                    tableCellTypeId = $cellTemplate.tableCellTypeId; cellConfigurationId = $cellTemplate.cellConfigurationId
                    rowNumber = $rowTemplate.rowNumber; columnNumber = $cellTemplate.columnNumber
                    rowSpan = [Math]::Max(1, [int]$cellTemplate.rowSpan); colSpan = [Math]::Max(1, [int]$cellTemplate.colSpan)
                    referenceId = [guid]::NewGuid().ToString()
                }

                $value = & $ValueOf $rowIndex $cellTemplate
                switch ($cellTemplate.tableCellType.modelType) {
                    'text' {
                        if ($null -ne $value) {
                            $operations += [ordered]@{ '$type' = 'AddTextValueOp'; specSheetTableCellId = $cellTempId; value = "$value" }
                        }
                    }
                    'numeric' {
                        if ($null -ne $value) {
                            $operations += [ordered]@{ '$type' = 'AddNumberValueOp'; specSheetTableCellId = $cellTempId; value = [double]$value }
                        }
                    }
                }
            }

            $rowIndex++
        }
    }

    return $operations
}

# The values of a sample table, by row (0 is the header row) and column.
$limitValue = {
    param([int]$RowIndex, $CellTemplate)

    $headings = @('Feature', 'Nominal', 'Lower limit', 'Upper limit')
    $features = @('Cylinder bore', 'Crank journal', 'Cam journal', 'Piston pin', 'Valve guide')
    $column = [int]$CellTemplate.columnNumber
    if ($RowIndex -eq 0) {
        return $headings[[Math]::Min($column, $headings.Count) - 1]
    }

    switch ($CellTemplate.tableCellType.modelType) {
        'text' { return $features[($RowIndex - 1) % $features.Count] }
        'numeric' { return [Math]::Round(80 + $RowIndex * 5 + $column * 0.01, 3) }
        default { return $null }
    }
}

$script:nextTempId = 0

Write-Host "Reading the users and sheet type from $ApiUrl..."
$UserIds = @{}
foreach ($user in $Admin, $Engineer, $Approver) {
    $current = Invoke-Api $user GET '/users/current'
    $UserIds[$user] = $current.id
    Write-Host "  local user $user is $($current.displayName) (UserID $($current.id))"
}

$sheetType = Invoke-Api $Admin GET "/specsheettypes/$SheetTypeId"
$tableTemplate = $sheetType.tableTemplates | Where-Object { -not $_.isLegacy } | Select-Object -First 1
if (-not $tableTemplate) {
    throw "Sheet type '$($sheetType.type)' has no table template."
}

if (-not $sheetType.requiresApprovalWorkflow) {
    Write-Warning "Sheet type '$($sheetType.type)' doesn't need approval, so every submit publishes straight away and nothing is left pending."
}

# A sheet with a published version is never touched; one that has never been published is cleared and made again.
$existing = Invoke-Api $Admin GET "/sheets/phase/$PhaseId/all" | Where-Object { $_.specSheetTypeId -eq $SheetTypeId } | Select-Object -First 1
if ($existing) {
    if ($existing.latestVersion) {
        throw "The sheet for phase $PhaseId and type $SheetTypeId already has version $($existing.latestVersion.versionNumber). Pass another -PhaseId or -SheetTypeId."
    }

    Write-Host "Clearing the never-published sheet $($existing.id)..."
    foreach ($user in $Admin, $Engineer, $Approver) {
        Invoke-Api $user DELETE "/sheets/$($existing.id)/draft/mine" | Out-Null
    }
}

Write-Host 'Creating the sheet as Local Admin...'
$location = Invoke-Api $Admin POST '/sheets/draft' @{ specSheetPhaseId = $PhaseId; specSheetTypeId = $SheetTypeId; propertyValues = @() }
$sheetId = $location.sheetId

$operations = @(New-TableOperations $tableTemplate 'Bore and journal limits' 1 4 $limitValue)
$operations += New-TableOperations $tableTemplate 'Running clearances' 2 3 $limitValue
Save-Changes $Admin $sheetId $operations

Write-Host 'v1: Local Admin submits, Local Approver approves...'
$submission = Submit-Changes $Admin $sheetId 'First issue of the limits.' @($UserIds[$Approver])
Approve-Submission $Approver $sheetId $submission

Write-Host 'v2: Local Engineer changes the first table, Local Admin approves...'
$sheet = Get-Sheet $sheetId
$rows = Get-Rows $sheet 'Bore and journal limits'
$operations = @(
    New-NumberUpdate (Get-Cell $rows[1] 'numeric') 85.025
    New-NumberUpdate (Get-Cell $rows[2] 'numeric') 90.04
    New-TextUpdate (Get-Cell $rows[2] 'text') 'Crank main journal'
)
Save-Changes $Engineer $sheetId $operations
$submission = Submit-Changes $Engineer $sheetId 'Tightened the bore and journal limits after the gauge study.' @($UserIds[$Admin])
Approve-Submission $Admin $sheetId $submission

Write-Host 'v3: Local Admin renames the second table and changes a row, Local Approver approves...'
$sheet = Get-Sheet $sheetId
$table = $sheet.sheet.tables | Where-Object { $_.title -eq 'Running clearances' }
$rows = Get-Rows $sheet 'Running clearances'
$operations = @(
    [ordered]@{ '$type' = 'UpdateSpecSheetTableOp'; id = $table.id; title = 'Running clearances (hot)'; displayOrder = $table.displayOrder }
    New-NumberUpdate (Get-Cell $rows[1] 'numeric') 81.5
)
Save-Changes $Admin $sheetId $operations
$submission = Submit-Changes $Admin $sheetId 'Clearances are now quoted hot.' @($UserIds[$Approver])
Approve-Submission $Approver $sheetId $submission

Write-Host 'Awaiting approval: Local Approver changes a row of the second table, for Local Admin to approve...'
$sheet = Get-Sheet $sheetId
$rows = Get-Rows $sheet 'Running clearances (hot)'
Save-Changes $Approver $sheetId @(New-NumberUpdate (Get-Cell $rows[2] 'numeric') 86.75)
Submit-Changes $Approver $sheetId 'Corrected the crank journal clearance.' @($UserIds[$Admin]) | Out-Null

Write-Host 'Rejected: Local Admin changes the first row, Local Approver rejects it...'
$rows = Get-Rows $sheet 'Bore and journal limits'
Save-Changes $Admin $sheetId @(New-TextUpdate (Get-Cell $rows[1] 'text') 'Cylinder bore (liner)')
$submission = Submit-Changes $Admin $sheetId 'Renamed the bore row for liners.' @($UserIds[$Approver])
Reject-Submission $Approver $sheetId $submission 'Liners are a separate table; keep this row for parent bores.'

Write-Host 'Checked out: Local Engineer changes a row and adds a row, without submitting...'
$sheet = Get-Sheet $sheetId
$table = $sheet.sheet.tables | Where-Object { $_.title -eq 'Bore and journal limits' }
$rows = Get-Rows $sheet 'Bore and journal limits'
Save-Changes $Engineer $sheetId @(New-NumberUpdate (Get-Cell $rows[3] 'numeric') 70.012)

# A new row is a copy of the table's last group, linked to its first group as the screens add one.
$groups = @($table.groups | Sort-Object displayOrder)
$lastGroup = $groups[-1]
$groupTemplate = $tableTemplate.tableGroupTemplates | Where-Object { $_.id -eq $lastGroup.tableGroupTemplateId }
$script:nextTempId = -1000
$groupOperation = [ordered]@{
    '$type' = 'AddSpecSheetTableGroupOp'; tempId = -1; specSheetTableId = $table.id; tableGroupTemplateId = $groupTemplate.id
    order = $lastGroup.displayOrder + 1; referenceId = [guid]::NewGuid().ToString()
}
if ($groups.Count -gt 1) {
    $groupOperation.linkedSpecSheetTableGroupId = $groups[0].id
}

$operations = @($groupOperation)
foreach ($rowTemplate in $groupTemplate.rowTemplates) {
    $operations += [ordered]@{
        '$type' = 'AddSpecSheetTableRowOp'; tempId = -2; specSheetTableGroupId = -1; tableRowTemplateId = $rowTemplate.id
        rowNumber = $rowTemplate.rowNumber; referenceId = [guid]::NewGuid().ToString()
    }
    $cellTempId = -10
    foreach ($cellTemplate in ($rowTemplate.cellTemplates | Sort-Object columnNumber)) {
        $cellTempId--
        $operations += [ordered]@{
            '$type' = 'AddSpecSheetTableCellOp'; tempId = $cellTempId; specSheetTableRowId = -2
            tableCellTypeId = $cellTemplate.tableCellTypeId; cellConfigurationId = $cellTemplate.cellConfigurationId
            rowNumber = $rowTemplate.rowNumber; columnNumber = $cellTemplate.columnNumber
            rowSpan = [Math]::Max(1, [int]$cellTemplate.rowSpan); colSpan = [Math]::Max(1, [int]$cellTemplate.colSpan)
            referenceId = [guid]::NewGuid().ToString()
        }
        switch ($cellTemplate.tableCellType.modelType) {
            'text' { $operations += [ordered]@{ '$type' = 'AddTextValueOp'; specSheetTableCellId = $cellTempId; value = 'Oil pump bore' } }
            'numeric' { $operations += [ordered]@{ '$type' = 'AddNumberValueOp'; specSheetTableCellId = $cellTempId; value = 42.0 + $cellTemplate.columnNumber / 100 } }
        }
    }
    break
}
Save-Changes $Engineer $sheetId $operations

$sheet = Get-Sheet $sheetId
Write-Host ''
Write-Host "Seeded sheet $sheetId (phase $PhaseId, type $SheetTypeId), at version $($sheet.sheet.version.versionNumber):"
foreach ($submission in ($sheet.submissions | Sort-Object id)) {
    $status = @('awaiting approval', 'approved', 'rejected', 'withdrawn')[$submission.status]
    $version = if ($submission.sheetVersionNumber) { " as v$($submission.sheetVersionNumber)" } else { '' }
    Write-Host ("  submission {0} by {1}: {2}{3} - {4}" -f $submission.id, $submission.submittedByUser.displayName, $status, $version, $submission.note)
}

$holders = @{}
foreach ($t in $sheet.sheet.tables) {
    $revisions = @($t.revision) + @($t.groups | ForEach-Object { $_.revision }) + @($t.groups | ForEach-Object { $_.rows } | ForEach-Object { $_.revision })
    foreach ($revision in ($revisions | Where-Object { $_.status -eq 0 })) {
        $key = "{0} ({1})" -f $revision.createdByUser.displayName, $(if ($revision.sheetSubmissionId) { 'awaiting approval' } else { 'checked out' })
        $holders[$key] = 1 + [int]$holders[$key]
    }
}
foreach ($key in ($holders.Keys | Sort-Object)) {
    Write-Host "  $($holders[$key]) item(s): $key"
}
