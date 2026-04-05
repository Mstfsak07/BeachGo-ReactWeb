#Requires -Version 5.1
$ErrorActionPreference = "Stop"

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding  = [System.Text.Encoding]::UTF8
$OutputEncoding            = [System.Text.Encoding]::UTF8

$scriptsDir    = $PSScriptRoot
$automationDir = Split-Path $scriptsDir -Parent
$queueDir      = Join-Path $automationDir "queue"
$promptsDir    = Join-Path $automationDir "prompts"
$logFile       = Join-Path $queueDir "automation.log"

function Write-Log($msg) {
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [EXECUTOR] $msg"
    Write-Host $line
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

function Set-SP {
    param($Obj, [string]$Name, $Value)
    if ($Obj.PSObject.Properties[$Name]) { $Obj.$Name = $Value }
    else { $Obj | Add-Member -NotePropertyName $Name -NotePropertyValue $Value -Force }
}

$lockFile = Join-Path $queueDir "executor.lock"

if (Test-Path $lockFile) {
    $lockAge = (Get-Date) - (Get-Item $lockFile).LastWriteTime
    if ($lockAge.TotalMinutes -gt 10) {
        Write-Log "Eski executor.lock siliniyor."
        Remove-Item $lockFile -Force
    } else {
        Write-Log "Executor zaten calisiyor."
        exit 0
    }
}
New-Item -ItemType File -Path $lockFile -Force | Out-Null

try {
    $statePath   = Join-Path $queueDir "state.json"
    $planPath    = Join-Path $queueDir "plan.txt"
    $resultPath  = Join-Path $queueDir "result.txt"
    $outputPath  = Join-Path $queueDir "executor-output.txt"

    if (-not (Test-Path $planPath))  { throw "plan.txt bulunamadi." }
    if (-not (Test-Path $statePath)) { throw "state.json bulunamadi." }

    try { $stateObj = Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json }
    catch { throw "state.json okunamadi." }

    $iteration = if ($stateObj.PSObject.Properties["current_iteration"]) { [int]$stateObj.current_iteration } else { 0 }

    Set-SP $stateObj "status"      "executing"
    Set-SP $stateObj "last_error"  $null
    Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8

    $planText = Get-Content $planPath -Raw -Encoding UTF8

    $executorSystemPath = Join-Path $promptsDir "executor-system.txt"
    if (-not (Test-Path $executorSystemPath)) { throw "executor-system.txt bulunamadi." }
    $systemPrompt = Get-Content $executorSystemPath -Raw -Encoding UTF8

    $fullPrompt = @"
$systemPrompt

--- ITERATION: $iteration ---

--- PLAN ---
$planText
"@

    Write-Log "Executor baslatiliyor (Gemini yolo mode, iteration=$iteration)..."

    $geminiPath = "$env:APPDATA\npm\gemini.cmd"
    if (-not (Test-Path $geminiPath)) { throw "Gemini CLI bulunamadi: $geminiPath" }

    $tempPrompt = Join-Path $env:TEMP "executor-prompt-$iteration.txt"
    $fullPrompt | Set-Content $tempPrompt -Encoding UTF8

    try {
        $promptText = Get-Content $tempPrompt -Raw -Encoding UTF8

        $oldEAP = $ErrorActionPreference
        $ErrorActionPreference = "Continue"

        $executorArgs = @(
            "-m", "gemini-3-pro",
            "-p", $promptText
        )
        $executorOutput = & $geminiPath @executorArgs 2>&1
        
        $exitCode = $LASTEXITCODE
        $ErrorActionPreference = $oldEAP

        $executorOutput | Set-Content $outputPath -Encoding UTF8
    }
    catch {
        $exitCode = 1
        $executorOutput = $_ | Out-String
        $executorOutput | Set-Content $outputPath -Encoding UTF8
    }

    # result.txt'e yaz (her iterasyonda üzerine yaz)
    $executorOutput | Set-Content $resultPath -Encoding UTF8

    # Gereksiz MCP loglarini ve bos satirlari temizle
    $raw = Get-Content $resultPath -Raw
    $clean = $raw -split "`r`n" | Where-Object {
        $_.Trim() -ne "" -and
        $_ -notmatch "Registering notification handlers for server" -and
        $_ -notmatch "Scheduling MCP context refresh" -and
        $_ -notmatch "Executing MCP context refresh" -and
        $_ -notmatch "browsermcp" -and
        $_ -notmatch "Connected to MCP server" -and
        $_ -notmatch "MCP server started" -and
        $_ -notmatch "^(INFO|DEBUG|TRACE)\b"
    }
    Set-Content -Path $resultPath -Value ($clean -join "`r`n") -Encoding UTF8

    if ($exitCode -ne 0) {
        $executorText = if ($executorOutput -is [System.Management.Automation.ErrorRecord]) {
            $executorOutput.ToString()
        } else {
            [string]$executorOutput
        }
        $preview = $executorText.Substring(0, [Math]::Min(500, $executorText.Length))
        throw "Gemini basarisiz. ExitCode=$exitCode`n$preview"
    }

    $resultText = Get-Content $resultPath -Raw -Encoding UTF8

    if ([string]::IsNullOrWhiteSpace($resultText)) {
        throw "Executor result.txt üretmedi."
    }

    # Basarisizlik pattern kontrolu
    $failPatterns = @(
        '"status"\s*:\s*"failed"',
        '"phase"\s*:\s*"failed"',
        'CRITICAL ERROR',
        'FATAL'
    )
    $isFailed = $false
    foreach ($p in $failPatterns) {
        if ($resultText -imatch $p) { $isFailed = $true; break }
    }

    if ($isFailed) {
        Set-SP $stateObj "status"     "failed"
        Set-SP $stateObj "last_error" ($resultText.Substring(0,[Math]::Min(500,$resultText.Length)))
        Write-Log "Executor basarisiz sonuc dondu."
    } else {
        Set-SP $stateObj "status"     "completed"
        Set-SP $stateObj "last_error" $null
        Write-Log "Executor tamamlandi."
    }

    # Result ozeti kaydet
    $summary = if ($resultText.Length -gt 400) { $resultText.Substring(0,400) + "..." } else { $resultText }
    Set-SP $stateObj "last_result_summary" $summary
    Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
}
catch {
    $err = $_ | Out-String
    Write-Log "HATA: $err"

    try { $stateObj = Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json }
    catch { $stateObj = [PSCustomObject]@{} }

    Set-SP $stateObj "status"      "failed"
    Set-SP $stateObj "last_error"  $err
    Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
}
finally {
    if (Test-Path $lockFile) { Remove-Item $lockFile -Force }
}
