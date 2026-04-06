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
        messages = @(@{ role = "user"; content = [string]$Prompt })
    }
    if (-not [string]::IsNullOrWhiteSpace($SystemPrompt)) {
        $bodyObj["system"] = [string]$SystemPrompt
    }

    $bodyJson = $bodyObj | ConvertTo-Json -Depth 10 -Compress
    $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($bodyJson)

    $headers = @{
        "x-api-key"         = $apiKey
        "anthropic-version" = "2023-06-01"
        "content-type"      = "application/json"
    }

    Write-Log "Anthropic API cagrisi: $baseUrl/v1/messages (model=claude-sonnet-4-6)"

    $responseRaw = Invoke-WebRequest `
        -Uri     "$baseUrl/v1/messages" `
        -Method  POST `
        -Headers $headers `
        -Body    $bodyBytes

    $responseText = [System.Text.Encoding]::UTF8.GetString($responseRaw.RawContentStream.ToArray())
$response = $responseText | ConvertFrom-Json
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
    $taskPath     = Join-Path $queueDir "task.txt"
    $resultPath   = Join-Path $queueDir "result.txt"
    $planPath     = Join-Path $queueDir "plan.txt"
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

    $resultText   = if (Test-Path $resultPath)   { Get-Content $resultPath   -Raw -Encoding UTF8 } else { "(henuz yok)" }
    $instructText = if (Test-Path $instructPath)  { Get-Content $instructPath -Raw -Encoding UTF8 } else { "(henuz yok)" }

    $repoRoot = Split-Path $automationDir -Parent
    $gitDiff  = try {
        $d = & git -C $repoRoot diff --stat HEAD 2>&1 | Out-String
        if ([string]::IsNullOrWhiteSpace($d)) { "Git diff temiz." } else { $d.Trim() }
    } catch { "Git diff alinamadi." }

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

    $env:ANTHROPIC_API_KEY = $env:BEACHGO_ANTHROPIC_KEY
    $env:ANTHROPIC_BASE_URL = "http://127.0.0.1:8045"
    $env:CLAUDE_CONFIG_DIR  = "$env:USERPROFILE\.claude-antigravity"

    Remove-Item Env:MSYSTEM -ErrorAction SilentlyContinue
    Remove-Item Env:MSYS    -ErrorAction SilentlyContinue

    $maxRetries = 10
    $retryCount = 0
    $success    = $false
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
        throw "Planner Claude API hatasi (10 deneme basarisiz oldu).`n$plannerOutput"
    }

    $plannerOutput | Set-Content $planPath     -Encoding UTF8
    $plannerOutput | Set-Content $instructPath  -Encoding UTF8

    if ([string]::IsNullOrWhiteSpace($plannerOutput)) { throw "Claude bos cikti dondu." }

    if ($plannerOutput -imatch "SYSTEM_COMPLETE") {
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
