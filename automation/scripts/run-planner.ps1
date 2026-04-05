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
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [PLANNER] $msg"
    Write-Host $line
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

function Set-SP {
    param($Obj, [string]$Name, $Value)
    if ($Obj.PSObject.Properties[$Name]) { $Obj.$Name = $Value }
    else { $Obj | Add-Member -NotePropertyName $Name -NotePropertyValue $Value -Force }
}

$lockFile = Join-Path $queueDir "planner.lock"

if (Test-Path $lockFile) {
    $lockAge = (Get-Date) - (Get-Item $lockFile).LastWriteTime
    if ($lockAge.TotalMinutes -gt 10) {
        Write-Log "Eski planner.lock siliniyor."
        Remove-Item $lockFile -Force
    } else {
        Write-Log "Planner zaten calisiyor."
        exit 0
    }
}
New-Item -ItemType File -Path $lockFile -Force | Out-Null

try {
    $statePath   = Join-Path $queueDir "state.json"
    $taskPath    = Join-Path $queueDir "task.txt"
    $resultPath  = Join-Path $queueDir "result.txt"
    $planPath    = Join-Path $queueDir "plan.txt"
    $instructPath = Join-Path $queueDir "instruction.txt"
    $historyFile  = Join-Path $queueDir "history.log"

    # State oku / olustur
    $stateObj = if (Test-Path $statePath) {
        try { Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json }
        catch { Write-Log "state.json bozuk, sifirlanıyor."; [PSCustomObject]@{} }
    } else { [PSCustomObject]@{} }

    foreach ($kv in @(
        @("status","idle"), @("retryCount",0), @("maxRetry",3), @("scope","web"),
        @("last_error",$null), @("consecutive_errors",0), @("current_iteration",0),
        @("max_iterations",20), @("is_complete",$false), @("last_result_summary",$null)
    )) { if (-not $stateObj.PSObject.Properties[$kv[0]]) { Set-SP $stateObj $kv[0] $kv[1] } }

    if (-not (Test-Path $taskPath)) { throw "task.txt bulunamadi." }
    $taskText = Get-Content $taskPath -Raw -Encoding UTF8
    if ([string]::IsNullOrWhiteSpace($taskText)) { throw "task.txt bos." }

    # Context toplama
    $resultText  = if (Test-Path $resultPath)  { Get-Content $resultPath  -Raw -Encoding UTF8 } else { "(henuz yok)" }
    $instructText = if (Test-Path $instructPath) { Get-Content $instructPath -Raw -Encoding UTF8 } else { "(henuz yok)" }

    # Git diff ozeti
    $repoRoot = Split-Path $automationDir -Parent
    $gitDiff  = try {
        $d = & git -C $repoRoot diff --stat HEAD 2>&1 | Out-String
        if ([string]::IsNullOrWhiteSpace($d)) { "Git diff temiz." } else { $d.Trim() }
    } catch { "Git diff alinamadi." }

    # Son history (son 30 satir)
    $historySnippet = if (Test-Path $historyFile) {
        $lines = Get-Content $historyFile -Encoding UTF8 -ErrorAction SilentlyContinue
        if ($lines.Count -gt 30) { $lines[-30..-1] -join "`n" } else { $lines -join "`n" }
    } else { "(bos)" }

    $iteration  = if ($stateObj.PSObject.Properties["current_iteration"]) { [int]$stateObj.current_iteration } else { 0 }
    $maxIter    = if ($stateObj.PSObject.Properties["max_iterations"])    { [int]$stateObj.max_iterations    } else { 20 }
    $scope      = if ($stateObj.PSObject.Properties["scope"])             { $stateObj.scope                  } else { "web" }
    $consErrors = if ($stateObj.PSObject.Properties["consecutive_errors"])  { [int]$stateObj.consecutive_errors } else { 0 }
    $lastError  = if ($stateObj.PSObject.Properties["last_error"] -and $stateObj.last_error) { $stateObj.last_error } else { "(yok)" }

    Set-SP $stateObj "status"      "planning"
    Set-SP $stateObj "last_error"  $null
    Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8

    $systemPromptPath = Join-Path $promptsDir "planner-system.txt"
    if (-not (Test-Path $systemPromptPath)) { throw "planner-system.txt bulunamadi." }
    $systemPrompt = Get-Content $systemPromptPath -Raw -Encoding UTF8

    $fullPrompt = @"
$systemPrompt

--- MEVCUT DURUM ---
Iteration: $iteration / $maxIter
Scope: $scope
Ardisik Hata Sayisi: $consErrors
Son Hata: $lastError

--- GÖREV ---
$taskText

--- ÖNCEKİ INSTRUCTION ---
$instructText

--- ÖNCEKİ SONUÇ ---
$resultText

--- GIT DIFF ÖZETI ---
$gitDiff

--- SON GEÇMİŞ (son 30 satir) ---
$historySnippet
"@

    Write-Log "Planner baslatiliyor (iteration=$iteration, scope=$scope)..."

    $env:ANTHROPIC_API_KEY  = "sk-1ea2056fc84442c59efcd5fd6fe30f5b"
    $env:ANTHROPIC_BASE_URL = "http://127.0.0.1:8045"
    $env:CLAUDE_CONFIG_DIR  = "$env:USERPROFILE\.claude-antigravity"

    $claudePath = "$env:APPDATA\npm\claude.cmd"
    if (-not (Test-Path $claudePath)) { throw "Claude bulunamadi: $claudePath" }

    Write-Log "Claude cagiriliyor..."

    $oldEAP = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    $tempPrompt = Join-Path $env:TEMP "planner-prompt-$iteration.txt"
    $fullPrompt | Set-Content $tempPrompt -Encoding UTF8

    $promptText = Get-Content $tempPrompt -Raw -Encoding UTF8
    
    $plannerArgs = @(
        "--model", "claude-sonnet-4-6",
        "--dangerously-skip-permissions",
        "--add-dir", "C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb",
        "--print",
        "-p", $promptText
    )

    $planOutput = & $claudePath @plannerArgs 2>&1

    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = $oldEAP

    $planOutput | Set-Content $planPath -Encoding UTF8

    # Instruction dosyasina da kaydet (executor icin)
    $planOutput | Set-Content $instructPath -Encoding UTF8

    if ($exitCode -ne 0) { throw "Claude basarisiz. ExitCode=$exitCode`n$planOutput" }

    $planContent = Get-Content $planPath -Raw -Encoding UTF8
    if ([string]::IsNullOrWhiteSpace($planContent)) { throw "Claude bos cikti dondu." }

    if ($planContent -imatch "GÖREV TAMAMLANDI") {
        Set-SP $stateObj "status"      "done"
        Set-SP $stateObj "is_complete" $true
        Write-Log "Gorev tamamlandi sinyali alindi."
    } else {
        Set-SP $stateObj "status" "planned"
        Write-Log "Plan uretildi, executor bekleniyor."
    }

    Set-SP $stateObj "last_error"  $null
    Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8

    # Kisa test
    Write-Log "Claude kisa test calistiriliyor..."
    $testOutput = & $claudePath --model claude-sonnet-4-6 -p "test" 2>&1
    Write-Log "Kisa test sonucu: ExitCode=$LASTEXITCODE, Cikti=$($testOutput.Trim())"
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
