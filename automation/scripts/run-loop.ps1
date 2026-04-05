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

$MAX_ITERATIONS       = 50
$MAX_CONSECUTIVE_ERR  = 3

# Helper Functions

function Write-Log($msg) {
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [LOOP] $msg"
    Write-Host $line
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

function Write-History($msg) {
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $msg"
    Add-Content -Path $historyFile -Value $line -Encoding UTF8
}

function Write-StatusLine($msg) {
    Write-Host ""
    Write-Host "  >>> $msg" -ForegroundColor Cyan
    Write-Host ""
}

function Set-SPFinalState {
    param($Obj, [string]$Name, $Value)
    if ($Obj.PSObject.Properties[$Name]) { $Obj.$Name = $Value }
    else { $Obj | Add-Member -NotePropertyName $Name -NotePropertyValue $Value -Force }
}

function Read-State {
    if (-not (Test-Path $statePath)) {
        return [PSCustomObject]@{
            current_iteration   = 0
            max_iterations      = $MAX_ITERATIONS
            status              = "idle"
            last_error          = $null
            last_result_summary = $null
            is_complete         = $false
            retryCount          = 0
            maxRetry            = 3
            consecutive_errors  = 0
            last_error_hash     = $null
            scope               = "web"
            lastUpdated         = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
        }
    }
    try {
        $s = Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json
        if (-not $s.PSObject.Properties["current_iteration"])   { Set-SPFinalState $s "current_iteration"   0 }
        if (-not $s.PSObject.Properties["max_iterations"])      { Set-SPFinalState $s "max_iterations"      $MAX_ITERATIONS }
        if (-not $s.PSObject.Properties["is_complete"])         { Set-SPFinalState $s "is_complete"         $false }
        if (-not $s.PSObject.Properties["last_error"])          { Set-SPFinalState $s "last_error"          $null }
        if (-not $s.PSObject.Properties["last_result_summary"]) { Set-SPFinalState $s "last_result_summary" $null }
        if (-not $s.PSObject.Properties["consecutive_errors"])  { Set-SPFinalState $s "consecutive_errors"  0 }
        if (-not $s.PSObject.Properties["last_error_hash"])     { Set-SPFinalState $s "last_error_hash"     $null }
        return $s
    }
    catch {
        Write-Log "state.json parse hatasi, sifirlaniyor."
        return Read-State
    }
}

function Save-State($s) {
    Set-SPFinalState $s "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $s | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
}

function Get-GitDiffSummary {
    try {
        $diff = & git -c core.safecrlf=false -C (Split-Path $automationDir -Parent) diff --stat HEAD 2>$null | Out-String
        if ([string]::IsNullOrWhiteSpace($diff)) { return "Git diff temiz (degisiklik yok)." }
        return $diff.Trim()
    } catch { return "Git diff alinamadi." }
}

function Get-ErrorHash($errText) {
    if ([string]::IsNullOrWhiteSpace($errText)) { return $null }
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($errText.Substring(0, [Math]::Min(300, $errText.Length)))
    $hash  = [System.Security.Cryptography.MD5]::Create().ComputeHash($bytes)
    return [BitConverter]::ToString($hash) -replace '-',''
}

# Analyzer (Claude Sonnet 4.6)
function Invoke-Analyzer($resultText, $planText) {
    Write-Log "Analyzer baslatiliyor (Claude Sonnet 4.6)..."
    
    $prompt = @"
Sen bir kod analizörüsün (Claude). Görevin, uygulayıcının (Gemini) yaptığı işi ve planı değerlendirmektir.

PLAN:
$planText

SONUÇ:
$resultText

Şu kararı ver:
1. "SYSTEM_COMPLETE": Eğer plandaki iş yapıldıysa ve genel GÖREV tamamen bittiyse (veya plan zaten SYSTEM_COMPLETE diyorsa).
2. "CONTINUE": Eğer görev henüz tam bitmediyse, plandaki aşama bittiyse (sonraki plan aşamasına geçilecekse) veya onarıma geçmek gerekiyorsa.

Aşağıdaki JSON formatında SADECE JSON döndür:
{
  "status": "SYSTEM_COMPLETE" veya "CONTINUE",
  "reason": "Karar nedeni (kısa)"
}
"@
    $tempPrompt = Join-Path $env:TEMP "analyzer-prompt.txt"
    $prompt | Set-Content $tempPrompt -Encoding UTF8
    
    $env:ANTHROPIC_BASE_URL = "http://127.0.0.1:8045"
    $env:CLAUDE_CONFIG_DIR  = "$env:USERPROFILE\.claude-antigravity"

    $oldEAP = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    $outputRaw = Get-Content $tempPrompt -Raw -Encoding UTF8 | & claude --model sonnet-4.6 -p 2>&1
    
    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = $oldEAP

    if ($exitCode -ne 0) {
        Write-Log "Analyzer Claude hatası. Varsayılan CONTINUE..."
        return @{ status = "CONTINUE"; reason = "Analyzer API Hatası" }
    }

    $output = ($outputRaw | Where-Object { $_ -is [string] }) -join "`n"
    try {
        if ($output -match '(?s)\{.*\}') {
            return ConvertFrom-Json $matches[0]
        }
        return ConvertFrom-Json $output
    } catch {
        return @{ status = "CONTINUE"; reason = "JSON parse error" }
    }
}

# Planner Call

function Invoke-Planner($iteration, $state) {
    Write-StatusLine "[Iteration $iteration/$MAX_ITERATIONS] Planner running..."
    Write-Log "Planner baslatiliyor (iteration $iteration)..."

    $plannerScript = Join-Path $scriptsDir "run-planner.ps1"
    try {
        & $plannerScript
        $exitCode = $LASTEXITCODE
    }
    catch {
        Write-Log "Planner script exception: $_"
        return $false
    }

    $newState = Read-State
    if ($newState.status -eq "planned" -or $newState.status -eq "done") {
        Write-Log "Planner tamamlandi. Status: $($newState.status)"
        return $true
    }
    else {
        Write-Log "Planner basarisiz. Status: $($newState.status)"
        return $false
    }
}

# Executor Call

function Invoke-Executor($iteration) {
    Write-StatusLine "[Iteration $iteration/$MAX_ITERATIONS] Coder running..."
    Write-Log "Executor baslatiliyor (iteration $iteration)..."

    $executorScript = Join-Path $scriptsDir "run-executor.ps1"
    try {
        & $executorScript
        $exitCode = $LASTEXITCODE
    }
    catch {
        Write-Log "Executor script exception: $_"
        return $false
    }

    $newState = Read-State
    if ($newState.status -in @("completed", "done")) {
        Write-Log "Executor tamamlandi."
        return $true
    }
    else {
        Write-Log "Executor basarisiz veya devam gerekiyor. Status: $($newState.status)"
        return $false
    }
}

# Main Loop

Write-Log "=========================================="
Write-Log "Recursive Otomasyon Dongusu BASLATILDI"
Write-Log "Max iterations: $MAX_ITERATIONS"
Write-Log "=========================================="
Write-History "=== YENI DONGU BASLADI ==="

if (Test-Path (Join-Path $queueDir "*.lock")) {
    Write-Log "Eski lock dosyalari temizleniyor..."
    Remove-Item (Join-Path $queueDir "*.lock") -Force -ErrorAction SilentlyContinue
}

$state = Read-State
Set-SPFinalState $state "current_iteration"  0
Set-SPFinalState $state "max_iterations"     $MAX_ITERATIONS
Set-SPFinalState $state "is_complete"        $false
Set-SPFinalState $state "consecutive_errors" 0
Set-SPFinalState $state "status"             "idle"
Save-State $state

for ($i = 1; $i -le $MAX_ITERATIONS; $i++) {

    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Yellow
    Write-Host "  ITERATION $i / $MAX_ITERATIONS" -ForegroundColor Yellow
    Write-Host "==========================================" -ForegroundColor Yellow

    $state = Read-State
    Set-SPFinalState $state "current_iteration" $i
    Save-State $state

    # 1. Planner
    $plannerOk = Invoke-Planner -iteration $i -state $state

    $state = Read-State

    if ($state.status -eq "done") {
        Write-StatusLine "Planner 'done' sinyali verdi. Is tamamlandi."
        Write-Log "Iteration ${i}: Planner DONE sinyali - dongu sonlandiriliyor."
        Write-History "Iteration ${i}: TAMAMLANDI (Planner done sinyali)"

        Set-SPFinalState $state "is_complete" $true
        Set-SPFinalState $state "status"      "done"
        Save-State $state
        break
    }

    if (-not $plannerOk) {
        $errText = if ($state.last_error) { $state.last_error } else { "Planner basarisiz" }
        $errHash = Get-ErrorHash $errText

        Set-SPFinalState $state "consecutive_errors" ([int]$state.consecutive_errors + 1)
        Set-SPFinalState $state "last_error"         $errText
        Set-SPFinalState $state "last_error_hash"    $errHash
        Save-State $state

        Write-Log "Planner basarisiz. Ardisik hata: $($state.consecutive_errors)"
        Write-History "Iteration ${i}: PLANNER HATASI - $errText"

        if ([int]$state.consecutive_errors -ge $MAX_CONSECUTIVE_ERR) {
            Write-Log "!!! $MAX_CONSECUTIVE_ERR ardisik hata! Dongu durduruluyor. Manuel mudahale gerekli. !!!"
            Write-History "DONGU DURDURULDU: $MAX_CONSECUTIVE_ERR ardiisik hata."
            Write-History "Son hata: $errText"
            Write-Host ""
            Write-Host "!!! OTOMASYON DURDURULDU !!!" -ForegroundColor Red
            Write-Host "Neden: $MAX_CONSECUTIVE_ERR tur ust uste ayni hata." -ForegroundColor Red
            Write-Host "Son hata: $errText" -ForegroundColor Red
            Write-Host "Detay: $historyFile" -ForegroundColor Yellow
            Set-SPFinalState $state "status" "halted"
            Save-State $state
            exit 1
        }

        Write-Log "Planner hatasi sonrasi devam deniyor..."
        Start-Sleep -Seconds 3
        continue
    }

    # 2. Executor
    Write-StatusLine "[Iteration $i/$MAX_ITERATIONS] Coder running..."
    $executorOk = Invoke-Executor -iteration $i

    $state      = Read-State
    $resultText = if (Test-Path $resultPath) { Get-Content $resultPath -Raw -Encoding UTF8 } else { "" }
    $planText   = if (Test-Path $planPath)   { Get-Content $planPath   -Raw -Encoding UTF8 } else { "" }
    $gitDiff    = Get-GitDiffSummary

    $summary = if ($resultText.Length -gt 400) { $resultText.Substring(0,400) + "..." } else { $resultText }
    Set-SPFinalState $state "last_result_summary" $summary
    Save-State $state

    # 3. Log results
    Write-StatusLine "[Iteration $i/$MAX_ITERATIONS] Analyzing result..."
    Write-Log "Git diff ozeti: $gitDiff"
    Write-History "--- Iteration $i ---"
    Write-History "Git diff: $gitDiff"
    Write-History "Result ozeti: $summary"

    # Git Auto Commit
    if ($gitDiff -notmatch "Git diff temiz" -and $gitDiff -notmatch "Git diff alinamadi") {
        $commitMsg = "Geliştirme: Otomatik tamamlama"
        if ($planText -match '(?is)<commit_message>(.*?)</commit_message>') {
            $commitMsg = $matches[1].Trim()
        }
        Write-Log "Degisiklikler algilandi, git commit yapiliyor..."
        & git -c core.safecrlf=false -C (Split-Path $automationDir -Parent) add .
        & git -c core.safecrlf=false -C (Split-Path $automationDir -Parent) commit -m $commitMsg
        Write-History "Commit basarili: $commitMsg"
    }

    # 4. Analyze with Claude
    $analysis = Invoke-Analyzer -resultText $resultText -planText $planText
    Write-Log "Analyzer Sonucu: $($analysis.status) - $($analysis.reason)"
    Write-History "Analyzer Sonucu: $($analysis.status) - $($analysis.reason)"

    if ($analysis.status -eq "SYSTEM_COMPLETE" -or $planText -imatch "SYSTEM_COMPLETE") {
        Write-StatusLine "Is tamamlandi (Analyzer Onayi)! Dongu sonlandiriliyor."
        Write-Log "Iteration ${i}: COMPLETE sinyali alindi. Dongu bitiyor."
        Set-SPFinalState $state "is_complete" $true
        Set-SPFinalState $state "status"      "done"
        Set-SPFinalState $state "consecutive_errors" 0
        Save-State $state
        break
    }

    # 5. Check Executor errors just in case
    if (-not $executorOk) {
        $errText = if ($state.last_error) { $state.last_error } else { $resultText }
        $errHash = Get-ErrorHash $errText

        $sameError = ($errHash -ne $null -and $errHash -eq $state.last_error_hash)
        $consErr   = [int]$state.consecutive_errors

        if ($sameError) {
            $consErr++
            Write-Log "Ayni hata $consErr. kez tekrarlandi."
        } else {
            $consErr = 1
        }

        Set-SPFinalState $state "consecutive_errors" $consErr
        Set-SPFinalState $state "last_error"         $errText
        Set-SPFinalState $state "last_error_hash"    $errHash
        Save-State $state

        Write-History "Iteration ${i}: EXECUTOR HATASI ($consErr. kez) - $errText"

        if ($consErr -ge $MAX_CONSECUTIVE_ERR) {
            Write-Log "!!! $MAX_CONSECUTIVE_ERR tur ust uste ayni hata! Dongu durduruluyor. !!!"
            Write-History "DONGU DURDURULDU: $MAX_CONSECUTIVE_ERR ayni hata."
            Write-History "Son hata: $errText"
            Write-Host ""
            Write-Host "!!! OTOMASYON DURDURULDU !!!" -ForegroundColor Red
            Write-Host "Neden: $MAX_CONSECUTIVE_ERR tur ust uste ayni hata tespit edildi." -ForegroundColor Red
            Write-Host "Son hata ozeti: $($errText.Substring(0,[Math]::Min(300,$errText.Length)))" -ForegroundColor Red
            Write-Host "Tam gecmis: $historyFile" -ForegroundColor Yellow
            Set-SPFinalState $state "status" "halted"
            Save-State $state
            exit 1
        }

        Write-Log "Hata sonrasi devam: yeni planner turu baslatiliyor..."
        Write-StatusLine "Continuing because executor failed (iteration ${i})..."
        continue
    }

    Write-Log "Iteration ${i}: Sonraki asama baslatiliyor. Status: $($analysis.status)"
    Set-SPFinalState $state "consecutive_errors" 0
    Set-SPFinalState $state "last_error_hash"    $null
    Save-State $state
    continue
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
    Set-SPFinalState $finalState "status" "halted"
    Save-State $finalState
}
