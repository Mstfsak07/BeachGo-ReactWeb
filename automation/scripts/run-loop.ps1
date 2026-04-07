#Requires -Version 5.1
$ErrorActionPreference = "Continue"

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
$phasesFile    = Join-Path $queueDir "phases.txt"

$MAX_ITERATIONS      = 50
$MAX_CONSECUTIVE_ERR = 3

# ─── GLOBAL PROCESS TRACKING ────────────────────────────────────
$script:BackendProcess = $null

function Write-Log($msg) {
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [LOOP] $msg"
    Write-Host $line
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

function Write-History($msg) {
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $msg"
    Add-Content -Path $historyFile -Value $line -Encoding UTF8
}

# ─── CLEANUP: Tüm child process'leri öldür ──────────────────────
function Invoke-Cleanup {
    Write-Log "Cleanup baslatiliyor (orphan process temizleme)..."

    # Backend process
    if ($script:BackendProcess -and -not $script:BackendProcess.HasExited) {
        try {
            $script:BackendProcess.Kill($true)   # recursive = true → child'ları da öldür
            Write-Log "Backend process durduruldu (PID=$($script:BackendProcess.Id))."
        } catch {
            Write-Log "Backend process durdurulamadi: $_"
        }
    }

    # Kaçan node.exe process'leri
    Get-Process -Name "node" -ErrorAction SilentlyContinue | ForEach-Object {
        try { $_.Kill(); Write-Log "Orphan node.exe durduruldu (PID=$($_.Id))." } catch {}
    }

    # Kaçan gemini process'leri
    Get-Process -Name "gemini" -ErrorAction SilentlyContinue | ForEach-Object {
        try { $_.Kill(); Write-Log "Orphan gemini durduruldu (PID=$($_.Id))." } catch {}
    }

    # dotnet run ile başlatılan backend'ler
    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {
        $_.MainWindowTitle -eq "" -and $_.CommandLine -match "run" 2>$null
    } | ForEach-Object {
        try { $_.Kill(); Write-Log "Orphan dotnet process durduruldu (PID=$($_.Id))." } catch {}
    }

    # executor.lock temizle
    $lockFile = Join-Path $queueDir "executor.lock"
    if (Test-Path $lockFile) { Remove-Item $lockFile -Force -ErrorAction SilentlyContinue }
    Write-Log "Cleanup tamamlandi."
}

# ─── ORPHAN CLEANUP: PowerShell kapanınca çalışır ───────────────
Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action {
    Invoke-Cleanup
} | Out-Null

# Ctrl+C handler
[Console]::TreatControlCAsInput = $false
$null = Register-ObjectEvent -InputObject ([Console]) -EventName CancelKeyPress -Action {
    Write-Host "`n[LOOP] Ctrl+C alindi. Cleanup yapiliyor..." -ForegroundColor Yellow
    Invoke-Cleanup
    exit 0
}

# ─── BACKEND YÖNETİMİ ───────────────────────────────────────────
function Stop-Backend {
    if ($script:BackendProcess -and -not $script:BackendProcess.HasExited) {
        try {
            $script:BackendProcess.Kill($true)
            $script:BackendProcess.WaitForExit(5000)
            Write-Log "Backend durduruldu (PID=$($script:BackendProcess.Id))."
        } catch {
            Write-Log "Backend durdurma hatasi: $_"
        }
    }
    $script:BackendProcess = $null
}

function Start-Backend {
    param([string]$RepoRoot)

    # Varsa önce durdur
    Stop-Backend

    # API projesini bul
    $csproj = Get-ChildItem -Path $RepoRoot -Recurse -Filter "*.csproj" -ErrorAction SilentlyContinue |
              Where-Object { $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" } |
              Select-Object -First 1

    if (-not $csproj) {
        Write-Log "Backend csproj bulunamadi, baslatilamadi."
        return $false
    }

    $apiDir = $csproj.DirectoryName
    Write-Log "Backend baslatiliyor: $apiDir"

    try {
        $psi = [System.Diagnostics.ProcessStartInfo]::new("dotnet", "run --no-build")
        $psi.WorkingDirectory        = $apiDir
        $psi.UseShellExecute         = $false
        $psi.RedirectStandardOutput  = $false
        $psi.RedirectStandardError   = $false
        $psi.CreateNoWindow          = $true

        $script:BackendProcess = [System.Diagnostics.Process]::Start($psi)
        Write-Log "Backend baslatildi (PID=$($script:BackendProcess.Id)). 3 saniye bekleniyor..."
        Start-Sleep -Seconds 3
        return $true
    } catch {
        Write-Log "Backend baslatma hatasi: $_"
        return $false
    }
}

function Set-SP {
    param($Obj, [string]$Name, $Value)
    if ($Obj.PSObject.Properties[$Name]) { $Obj.$Name = $Value }
    else { $Obj | Add-Member -NotePropertyName $Name -NotePropertyValue $Value -Force }
}

# ─── BUILD HATA KONTROLÜ ────────────────────────────────────────
function Test-BuildSuccess {
    param([string]$ResultText)
    if ([string]::IsNullOrWhiteSpace($ResultText)) { return $false }
    # result.txt'te build hatası varsa false
    if ($ResultText -imatch "Build FAILED|Error\(s\)|error CS[0-9]|HATALI|BUILD: HATALI") { return $false }
    return $true
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
            current_phase      = 1
            total_phases       = 20
            lastUpdated        = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
        }
    }
    try {
        return Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json
    } catch {
        Write-Log "state.json okunamadi, sifirlaniyor."
        return [PSCustomObject]@{
            current_iteration  = 0
            max_iterations     = $MAX_ITERATIONS
            status             = "idle"
            is_complete        = $false
            consecutive_errors = 0
            last_error         = $null
            last_error_hash    = $null
            current_phase      = 1
            total_phases       = 20
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
Sen bir kod analiz uzmanısın. Gemini'nin yaptığı işi değerlendiriyorsun.

PLAN:
$planText

SONUC:
$resultText

Karar kuralları:
1. PHASE_COMPLETE: Bu faz başarıyla tamamlandı, bir sonraki faza geç.
   - Gemini görevi yaptı, build başarılı, hata yok
2. RETRY: Aynı fazı tekrar dene.
   - Build hatası var, eksik iş var, hata mesajı var
3. SYSTEM_COMPLETE: Tüm sistem tamamlandı.
   - Sadece tüm fazlar bittiyse

Sadece JSON döndür:
{
  "status": "PHASE_COMPLETE" veya "RETRY" veya "SYSTEM_COMPLETE",
  "reason": "Kısa neden",
  "issues": ["varsa sorun 1", "varsa sorun 2"]
}
"@

    try {
        $body = @{
            model      = "claude-sonnet-4-6"
            max_tokens = 500
            messages   = @(@{ role = "user"; content = $prompt })
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
    } catch {
        Write-Log "Analyzer hatasi: $_"
        return [PSCustomObject]@{
            status = "RETRY"
            reason = "Analyzer hatasi"
            issues = @("Analyzer cagirilirken hata olustu")
        }
    }
}

# ─── INITIALIZER KONTROLU ───────────────────────────────────────
if (-not (Test-Path $phasesFile)) {
    Write-Log "phases.txt bulunamadi. Initializer calistiriliyor..."
    & (Join-Path $scriptsDir "run-initializer.ps1")

    if (-not (Test-Path $phasesFile)) {
        Write-Log "KRITIK HATA: Initializer phases.txt olusturamadi. Cikiliyor."
        exit 1
    }
    Write-Log "Initializer tamamlandi. Loop basliyor..."
}

$state    = Read-State
$repoRoot = Split-Path $automationDir -Parent

# ─── ANA DONGU ──────────────────────────────────────────────────
for ($i = 1; $i -le $MAX_ITERATIONS; $i++) {
    Write-Log "===== ITERATION $i (Faz $($state.current_phase)/$($state.total_phases)) ====="

    Set-SP $state "current_iteration" $i
    Set-SP $state "status" "running"
    Save-State $state

    # 1) Planla
    & (Join-Path $scriptsDir "run-planner.ps1")

    $planText = if (Test-Path $planPath) { Get-Content $planPath -Raw -Encoding UTF8 } else { "" }
    if ($planText -imatch "SYSTEM_COMPLETE") {
        Write-Log "SYSTEM_COMPLETE (planner). Dongu bitiyor."
        Set-SP $state "is_complete" $true
        Set-SP $state "status" "done"
        Save-State $state
        break
    }

    # 2) Backend'i durdur — Gemini kod yazarken restart etmesin
    Write-Log "Backend durduruluyor (Gemini calisacak)..."
    Stop-Backend

    # 3) Gemini'yi calistir
    & (Join-Path $scriptsDir "run-executor.ps1")

    $resultText = if (Test-Path $resultPath) { Get-Content $resultPath -Raw -Encoding UTF8 } else { "" }

    # 4) Build basariliysa backend'i yeniden baslat; basarisizsa Gemini'ye geri don
    $buildOk = Test-BuildSuccess -ResultText $resultText
    if ($buildOk) {
        Write-Log "Build basarili. Backend yeniden baslatiliyor..."
        Start-Backend -RepoRoot $repoRoot | Out-Null
    } else {
        Write-Log "Build hatasi var. Backend baslatilmadi, RETRY yapilacak."
    }

    $state = Read-State

    # Tum fazlar bittiyse dur
    if ($state.is_complete -eq $true) {
        Write-Log "is_complete=true. Dongu bitiyor."
        Set-SP $state "status" "done"
        Save-State $state
        break
    }

    $analysis = Invoke-Analyzer -resultText $resultText -planText $planText
    Write-Log "Analyzer: $($analysis.status) - $($analysis.reason)"
    Write-History "Iteration $i | Faz $($state.current_phase) => $($analysis.status) : $($analysis.reason)"

    if ($analysis.status -eq "PHASE_COMPLETE") {
        $nextPhase = [int]$state.current_phase + 1
        Set-SP $state "current_phase" $nextPhase
        Set-SP $state "consecutive_errors" 0
        Set-SP $state "last_error" $null
        Set-SP $state "last_error_hash" $null
        Write-Log "Faz $($nextPhase - 1) tamamlandi. Faz $nextPhase basliyor."
        Write-History "Faz $($nextPhase - 1) TAMAMLANDI => Faz $nextPhase basliyor"

        if ($nextPhase -gt [int]$state.total_phases) {
            Write-Log "Tum fazlar tamamlandi!"
            Set-SP $state "is_complete" $true
            Set-SP $state "status" "done"
            Save-State $state
            break
        }
    } elseif ($analysis.status -eq "SYSTEM_COMPLETE") {
        Write-Log "SYSTEM_COMPLETE. Dongu bitiyor."
        Set-SP $state "is_complete" $true
        Set-SP $state "status" "done"
        Save-State $state
        break
    } else {
        # RETRY - ayni fazda kal
        $errHash = Get-ErrorHash $resultText
        if ($errHash -and $errHash -eq $state.last_error_hash) {
            $state.consecutive_errors = [int]$state.consecutive_errors + 1
        } else {
            $state.consecutive_errors = 1
        }
        Set-SP $state "last_error_hash" $errHash
        Write-Log "Faz $($state.current_phase) tekrar deneniyor (ardisik hata: $($state.consecutive_errors))"

        if ([int]$state.consecutive_errors -ge $MAX_CONSECUTIVE_ERR) {
            Write-Log "$MAX_CONSECUTIVE_ERR kez ayni hata. Sonraki faza geciliyor."
            $nextPhase = [int]$state.current_phase + 1
            Set-SP $state "current_phase" $nextPhase
            Set-SP $state "consecutive_errors" 0
            Write-History "Faz $($nextPhase-1) ATLANDI (max hata) => Faz $nextPhase"
        }
    }

    Save-State $state
    Start-Sleep -Seconds 2
}

# ─── CIKIS CLEANUP ──────────────────────────────────────────────
Stop-Backend

$finalState = Read-State

if ($finalState.is_complete -eq $true -or $finalState.status -eq "done") {
    Write-Host ""
    Write-Host "******************************************" -ForegroundColor Green
    Write-Host "*   OTOMASYON TAMAMLANDI                 *" -ForegroundColor Green
    Write-Host "******************************************" -ForegroundColor Green
    Write-Log "Otomasyon basariyla tamamlandi."
    Write-History "=== DONGU TAMAMLANDI ==="
} else {
    Write-Host ""
    Write-Host "!!! MAX ITERATION LIMITINE ULASILDI ($MAX_ITERATIONS) !!!" -ForegroundColor Red
    Write-Log "Max iteration limitine ulasildi."
    Write-History "=== MAX ITERATION LIMITINE ULASILDI ==="
    Set-SP $finalState "status" "halted"
    Save-State $finalState
}
