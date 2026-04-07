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

function Invoke-ClaudeAPI {
    param([string]$Prompt, [string]$SystemPrompt = "")

    $apiKey  = $env:ANTHROPIC_API_KEY
    $baseUrl = if ($env:ANTHROPIC_BASE_URL) { $env:ANTHROPIC_BASE_URL.TrimEnd('/') } else { "https://api.anthropic.com" }

    $bodyObj = [ordered]@{
        model      = "claude-sonnet-4-6"
        max_tokens = 8096
        messages   = @(@{ role = "user"; content = [string]$Prompt })
    }
    if (-not [string]::IsNullOrWhiteSpace($SystemPrompt)) {
        $bodyObj["system"] = [string]$SystemPrompt
    }

    $bodyJson  = $bodyObj | ConvertTo-Json -Depth 10 -Compress
    $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($bodyJson)

    $headers = @{
        "x-api-key"         = $apiKey
        "anthropic-version" = "2023-06-01"
        "content-type"      = "application/json"
    }

    Write-Log "Anthropic API cagrisi: $baseUrl/v1/messages (model=claude-sonnet-4-6)"

    $response = Invoke-RestMethod `
        -Uri     "$baseUrl/v1/messages" `
        -Method  POST `
        -Headers $headers `
        -Body    $bodyBytes

    return $response.content[0].text
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
    $statePath    = Join-Path $queueDir "state.json"
    $resultPath   = Join-Path $queueDir "result.txt"
    $planPath     = Join-Path $queueDir "plan.txt"
    $instructPath = Join-Path $queueDir "instruction.txt"
    $historyFile  = Join-Path $queueDir "history.log"
    $phasesFile   = Join-Path $queueDir "phases.txt"
    $repoRoot     = Split-Path $automationDir -Parent

    # State oku
    $stateObj = if (Test-Path $statePath) {
        try { Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json }
        catch { Write-Log "state.json bozuk, sifirlanıyor."; [PSCustomObject]@{} }
    } else { [PSCustomObject]@{} }

    foreach ($kv in @(
        @("status","idle"), @("scope","web"),
        @("last_error",$null), @("consecutive_errors",0), @("current_iteration",0),
        @("max_iterations",50), @("is_complete",$false), @("last_result_summary",$null),
        @("current_phase",1), @("total_phases",20)
    )) { if (-not $stateObj.PSObject.Properties[$kv[0]]) { Set-SP $stateObj $kv[0] $kv[1] } }

    # phases.txt yoksa hata ver
    if (-not (Test-Path $phasesFile)) {
        throw "phases.txt bulunamadi. Once run-initializer.ps1 calistirin."
    }

    $phasesContent = Get-Content $phasesFile -Raw -Encoding UTF8

    # Mevcut faz numarasini al
    $currentPhase = if ($stateObj.PSObject.Properties["current_phase"]) { [int]$stateObj.current_phase } else { 1 }
    $totalPhases  = if ($stateObj.PSObject.Properties["total_phases"])  { [int]$stateObj.total_phases  } else { 20 }

    # Siradaki fazi phases.txt'ten cek
    $nextPhase = $currentPhase + 1
    $currentPhaseText = ""
    if ($phasesContent -match "(?s)(FAZ ${currentPhase}:.*?)(?=\r?\nFAZ ${nextPhase}:|\r?\nFAZ_LISTESI_BITIS|\z)") {
        $currentPhaseText = $Matches[1].Trim()
    }

    if ([string]::IsNullOrWhiteSpace($currentPhaseText)) {
        Write-Log "Faz $currentPhase bulunamadi. Tum fazlar tamamlandi."
        Set-SP $stateObj "is_complete" $true
        Set-SP $stateObj "status" "done"
        $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
        "SYSTEM_COMPLETE" | Set-Content $planPath -Encoding UTF8
        "SYSTEM_COMPLETE" | Set-Content $instructPath -Encoding UTF8
        exit 0
    }

    # Diger verileri oku
    $resultText   = if (Test-Path $resultPath)  { Get-Content $resultPath  -Raw -Encoding UTF8 } else { "(henuz yok)" }
    $instructText = if (Test-Path $instructPath) { Get-Content $instructPath -Raw -Encoding UTF8 } else { "(henuz yok)" }

    $gitDiff = try {
        $d = & git -C $repoRoot diff --stat HEAD 2>&1 | Out-String
        if ([string]::IsNullOrWhiteSpace($d)) { "Git diff temiz." } else { $d.Trim() }
    } catch { "Git diff alinamadi." }

    $historySnippet = if (Test-Path $historyFile) {
        $lines = Get-Content $historyFile -Encoding UTF8 -ErrorAction SilentlyContinue
        if ($lines.Count -gt 20) { $lines[-20..-1] -join "`n" } else { $lines -join "`n" }
    } else { "(bos)" }

    $iteration  = if ($stateObj.PSObject.Properties["current_iteration"]) { [int]$stateObj.current_iteration } else { 0 }
    $maxIter    = if ($stateObj.PSObject.Properties["max_iterations"])    { [int]$stateObj.max_iterations    } else { 50 }
    $scope      = if ($stateObj.PSObject.Properties["scope"])             { $stateObj.scope                  } else { "web" }
    $consErrors = if ($stateObj.PSObject.Properties["consecutive_errors"]) { [int]$stateObj.consecutive_errors } else { 0 }
    $lastError  = if ($stateObj.PSObject.Properties["last_error"] -and $stateObj.last_error) { $stateObj.last_error } else { "(yok)" }

    Set-SP $stateObj "status"      "planning"
    Set-SP $stateObj "last_error"  $null
    Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8

    $systemPromptPath = Join-Path $promptsDir "planner-system.txt"
    if (-not (Test-Path $systemPromptPath)) { throw "planner-system.txt bulunamadi." }
    $systemPrompt = Get-Content $systemPromptPath -Raw -Encoding UTF8

    $fullPrompt = @"
--- MEVCUT DURUM ---
Iteration: $iteration / $maxIter
Faz: $currentPhase / $totalPhases
Scope: $scope
Ardisik Hata Sayisi: $consErrors
Son Hata: $lastError

--- SIRADAKI FAZ ---
$currentPhaseText

--- ÖNCEKİ SONUÇ (Gemini ne yaptı) ---
$resultText

--- GIT DIFF ÖZETI ---
$gitDiff

--- SON GEÇMİŞ (son 20 satir) ---
$historySnippet
"@

    Write-Log "Planner baslatiliyor (iteration=$iteration, faz=$currentPhase/$totalPhases)..."

    $env:ANTHROPIC_API_KEY  = $env:BEACHGO_ANTHROPIC_KEY
    $env:ANTHROPIC_BASE_URL = "http://127.0.0.1:8045"
    $env:CLAUDE_CONFIG_DIR  = "$env:USERPROFILE\.claude-antigravity"

    Remove-Item Env:MSYSTEM -ErrorAction SilentlyContinue
    Remove-Item Env:MSYS    -ErrorAction SilentlyContinue

    $maxRetries    = 10
    $retryCount    = 0
    $success       = $false
    $plannerOutput = ""

    while ($retryCount -lt $maxRetries -and -not $success) {
        $retryCount++
        if ($retryCount -gt 1) { Write-Log "Yeniden deneniyor ($retryCount / $maxRetries)..." }

        try {
            $plannerOutput = Invoke-ClaudeAPI -Prompt $fullPrompt -SystemPrompt $systemPrompt
            if (-not [string]::IsNullOrWhiteSpace($plannerOutput)) {
                $success = $true
            } else {
                Write-Log "API bos cikti dondu. (Deneme $retryCount / $maxRetries)"
            }
        } catch {
            Write-Log "API hatasi: $_ (Deneme $retryCount / $maxRetries)"
        }
    }

    if (-not $success) {
        throw "Planner Claude API hatasi ($maxRetries deneme basarisiz oldu)."
    }

    $plannerOutput | Set-Content $planPath    -Encoding UTF8
    $plannerOutput | Set-Content $instructPath -Encoding UTF8

    if ($plannerOutput -imatch "SYSTEM_COMPLETE") {
        Set-SP $stateObj "status"      "done"
        Set-SP $stateObj "is_complete" $true
        Write-Log "Tum fazlar tamamlandi."
    } else {
        Set-SP $stateObj "status" "planned"
        Write-Log "Faz $currentPhase plani uretildi, executor bekleniyor."
    }

    Set-SP $stateObj "last_error"  $null
    Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8

    Write-Host "[PLANNER] Plan uretildi, executor bekleniyor."
    return
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
