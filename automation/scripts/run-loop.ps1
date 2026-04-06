#Requires -Version 5.1
$ErrorActionPreference = "Continue"

# UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding  = [System.Text.Encoding]::UTF8
$OutputEncoding            = [System.Text.Encoding]::UTF8

$scriptsDir    = $PSScriptRoot
$automationDir = Split-Path $scriptsDir -Parent
$queueDir      = Join-Path $automationDir "queue"
$logFile       = Join-Path $queueDir "automation.log"
$historyFile   = Join-Path $queueDir "history.log"
$statePath     = Join-Path $queueDir "state.json"
$resultPath    = Join-Path $queueDir "result.txt"
$planPath      = Join-Path $queueDir "plan.txt"
$taskPath      = Join-Path $queueDir "task.txt"
$instructPath  = Join-Path $queueDir "instruction.txt"

$MAX_ITERATIONS      = 50
$MAX_CONSECUTIVE_ERR = 3

function Write-Log($msg) {
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [LOOP] $msg"
    Write-Host $line
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

function Write-History($msg) {
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $msg"
    Add-Content -Path $historyFile -Value $line -Encoding UTF8
}

function Set-SP {
    param($Obj, [string]$Name, $Value)
    if ($Obj.PSObject.Properties[$Name]) { $Obj.$Name = $Value }
    else { $Obj | Add-Member -NotePropertyName $Name -NotePropertyValue $Value -Force }
}

function Read-State {
    if (-not (Test-Path $statePath)) {
        return [PSCustomObject]@{
            current_iteration  = 0
            max_iterations     = $MAX_ITERATIONS
            status             = "idle"
            is_complete        = $false
            consecutive_errors = 0
            last_error         = $null
            last_error_hash    = $null
            lastUpdated        = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
        }
    }

    try {
        return Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json
    }
    catch {
        Write-Log "state.json okunamadi, sifirlaniyor."
        return [PSCustomObject]@{
            current_iteration  = 0
            max_iterations     = $MAX_ITERATIONS
            status             = "idle"
            is_complete        = $false
            consecutive_errors = 0
            last_error         = $null
            last_error_hash    = $null
        }
    }
}

function Save-State($state) {
    Set-SP $state "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $state | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
}

function Get-ErrorHash($text) {
    if ([string]::IsNullOrWhiteSpace($text)) { return $null }

    $bytes = [System.Text.Encoding]::UTF8.GetBytes(
        $text.Substring(0, [Math]::Min(300, $text.Length))
    )

    $hash = [System.Security.Cryptography.MD5]::Create().ComputeHash($bytes)
    return [BitConverter]::ToString($hash) -replace '-', ''
}

function Invoke-Analyzer($resultText, $planText) {
    Write-Log "Analyzer baslatiliyor (Claude Sonnet 4.6)..."

    $prompt = @"
Sen bir kod analizörüsün. Görevin, uygulayýcýnýn yaptýðý iþi deðerlendirmek.

PLAN:
$planText

SONUC:
$resultText

Karar kurallari:

1. SYSTEM_COMPLETE sadece su durumda verilebilir:
- Planin tum maddeleri eksiksiz tamamlandiysa
- Hicbir eksik endpoint, UI, route, validation, migration, test veya entegrasyon kalmadiysa
- SONUC icinde remaining, follow-up, todo, next, later, not implemented, still, needs gibi ifadeler gecmiyorsa
- Sonraki iterasyona ihtiyac yoksa

2. CONTINUE su durumlarin herhangi birinde verilmelidir:
- En ufak eksik veya belirsizlik varsa
- Kod degismis ama gorev tam bitmemisse
- Follow-up veya remaining issue varsa
- Sonraki adim gerekiyorsa
- Emin degilsen

Sadece JSON don:
{
  "status": "SYSTEM_COMPLETE" veya "CONTINUE",
  "reason": "Kisa neden",
  "next_steps": [
    "Eksik is 1",
    "Eksik is 2"
  ]
}
"@

    try {
        $body = @{
            model      = "claude-sonnet-4-6"
            max_tokens = 1000
            messages   = @(
                @{
                    role    = "user"
                    content = $prompt
                }
            )
        } | ConvertTo-Json -Depth 10

        $response = Invoke-RestMethod `
            -Uri "http://127.0.0.1:8045/v1/messages" `
            -Method Post `
            -Headers @{
                "x-api-key"         = $env:BEACHGO_ANTHROPIC_KEY
                "anthropic-version" = "2023-06-01"
                "content-type"      = "application/json"
            } `
            -Body ([System.Text.Encoding]::UTF8.GetBytes($body))

        $raw = $response.content[0].text

        if ($raw -match '(?s)\{.*\}') {
            return ($Matches[0] | ConvertFrom-Json)
        }

        return ($raw | ConvertFrom-Json)
    }
    catch {
        Write-Log "Analyzer hatasi: $_"
        return [PSCustomObject]@{
            status     = "CONTINUE"
            reason     = "Analyzer hatasi"
            next_steps = @("Analyzer hatasini duzelt", "Ayni iteration'i tekrar dene")
        }
    }
}

$state = Read-State

for ($i = 1; $i -le $MAX_ITERATIONS; $i++) {
    Write-Log "===== ITERATION $i ====="

    Set-SP $state "current_iteration" $i
    Set-SP $state "status" "running"
    Save-State $state

    & (Join-Path $scriptsDir "run-planner.ps1")
    & (Join-Path $scriptsDir "run-executor.ps1")

    $state = Read-State

    $resultText = if (Test-Path $resultPath) {
        Get-Content $resultPath -Raw -Encoding UTF8
    } else {
        ""
    }

    $planText = if (Test-Path $planPath) {
        Get-Content $planPath -Raw -Encoding UTF8
    } else {
        ""
    }

    $analysis = Invoke-Analyzer -resultText $resultText -planText $planText

    Write-Log "Analyzer Sonucu: $($analysis.status) - $($analysis.reason)"
    Write-History "Iteration $i => $($analysis.status) : $($analysis.reason)"

    if ($analysis.status -eq "CONTINUE" -and $analysis.next_steps) {
        $instruction = @"
Analyzer sonraki turda su eksiklerin tamamlanmasini istiyor:

- $($analysis.next_steps -join "`r`n- ")
"@

        Set-Content -Path $instructPath -Value $instruction -Encoding UTF8

        Write-Log "instruction.txt analyzer next_steps ile guncellendi."
        Write-History "Next Steps: $($analysis.next_steps -join ' | ')"
    }

    if ($analysis.status -eq "SYSTEM_COMPLETE") {
        Write-Log "Iteration ${i}: SYSTEM_COMPLETE alindi. Dongu bitiyor."

        Set-SP $state "is_complete" $true
        Set-SP $state "status" "done"
        Set-SP $state "consecutive_errors" 0
        Save-State $state
        break
    }

    $errHash = Get-ErrorHash $resultText

    if ($errHash -and $errHash -eq $state.last_error_hash) {
        $state.consecutive_errors = [int]$state.consecutive_errors + 1
    }
    else {
        $state.consecutive_errors = 1
    }

    Set-SP $state "last_error_hash" $errHash
    Save-State $state

    if ([int]$state.consecutive_errors -ge $MAX_CONSECUTIVE_ERR) {
        Write-Log "$MAX_CONSECUTIVE_ERR kez ayni hata alindi. Dongu durduruluyor."
        Set-SP $state "status" "halted"
        Save-State $state
        break
    }

    Start-Sleep -Seconds 2
}

# Loop finished

$finalState = Read-State

if ($finalState.is_complete -eq $true -or $finalState.status -eq "done") {
    Write-Host ""
    Write-Host "******************************************" -ForegroundColor Green
    Write-Host "*   OTOMASYON TAMAMLANDI                 *" -ForegroundColor Green
    Write-Host "******************************************" -ForegroundColor Green

    Write-Log "Otomasyon basariyla tamamlandi."
    Write-History "=== DONGU TAMAMLANDI ==="
}
elseif ($finalState.status -ne "halted") {
    Write-Host ""
    Write-Host "!!! MAX ITERATION LIMITINE ULASILDI ($MAX_ITERATIONS) !!!" -ForegroundColor Red
    Write-Host "Is tamamlanamadi. history.log dosyasini inceleyin." -ForegroundColor Yellow

    Write-Log "Max iteration limitine ulasildi. Is tamamlanamadi."
    Write-History "=== MAX ITERATION LIMITINE ULASILDI ==="

    Set-SP $finalState "status" "halted"
    Save-State $finalState
}
