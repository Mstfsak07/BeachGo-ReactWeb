#Requires -Version 5.1
$ErrorActionPreference = "Continue"

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
    $statePath       = Join-Path $queueDir "state.json"
   
    $instructionPath = Join-Path $queueDir "instruction.txt"
    $coderPromptPath = Join-Path $queueDir "coder-prompt.txt"
    $resultPath      = Join-Path $queueDir "result.txt"
    $outputPath      = Join-Path $queueDir "executor-output.txt"

   if (-not (Test-Path $instructionPath)) { Write-Log "HATA: instruction.txt bulunamadi."; return }
if (-not (Test-Path $statePath)) { Write-Log "HATA: state.json bulunamadi."; return }

    try { $stateObj = Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json }
    catch { Write-Log "HATA: state.json okunamadi."; return }

    $iteration = if ($stateObj.PSObject.Properties["current_iteration"]) { [int]$stateObj.current_iteration } else { 0 }

    Set-SP $stateObj "status"      "executing"
    Set-SP $stateObj "last_error"  $null
    Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8

    $instructionText = Get-Content $instructionPath -Raw -Encoding UTF8
$planText = $instructionText

    $coderSystemPath = Join-Path $promptsDir "coder-system.txt"
    if (-not (Test-Path $coderSystemPath)) { Write-Log "HATA: coder-system.txt bulunamadi."; return }
    $coderSystem = Get-Content $coderSystemPath -Raw -Encoding UTF8

    # ── CONTEXT ENRICHMENT ──────────────────────────────────────────
    # Plan'daki [API_ROOT] / [WEB_ROOT] placeholder'larini gercek path'e coz
    $repoRoot = Split-Path $automationDir -Parent

    # Otomatik proje koku tespiti
    $apiRoot = ""
    $webRoot = ""
    $csprojFiles = @(Get-ChildItem -Path $repoRoot -Recurse -Filter "*.csproj" -ErrorAction SilentlyContinue | Select-Object -First 1)
    if ($csprojFiles.Count -gt 0) { $apiRoot = $csprojFiles[0].DirectoryName }

    $packageJsonFiles = @(Get-ChildItem -Path $repoRoot -Recurse -Filter "package.json" -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch "node_modules" } | Select-Object -First 1)
    if ($packageJsonFiles.Count -gt 0) { $webRoot = $packageJsonFiles[0].DirectoryName }

    Write-Log "Context: apiRoot=$apiRoot webRoot=$webRoot"

    # Plan'daki referans dosyalari bul ve oku — [API_ROOT] ve gercek path'ler
    $contextBlocks = [System.Text.StringBuilder]::new()
    [void]$contextBlocks.AppendLine("=== PROJE DOSYA ICERIKLERI (Claude tarafindan okundu) ===")
    [void]$contextBlocks.AppendLine("")

    # Plan'da gecen .cs, .ts, .tsx, .json (package/program) dosya referanslarini yakala
    $filePatterns = [regex]::Matches($planText, '`[^`]+\.(cs|ts|tsx|json|txt|csproj)`|(?:cat|type)\s+"?([^">\s]+\.[a-z]+)"?')
    $resolvedFiles = [System.Collections.Generic.HashSet[string]]::new()

    foreach ($m in $filePatterns) {
        $raw = $m.Value -replace '^`|`$' -replace '^(cat|type)\s+"?' -replace '"?$'
        $raw = $raw.Trim()
        if ([string]::IsNullOrWhiteSpace($raw)) { continue }

        # [API_ROOT] placeholder'ini gercek path ile degistir
        $raw = $raw -replace '\[API_ROOT\]', $apiRoot -replace '\[WEB_ROOT\]', $webRoot

        if (-not [System.IO.Path]::IsPathRooted($raw)) {
            $raw = Join-Path $repoRoot $raw
        }
        if (-not $resolvedFiles.Add($raw)) { continue }

        if (Test-Path $raw) {
            $content = Get-Content $raw -Raw -Encoding UTF8 -ErrorAction SilentlyContinue
            [void]$contextBlocks.AppendLine("--- FILE: $raw ---")
            [void]$contextBlocks.AppendLine($content)
            [void]$contextBlocks.AppendLine("")
            Write-Log "Dosya okundu: $raw"
        }
    }

    # Her zaman ekle: AuthController, AuthService, IAuthService, Program.cs (varsa)
    $alwaysRead = @("AuthController.cs","AuthService.cs","IAuthService.cs","Program.cs","AuthModels.cs")
    foreach ($fname in $alwaysRead) {
        $found = Get-ChildItem -Path $repoRoot -Recurse -Filter $fname -ErrorAction SilentlyContinue |
                 Where-Object { $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" } |
                 Select-Object -First 1
        if ($found -and $resolvedFiles.Add($found.FullName)) {
            $content = Get-Content $found.FullName -Raw -Encoding UTF8 -ErrorAction SilentlyContinue
            [void]$contextBlocks.AppendLine("--- FILE: $($found.FullName) ---")
            [void]$contextBlocks.AppendLine($content)
            [void]$contextBlocks.AppendLine("")
            Write-Log "Otomatik eklendi: $($found.FullName)"
        }
    }

    $contextSection = $contextBlocks.ToString()
    # ── CONTEXT ENRICHMENT SONU ─────────────────────────────────────

    $coderPrompt = @"
$coderSystem

=== GOREV ===

$instructionText

$planText

$contextSection

=== TALIMATLAR ===
- Yukarida FILE bloklari olarak verilen dosya iceriklerini kullan. Bu dosyalari tekrar okuman GEREKMEZ.
- [API_ROOT] = $apiRoot
- [WEB_ROOT] = $webRoot
- Dosyalari DOGRUDAN bu tam path'lere yaz. Baska path deneme.
- find, cat, ls gibi kesfetme komutlari calistirma — bilgi sana yukarida verildi.
- Sadece kod yaz, dosyalara kaydet, build al, result.txt'e yaz.

Final response requirements:
- Briefly list the files changed (full paths).
- Build result (success/fail + error if any).
- Show the git commit message used.
"@
    $coderPrompt | Set-Content $coderPromptPath -Encoding UTF8
    Write-Log "coder-prompt.txt olusturuldu (context enriched, $($resolvedFiles.Count) dosya eklendi)."

    $tmpDir = Join-Path $automationDir "queue"
if (-not (Test-Path $tmpDir)) { New-Item -ItemType Directory -Path $tmpDir -Force | Out-Null }

$tempPrompt = Join-Path $tmpDir "executor-prompt.txt"
$tempOutput = Join-Path $tmpDir "gemini-out-$iteration.txt"
$tempBat    = Join-Path $tmpDir "run-gemini-$iteration.bat"

$coderPrompt | Set-Content $tempPrompt -Encoding UTF8

    $maxRetries     = 5
    $retryCount     = 0
    $exitCode       = 1
    $executorOutput = ""

    while ($retryCount -lt $maxRetries) {
        $retryCount++
        if ($retryCount -gt 1) {
            Write-Log "Gemini yeniden deneniyor ($retryCount / $maxRetries)..."
            Start-Sleep -Seconds 5
        }

        # Her deneme icin temiz output dosyasi
        if (Test-Path $tempOutput) { Remove-Item $tempOutput -Force }

        try {
            # CI=true ve NO_COLOR=1 → Gemini PTY açmaya çalışmaz, headless çalışır
            $batContent = @"
@echo off
set GOOGLE_GEMINI_BASE_URL=http://127.0.0.1:8045
set GEMINI_API_KEY=$($env:GEMINI_API_KEY)
set BEACHGO_ANTHROPIC_KEY=$($env:BEACHGO_ANTHROPIC_KEY)
set CI=true
set NO_COLOR=1
set TERM=dumb
cd /d "$repoRoot"
gemini --approval-mode yolo --model gemini-3-flash -p "@$tempPrompt" > "$tempOutput" 2>&1
exit /b %ERRORLEVEL%
"@
            $batContent | Set-Content $tempBat -Encoding ASCII

            Write-Log "Gemini baslatiliyor (iteration=$iteration, deneme=$retryCount)..."

            # WindowStyle Hidden: gercek console var ama gizli — PTY attach sorunu olmaz
            $proc = Start-Process cmd.exe `
                -ArgumentList "/c `"$tempBat`"" `
                -WorkingDirectory $automationDir `
                -WindowStyle Hidden `
                -Wait `
                -PassThru

            $exitCode = $proc.ExitCode

            # Ciktiyi oku
            if (Test-Path $tempOutput) {
                $executorOutput = Get-Content $tempOutput -Raw -Encoding UTF8
            } else {
                $executorOutput = ""
            }

            # Konsola yazdir
            if (-not [string]::IsNullOrWhiteSpace($executorOutput)) {
                $executorOutput -split "`n" | ForEach-Object {
                    if ($_.Trim()) { Write-Host "[GEMINI] $_" -ForegroundColor Cyan }
                }
            }

            Write-Log "Gemini tamamlandi. ExitCode=$exitCode Cikti uzunlugu=$($executorOutput.Length) karakter"

            # Kota hatasi kontrolu
            if ($executorOutput -match "QUOTA_EXHAUSTED|429|rate.limit|exhausted|All accounts exhausted") {
                Write-Log "Kota hatasi alindi, 10 saniye bekleniyor..."
                Start-Sleep -Seconds 10
                continue
            }

            # Basarili cikis
            if ($exitCode -eq 0 -and -not [string]::IsNullOrWhiteSpace($executorOutput)) {
                break
            }

        } catch {
            $exitCode       = 1
            $executorOutput = $_ | Out-String
            Write-Log "Gemini exception: $executorOutput"
        }
    }

    # Sonuclari dosyalara yaz
    $executorOutput | Set-Content $outputPath -Encoding UTF8
    $executorOutput | Set-Content $resultPath  -Encoding UTF8

    # Gereksiz MCP loglarini temizle
    $raw   = Get-Content $resultPath -Raw -Encoding UTF8
    $clean = ($raw -split "`r`n|`n") | Where-Object {
        $_.Trim() -ne "" -and
        $_ -notmatch "Registering notification handlers for server" -and
        $_ -notmatch "Scheduling MCP context refresh" -and
        $_ -notmatch "Executing MCP context refresh" -and
        $_ -notmatch "MCP context refresh complete" -and
        $_ -notmatch "browsermcp" -and
        $_ -notmatch "Connected to MCP server" -and
        $_ -notmatch "MCP server started" -and
        $_ -notmatch "YOLO mode is enabled" -and
        $_ -notmatch "^(INFO|DEBUG|TRACE)\b"
    }
    Set-Content -Path $resultPath -Value ($clean -join "`r`n") -Encoding UTF8

    if ($exitCode -ne 0) {
        $preview = $executorOutput.Substring(0, [Math]::Min(500, $executorOutput.Length))
        Write-Log "Gemini basarisiz. ExitCode=$exitCode`n$preview"
    }

    $resultText = Get-Content $resultPath -Raw -Encoding UTF8

    if ([string]::IsNullOrWhiteSpace($resultText)) {
        Write-Log "Executor result.txt uretmedi."
        $resultText = "(empty)"
    }

    # Degisen dosyalari yakala (satir basinda mutlak path veya relative path)
    $changedFiles = [regex]::Matches($resultText, '(?m)^[-+]\s*(.+\.(cs|ts|tsx|json|csproj|txt))') |
        ForEach-Object { $_.Groups[1].Value.Trim() } | Select-Object -Unique

    # Sadece 3 alan guncelle — is_complete'e dokunma
    Set-SP $stateObj "last_result"        ($resultText.Substring(0, [Math]::Min(800, $resultText.Length)))
    Set-SP $stateObj "last_exit_code"     $exitCode
    Set-SP $stateObj "last_files_changed" ($changedFiles -join ", ")
    Set-SP $stateObj "lastUpdated"        (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8

    Write-Log "Executor tamamlandi. ExitCode=$exitCode"
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