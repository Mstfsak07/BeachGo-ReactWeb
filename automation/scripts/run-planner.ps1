$ErrorActionPreference = "Stop"

# UTF-8 ayarları
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

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

function Set-StateProperty {
    param(
        [Parameter(Mandatory=$true)]$Obj,
        [Parameter(Mandatory=$true)][string]$Name,
        $Value
    )

    if ($Obj.PSObject.Properties[$Name]) {
        $Obj.$Name = $Value
    }
    else {
        $Obj | Add-Member -NotePropertyName $Name -NotePropertyValue $Value -Force
    }
}

$lockFile = Join-Path $queueDir "planner.lock"

if (Test-Path $lockFile) {
    $lockAge = (Get-Date) - (Get-Item $lockFile).LastWriteTime

    if ($lockAge.TotalMinutes -gt 10) {
        Write-Log "Eski planner.lock bulundu, siliniyor."
        Remove-Item $lockFile -Force
    }
    else {
        Write-Log "Planner zaten çalışıyor."
        exit
    }
}

New-Item -ItemType File -Path $lockFile -Force | Out-Null

try {
    $statePath  = Join-Path $queueDir "state.json"
    $taskPath   = Join-Path $queueDir "task.txt"
    $resultPath = Join-Path $queueDir "result.txt"
    $planPath   = Join-Path $queueDir "plan.txt"

    if (-not (Test-Path $statePath)) {
        [PSCustomObject]@{
            status      = "idle"
            retryCount  = 0
            maxRetry    = 3
            scope       = "web"
            error       = $null
            lastUpdated = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
        } | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
    }

    try {
        $stateObj = Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json
    }
    catch {
        Write-Log "state.json bozuk, yeniden oluşturuluyor."

        $stateObj = [PSCustomObject]@{
            status      = "idle"
            retryCount  = 0
            maxRetry    = 3
            scope       = "web"
            error       = $null
            lastUpdated = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
        }
    }

    if (-not $stateObj.PSObject.Properties["status"])      { Set-StateProperty $stateObj "status" "idle" }
    if (-not $stateObj.PSObject.Properties["retryCount"])  { Set-StateProperty $stateObj "retryCount" 0 }
    if (-not $stateObj.PSObject.Properties["maxRetry"])    { Set-StateProperty $stateObj "maxRetry" 3 }
    if (-not $stateObj.PSObject.Properties["scope"])       { Set-StateProperty $stateObj "scope" "web" }
    if (-not $stateObj.PSObject.Properties["error"])       { Set-StateProperty $stateObj "error" $null }
    if (-not $stateObj.PSObject.Properties["lastUpdated"]) { Set-StateProperty $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss") }

    if (-not (Test-Path $taskPath)) {
        throw "task.txt bulunamadı."
    }

    $taskText = Get-Content $taskPath -Raw -Encoding UTF8

    if ([string]::IsNullOrWhiteSpace($taskText)) {
        throw "task.txt boş."
    }

    $resultText = ""
    if (Test-Path $resultPath) {
        $resultText = Get-Content $resultPath -Raw -Encoding UTF8
    }

    Set-StateProperty $stateObj "status" "planning"
    Set-StateProperty $stateObj "error" $null
    Set-StateProperty $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")

    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8

    $systemPromptPath = Join-Path $promptsDir "planner-system.txt"

    if (-not (Test-Path $systemPromptPath)) {
        throw "planner-system.txt bulunamadı."
    }

    $systemPrompt = Get-Content $systemPromptPath -Raw -Encoding UTF8

    $fullPrompt = @"
$systemPrompt

--- GÖREV ---
$taskText

--- ÖNCEKİ SONUÇ ---
$resultText
"@

    Write-Log "Planner başlatılıyor..."

    $env:ANTHROPIC_API_KEY  = "sk-1ea2056fc84442c59efcd5fd6fe30f5b"
    $env:ANTHROPIC_BASE_URL = "http://127.0.0.1:8045"
    $env:CLAUDE_CONFIG_DIR  = "$env:USERPROFILE\.claude-antigravity"

    $claudePath = "$env:APPDATA\npm\claude.cmd"

    if (-not (Test-Path $claudePath)) {
        throw "Claude yolu bulunamadı: $claudePath"
    }

    Write-Log "Claude çağrılıyor: $claudePath"

    # Claude çağrısı non-interactive, pipe + --print ile yapılıyor
    $planOutput = $fullPrompt | & $claudePath `
        --model claude-sonnet-4-6 `
        --print 2>&1

    $exitCode = $LASTEXITCODE

    $planOutput | Set-Content $planPath -Encoding UTF8

    if ($exitCode -ne 0) {
        throw "Claude başarısız oldu. ExitCode=$exitCode`n$planOutput"
    }

    $planContent = Get-Content $planPath -Raw -Encoding UTF8

    if ([string]::IsNullOrWhiteSpace($planContent)) {
        throw "Claude boş çıktı döndürdü."
    }

    if ($planContent -match "GÖREV TAMAMLANDI") {
        Set-StateProperty $stateObj "status" "done"
        Write-Log "Görev tamamlandı."
    }
    else {
        Set-StateProperty $stateObj "status" "executing"
        Write-Log "Plan üretildi, executor başlatılıyor."
    }

    Set-StateProperty $stateObj "error" $null
    Set-StateProperty $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")

    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
}
catch {
    $err = $_ | Out-String

    Write-Log "HATA:"
    Write-Log $err

    if (Test-Path $statePath) {
        try {
            $stateObj = Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json
        }
        catch {
            $stateObj = [PSCustomObject]@{}
        }

        Set-StateProperty $stateObj "status" "failed"
        Set-StateProperty $stateObj "error" $err
        Set-StateProperty $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")

        $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
    }
}
finally {
    if (Test-Path $lockFile) {
        Remove-Item $lockFile -Force
    }
}
