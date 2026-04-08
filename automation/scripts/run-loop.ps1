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
$statePath       = Join-Path $queueDir "state.json"
$resultPath      = Join-Path $queueDir "result.txt"
$instructionPath        = Join-Path $queueDir "instruction.txt"
$phasesFile      = Join-Path $queueDir "phases.txt"
$instructionFile = Join-Path $queueDir "instruction.txt"

$MAX_ITERATIONS      = 50
$MAX_CONSECUTIVE_ERR = 3
$MAX_ANALYZER_ERR    = 3
$ANALYZER_MODEL      = if ($env:BEACHGO_ANALYZER_MODEL) { $env:BEACHGO_ANALYZER_MODEL } else { "claude-sonnet-4-6" }

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
    Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -match "run" } | ForEach-Object {
        try { Stop-Process -Id $_.ProcessId -Force; Write-Log "Orphan dotnet process durduruldu (PID=$($_.ProcessId))." } catch {}
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
try {
    [Console]::TreatControlCAsInput = $false
    $null = Register-ObjectEvent -InputObject ([Console]) -EventName CancelKeyPress -Action {
        Write-Host "`n[LOOP] Ctrl+C alindi. Cleanup yapiliyor..." -ForegroundColor Yellow
        Invoke-Cleanup
        exit 0
    }
} catch {
    Write-Log "Ctrl+C handler atanamadi (non-interactive shell), devam ediliyor."
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

function Invoke-Analyzer {
    Write-Log "Analyzer baslatiliyor ($ANALYZER_MODEL)..."

    # Dosya içeriklerini oku ve prompt'a göm — API call'un tool erişimi yok
    $resultContent = if (Test-Path $resultPath)    { Get-Content $resultPath -Raw -Encoding UTF8 } else { "(empty)" }
    $stateContent  = if (Test-Path $statePath)     { Get-Content $statePath  -Raw -Encoding UTF8 } else { "(empty)" }
    $instrContent  = if (Test-Path $instructionFile) { Get-Content $instructionFile -Raw -Encoding UTF8 } else { "(empty)" }

    # Çok uzun sonuçları kırp (token limiti aşmasın)
    if ($resultContent.Length -gt 3000) { $resultContent = $resultContent.Substring(0, 3000) + "`n...(truncated)" }

    $prompt = @"
You are an automation loop analyzer with code quality verification capabilities.
Evaluate the last executor result and decide what to do next.

=== queue/state.json ===
$stateContent

=== queue/result.txt (last executor output) ===
$resultContent

=== queue/instruction.txt (what was attempted) ===
$instrContent

VERIFICATION CHECKLIST:
- Were all requested files written successfully?
- Did the build succeed?
- Are there any security issues in the changes (input validation, auth, data exposure)?
- Are there any obvious performance problems (N+1 queries, missing pagination)?
- Were edge cases considered?

RULES:
- Return CONTINUE if: any write_file failed, build failed, files were missing, workspace errors occurred, any step is incomplete, or verification found issues.
- Return SYSTEM_COMPLETE only if all files were written successfully AND there are no errors AND verification passes.
- If returning CONTINUE, next_steps must be ONE SHORT SENTENCE (max 120 chars) describing only the immediate next action.
- Do NOT write paragraphs, steps, or lists in next_steps. One line only.

Return ONLY valid JSON, nothing else:

{"decision":"CONTINUE","next_steps":"<one short sentence, max 120 chars>"}

or

{"decision":"SYSTEM_COMPLETE"}
"@

    try {
        $body = @{
            model      = $ANALYZER_MODEL
            max_tokens = 200
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
            $parsed = $Matches[0] | ConvertFrom-Json
            # Sadece gecerli decision degerlerini kabul et
            if ($parsed.decision -eq "SYSTEM_COMPLETE" -or $parsed.decision -eq "CONTINUE") {
                return $parsed
            }
        }
        Write-Log "Analyzer gecersiz JSON dondu: $raw"
        return $null
    } catch {
        Write-Log "Analyzer hatasi: $_"
        return $null
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

Write-Log "STATE BASLANGIC: iteration=$($state.current_iteration) status=$($state.status) is_complete=$($state.is_complete)"

# phases.txt'ten toplam faz sayısını oku ve state'e yaz
$phaseCount = 0
if (Test-Path $phasesFile) {
    $phaseCount = (Select-String -Path $phasesFile -Pattern "^FAZ \d+:").Count
}
if ($phaseCount -lt 1) { $phaseCount = 1 }
Set-SP $state "total_phases" $phaseCount
Save-State $state
Write-Log "Toplam faz sayisi: $phaseCount (phases.txt'ten okundu)"

# ─── ANA DONGU ──────────────────────────────────────────────────
$analyzerErrorCount = 0

$startIteration = 1
if ($state.current_iteration -and [int]$state.current_iteration -gt 1 -and $state.status -ne "done") {
    $startIteration = [int]$state.current_iteration
    Write-Log "Onceki iteration'dan devam ediliyor: $startIteration"
}

for ($i = $startIteration; $i -le $MAX_ITERATIONS; $i++) {
    Write-Log "===== ITERATION $i ====="

    Set-SP $state "current_iteration" $i
    Set-SP $state "status" "running"
    Save-State $state

    # 1) Backend'i durdur — Executor kod yazarken restart etmesin
    Write-Log "Backend durduruluyor (Executor calisacak)..."
    Stop-Backend

    # 2) Executor'i calistir (try/catch ile — executor hatasi loop'u kirmaz)
    $executorFailed = $false
    try {
        & (Join-Path $scriptsDir "run-executor.ps1")
    } catch {
        Write-Log "EXECUTOR HATASI (loop devam edecek): $_"
        $executorFailed = $true
    }

    if (Test-Path $instructionFile) {
        Write-Log "Current instruction after executor:"
        Write-Log (Get-Content $instructionFile -Raw -ErrorAction SilentlyContinue)
    }

    $resultText = if (Test-Path $resultPath) { Get-Content $resultPath -Raw -Encoding UTF8 } else { "" }

    if ($executorFailed -and [string]::IsNullOrWhiteSpace($resultText)) {
        Write-Log "Executor basarisiz ve result bos. Analyzer'a geciliyor (bos result ile)."
        "EXECUTOR HATASI: Onceki adim basarisiz oldu. Devam et." | Set-Content $resultPath -Encoding UTF8
        $resultText = Get-Content $resultPath -Raw -Encoding UTF8
    }

    # 3) Build basariliysa backend'i yeniden baslat
    $buildOk = Test-BuildSuccess -ResultText $resultText
    if ($buildOk) {
        Write-Log "Build basarili. Backend yeniden baslatiliyor..."
        Start-Backend -RepoRoot $repoRoot | Out-Null
    } else {
        Write-Log "Build hatasi var. Backend baslatilmadi."
    }

    # 4) Analyzer calistir — sadece SYSTEM_COMPLETE donerse dur
    #    Verify/iterate pattern: Analyzer hem ilerleme karari hem de kalite kontrolu yapar
    Write-Host "ANALYZER BASLIYOR"
    $analysis = Invoke-Analyzer
    Write-Host "ANALYZER BITTI"
    Write-Log "Analyzer raw response: $($analysis | ConvertTo-Json -Compress)"
    Write-History "Iteration $i => Analyzer: $($analysis.decision)"

    if ($analysis -eq $null) {
        $analyzerErrorCount++
        Write-Log "Analyzer gecersiz/bos yanit ($analyzerErrorCount/$MAX_ANALYZER_ERR). Onceki instruction korunuyor, devam ediliyor."
        if ($analyzerErrorCount -ge $MAX_ANALYZER_ERR) {
            Write-Log "$MAX_ANALYZER_ERR analyzer hatasi. Dongu durduruluyor."
            break
        }
        Save-State $state
        Start-Sleep -Seconds 2
        continue
    }

    $analyzerErrorCount = 0

    if ($analysis.decision -eq "SYSTEM_COMPLETE") {
        Write-Log "[ANALYZER] SYSTEM_COMPLETE. Dongu bitiyor."
        Set-SP $state "is_complete" $true
        Set-SP $state "status" "done"
        Save-State $state
        break
    }

    if ($analysis.decision -eq "CONTINUE") {
    Write-Log "[ANALYZER] CONTINUE"
    Write-Log "[ANALYZER] next_steps = $($analysis.next_steps)"

    if (-not [string]::IsNullOrWhiteSpace($analysis.next_steps)) {
        $analysis.next_steps | Set-Content $instructionFile -Encoding UTF8
        Write-Log "[ANALYZER] instruction guncellendi"
    }

    Set-SP $state "status" "running"
    Save-State $state
    Write-Log "[LOOP] Sonraki iteration basliyor"
    Start-Sleep -Seconds 2
    continue
}

    Write-Log "Analyzer tanimsiz decision='$($analysis.decision)'. Onceki instruction korunuyor, devam ediliyor."
    Save-State $state
    Start-Sleep -Seconds 2
}

# ─── CIKIS CLEANUP ──────────────────────────────────────────────
Stop-Backend

$finalState = Read-State
Write-Log "STATE BITIS: iteration=$($finalState.current_iteration) status=$($finalState.status) is_complete=$($finalState.is_complete)"

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
