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

function Test-IsGeminiBoilerplate {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) { return $true }

    $meaningful = ($Text -split "`r`n|`n") | Where-Object {
        $line = $_.Trim()
        $line -and
        $line -notmatch "^YOLO mode is enabled" -and
        $line -notmatch "^Both GOOGLE_API_KEY and GEMINI_API_KEY are set" -and
        $line -notmatch "^Scheduling MCP context refresh" -and
        $line -notmatch "^Executing MCP context refresh" -and
        $line -notmatch "^MCP context refresh complete" -and
        $line -notmatch "^Connected to MCP server" -and
        $line -notmatch "^MCP server started"
    }

    return $meaningful.Count -eq 0
}

function Add-ContextFile {
    param(
        [System.Text.StringBuilder]$Builder,
        [string]$Path,
        [int]$MaxCharsPerFile = 2500
    )

    $content = Get-Content $Path -Raw -Encoding UTF8 -ErrorAction SilentlyContinue
    if ($null -eq $content) { $content = "" }
    if ($content.Length -gt $MaxCharsPerFile) {
        $content = $content.Substring(0, $MaxCharsPerFile) + "`n... [truncated]"
    }

    [void]$Builder.AppendLine("--- FILE: $Path ---")
    [void]$Builder.AppendLine($content)
    [void]$Builder.AppendLine("")
}

function Test-IsQuotaError {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) { return $false }
    return $Text -match "QUOTA_EXHAUSTED|RATE_LIMIT_EXCEEDED|429|RESOURCE_EXHAUSTED|All accounts exhausted|quota will reset|Too Many Requests"
}

function Get-MojibakeScore {
    param([string]$Text)

    if ([string]::IsNullOrEmpty($Text)) { return 0 }
    return ([regex]::Matches($Text, 'Ã|Ä|Å|â|�|ğŸ|Â')).Count
}

function Repair-MojibakeText {
    param([string]$Text)

    if ([string]::IsNullOrEmpty($Text)) { return $Text }

    $current = $Text
    for ($round = 0; $round -lt 3; $round++) {
        $originalScore = Get-MojibakeScore $current
        if ($originalScore -eq 0) { break }

        $candidates = @()
        foreach ($encName in @("windows-1254", "windows-1252")) {
            try {
                $enc = [System.Text.Encoding]::GetEncoding($encName)
                $candidate = [System.Text.Encoding]::UTF8.GetString($enc.GetBytes($current))
                $candidates += [PSCustomObject]@{
                    Text  = $candidate
                    Score = Get-MojibakeScore $candidate
                }
            } catch {}
        }

        $best = $candidates | Sort-Object Score | Select-Object -First 1
        if ($best -and $best.Score -lt $originalScore) {
            $current = $best.Text
        } else {
            break
        }
    }

    return $current
}

function Test-IsExecutorDrift {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) { return $false }

    return $Text -match "(?im)\bI will\b|\bI'll\b|once .*kontrol|kontrol edecegim|searching for|read the|locate the|first,?\s+i'?ll|let me\s|inspecting|opening the file|read_file"
}

function Get-FileFingerprintMap {
    param([string[]]$Paths)

    $map = @{}
    foreach ($path in ($Paths | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)) {
        if (Test-Path $path) {
            try {
                $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $path -ErrorAction Stop).Hash
                $map[$path] = "existing:$hash"
            } catch {
                $map[$path] = "existing:(hash-error)"
            }
        } else {
            $map[$path] = "missing"
        }
    }
    return $map
}

function Get-BatchEnvValue {
    param(
        [string]$BatchPath,
        [string]$VariableName
    )

    if (-not (Test-Path $BatchPath)) { return "" }

    $pattern = "^\s*set\s+$([regex]::Escape($VariableName))=(.*)$"
    foreach ($line in Get-Content $BatchPath -Encoding UTF8 -ErrorAction SilentlyContinue) {
        $match = [regex]::Match($line, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($match.Success) {
            return $match.Groups[1].Value.Trim()
        }
    }

    return ""
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

    $instructionText = Repair-MojibakeText (Get-Content $instructionPath -Raw -Encoding UTF8)
    $planText = $instructionText

    $geminiModel = if ($env:BEACHGO_GEMINI_MODEL) { $env:BEACHGO_GEMINI_MODEL } else { "gemini-3.1-pro" }
    if ($instructionText -imatch "\[SECURITY\]|\[REFACTOR\]|guvenlik|security|audit|mimari|architecture|refactor") {
        $geminiModel = if ($env:BEACHGO_GEMINI_PRO_MODEL) { $env:BEACHGO_GEMINI_PRO_MODEL } else { "gemini-3.1-pro" }
        Write-Log "Karmasik gorev tespit edildi, PRO model kullaniliyor: $geminiModel"
    } else {
        Write-Log "Standart gorev, FLASH model kullaniliyor: $geminiModel"
    }

    $coderSystemPath = Join-Path $promptsDir "coder-system.txt"
    if (-not (Test-Path $coderSystemPath)) { Write-Log "HATA: coder-system.txt bulunamadi."; return }

    $workflowPromptPath = $null
    if ($instructionText -imatch "\[SECURITY\]|guvenlik|security|audit") {
        $workflowPromptPath = Join-Path $promptsDir "security-audit-system.txt"
    } elseif ($instructionText -imatch "\[BUGFIX\]|hata|bug|fix-cycle|fix") {
        $workflowPromptPath = Join-Path $promptsDir "fix-cycle-system.txt"
    } elseif ($instructionText -imatch "\[FEATURE\]|ozellik|feature|build-cycle") {
        $workflowPromptPath = Join-Path $promptsDir "build-cycle-system.txt"
    }

    $coderSystem = Repair-MojibakeText (Get-Content $coderSystemPath -Raw -Encoding UTF8)
    if ($workflowPromptPath -and (Test-Path $workflowPromptPath)) {
        $workflowSystem = Repair-MojibakeText (Get-Content $workflowPromptPath -Raw -Encoding UTF8)
        $coderSystem = $coderSystem + "`n`n=== EK WORKFLOW TALIMATLARI ===`n" + $workflowSystem
        Write-Log "Workflow prompt eklendi: $workflowPromptPath"
    }

    # -- CONTEXT ENRICHMENT ------------------------------------------
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

    # Plan'daki referans dosyalari bul ve oku - [API_ROOT] ve gercek path'ler
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
            Add-ContextFile -Builder $contextBlocks -Path $raw
            Write-Log "Dosya okundu: $raw"
        }
    }

    $instructionFileList = [regex]::Matches($instructionText, '(?im)^\s*-\s*`?([^`]+?\.(cs|ts|tsx|json|txt|csproj|sql))`?\s*$') |
        ForEach-Object { $_.Groups[1].Value.Trim() } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object {
            if ([System.IO.Path]::IsPathRooted($_)) { $_ } else { Join-Path $repoRoot $_ }
        } |
        Select-Object -Unique

    foreach ($instructionFile in $instructionFileList) {
        if ($resolvedFiles.Add($instructionFile) -and (Test-Path $instructionFile)) {
            Add-ContextFile -Builder $contextBlocks -Path $instructionFile
            Write-Log "Instruction dosyasi okundu: $instructionFile"
        }
    }

    $beforeFingerprints = Get-FileFingerprintMap -Paths $instructionFileList

    $scopeMatch = [regex]::Match($instructionText, '(?im)^\s*SCOPE:\s*(web|api|mobile)\s*$')
    $declaredScope = if ($scopeMatch.Success) { $scopeMatch.Groups[1].Value.ToLowerInvariant() } else { "" }
    $isFrontendTask = ($declaredScope -eq "web") -or ($instructionText -imatch "\blogin\b|\bregister\b|\bdark mode\b|\bdark-mode\b|\btema\b|\btheme\b|\bui\b|\bnavbar\b|\breact\b|\bfrontend\b|\bcss\b|\btailwind\b")
    $isBackendTask = ($declaredScope -eq "api") -or ($instructionText -imatch "\bapi\b|\bbackend\b|\bcontroller\b|\bservice\b|\bauth\b|\bjwt\b|\btoken\b|\bendpoint\b|\bprogram\.cs\b|\bc#\b|\bdotnet\b")

    if ($declaredScope -eq "api") { $isFrontendTask = $false }
    if ($declaredScope -eq "web") { $isBackendTask = $false }

    # Gorev tipine gore otomatik baglam ekle
    $alwaysRead = @()
    if ($isFrontendTask) {
        $alwaysRead += @(
            "Login.jsx",
            "Register.jsx",
            "App.js"
        )
    }
    if ($isBackendTask -or -not $isFrontendTask) {
        $alwaysRead += @(
            "AuthController.cs",
            "Program.cs"
        )
    }
    if ($isBackendTask -and $instructionText -imatch "\btoken\b|\bauth\b|\brefresh\b|\blogout\b|\bjwt\b|\bblacklist\b") {
        $alwaysRead += @(
            "ITokenService.cs",
            "AuthModels.cs",
            "BusinessUser.cs",
            "RefreshToken.cs",
            "RevokedToken.cs",
            "BeachDbContext.cs",
            "JwtBlacklistMiddleware.cs"
        )
    }

    $alwaysRead = $alwaysRead | Select-Object -Unique
    foreach ($fname in $alwaysRead) {
        $found = Get-ChildItem -Path $repoRoot -Recurse -Filter $fname -ErrorAction SilentlyContinue |
                 Where-Object {
                     $_.FullName -notmatch "\\obj\\" -and
                     $_.FullName -notmatch "\\bin\\" -and
                     $_.FullName -notmatch "\\node_modules\\"
                 } |
                 Select-Object -First 1
        if ($found -and $resolvedFiles.Add($found.FullName)) {
            Add-ContextFile -Builder $contextBlocks -Path $found.FullName
            Write-Log "Otomatik eklendi: $($found.FullName)"
        }
    }

    $contextSection = $contextBlocks.ToString()
    # -- CONTEXT ENRICHMENT SONU -------------------------------------

    $coderPrompt = @"
$coderSystem

=== GOREV ===

$instructionText

$contextSection

=== TALIMATLAR ===
- Yukarida FILE bloklari olarak verilen dosya iceriklerini kullan. Bu dosyalari tekrar okuman GEREKMEZ.
- [API_ROOT] = $apiRoot
- [WEB_ROOT] = $webRoot
- Dosyalari DOGRUDAN bu tam path'lere yaz. Baska path deneme.
- find, cat, ls gibi kesfetme komutlari calistirma - bilgi sana yukarida verildi.
- Plan veya dusunce yazma. Hemen uygula; gerekli tum hedef dosyalar yukarida verildi.
- Hedef dosya yoksa veya baglam yetmiyorsa tahmin yurutme; verilen dosyalarla sinirli kal.
- Son cikti sadece sonuc olsun; "I will", "once sunu kontrol edecegim" gibi ifadeler kullanma.

Final response requirements:
- Briefly list the files changed (full paths).
- Build result: `not-run`.
- Show the git commit message used.
"@
    $coderPrompt | Set-Content $coderPromptPath -Encoding UTF8
    Write-Log "coder-prompt.txt olusturuldu (context enriched, $($resolvedFiles.Count) dosya eklendi)."

    $tmpDir = Join-Path $automationDir "queue"
if (-not (Test-Path $tmpDir)) { New-Item -ItemType Directory -Path $tmpDir -Force | Out-Null }

$tempPrompt = Join-Path $tmpDir "executor-prompt.txt"
$tempOutput = Join-Path $tmpDir "gemini-out-$iteration.txt"

$coderPrompt | Set-Content $tempPrompt -Encoding UTF8

    $maxRetries     = if ($env:BEACHGO_GEMINI_MAX_RETRIES) { [int]$env:BEACHGO_GEMINI_MAX_RETRIES } else { 6 }
    $maxRunSeconds  = if ($env:BEACHGO_GEMINI_TIMEOUT_SEC) { [int]$env:BEACHGO_GEMINI_TIMEOUT_SEC } else { 180 }
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
            Write-Log "Gemini baslatiliyor (iteration=$iteration, deneme=$retryCount)..."

            $runnerBatchPath = Join-Path $queueDir "run-gemini-3.bat"
            $geminiBaseUrl = if ($env:GOOGLE_GEMINI_BASE_URL) {
                $env:GOOGLE_GEMINI_BASE_URL
            } else {
                $batchBaseUrl = Get-BatchEnvValue -BatchPath $runnerBatchPath -VariableName "GOOGLE_GEMINI_BASE_URL"
                if ($batchBaseUrl) { $batchBaseUrl } else { "http://127.0.0.1:8045" }
            }
            $geminiApiKey = if ($env:GEMINI_API_KEY) {
                $env:GEMINI_API_KEY
            } elseif ($env:GOOGLE_API_KEY) {
                $env:GOOGLE_API_KEY
            } elseif ($env:BEACHGO_GEMINI_API_KEY) {
                $env:BEACHGO_GEMINI_API_KEY
            } else {
                $batchApiKey = Get-BatchEnvValue -BatchPath $runnerBatchPath -VariableName "GOOGLE_API_KEY"
                if (-not $batchApiKey) {
                    $batchApiKey = Get-BatchEnvValue -BatchPath $runnerBatchPath -VariableName "GEMINI_API_KEY"
                }
                $batchApiKey
            }
            $anthropicApiKey = if ($env:BEACHGO_ANTHROPIC_KEY) { $env:BEACHGO_ANTHROPIC_KEY } else { "" }
            $maskedGeminiApiKey = if ($geminiApiKey.Length -ge 6) { $geminiApiKey.Substring(0, 6) + "..." } elseif ($geminiApiKey) { "***" } else { "(empty)" }
            Write-Log "Gemini env: baseUrl=$geminiBaseUrl apiKey=$maskedGeminiApiKey"

            $psi = [System.Diagnostics.ProcessStartInfo]::new()
            $psi.FileName = "cmd.exe"
            $escapedPromptPath = $tempPrompt.Replace('"', '""')
            $psi.Arguments = "/d /s /c ""chcp 65001>nul & gemini --approval-mode yolo --allowed-mcp-server-names disabled --model $geminiModel --output-format text -p ""@$escapedPromptPath"""""
            $psi.WorkingDirectory = $repoRoot
            $psi.UseShellExecute = $false
            $psi.RedirectStandardInput = $false
            $psi.RedirectStandardOutput = $true
            $psi.RedirectStandardError = $true
            $psi.CreateNoWindow = $true
            if ($psi.PSObject.Properties.Name -contains "StandardOutputEncoding") { $psi.StandardOutputEncoding = [System.Text.Encoding]::UTF8 }
            if ($psi.PSObject.Properties.Name -contains "StandardErrorEncoding") { $psi.StandardErrorEncoding = [System.Text.Encoding]::UTF8 }
            $psi.EnvironmentVariables["GOOGLE_GEMINI_BASE_URL"] = $geminiBaseUrl
            $psi.EnvironmentVariables["GOOGLE_API_KEY"] = $geminiApiKey
            $psi.EnvironmentVariables["BEACHGO_ANTHROPIC_KEY"] = $anthropicApiKey
            $psi.EnvironmentVariables["GEMINI_CLI_NO_RELAUNCH"] = "true"
            $psi.EnvironmentVariables["CI"] = "true"
            $psi.EnvironmentVariables["NO_COLOR"] = "1"
            $psi.EnvironmentVariables["TERM"] = "dumb"

            $proc = [System.Diagnostics.Process]::Start($psi)
            Write-Log "Gemini PID=$($proc.Id) baslatildi. Prompt dosyadan aktariliyor: $tempPrompt"

            $waited = $proc.WaitForExit($maxRunSeconds * 1000)
            if (-not $waited) {
                Write-Log "Gemini timeout oldu. Process tree sonlandiriliyor..."
                try { & taskkill /PID $proc.Id /T /F | Out-Null } catch {}
                Start-Sleep -Seconds 2
                $exitCode = 124
            } else {
                $exitCode = $proc.ExitCode
            }

            $stdout = $proc.StandardOutput.ReadToEnd()
            $stderr = $proc.StandardError.ReadToEnd()
            $executorOutput = @($stdout, $stderr) -join ""
            $executorOutput = Repair-MojibakeText $executorOutput
            $executorOutput | Set-Content $tempOutput -Encoding UTF8

            if ($exitCode -eq 124 -and [string]::IsNullOrWhiteSpace($executorOutput)) {
                $executorOutput = "Gemini timeout oldu ve cikti uretmeden sonlandirildi."
            }

            if (Test-IsGeminiBoilerplate $executorOutput) {
                Write-Log "Gemini anlamli cikti uretmedi; sadece boilerplate/MCP loglari dondu."
                if ($exitCode -eq 0) { $exitCode = 125 }
            }

            # Konsola yazdir
            if (-not [string]::IsNullOrWhiteSpace($executorOutput)) {
                $executorOutput -split "`n" | ForEach-Object {
                    if ($_.Trim()) { Write-Host "[GEMINI] $_" -ForegroundColor Cyan }
                }
            }

            Write-Log "Gemini tamamlandi. ExitCode=$exitCode Cikti uzunlugu=$($executorOutput.Length) karakter"

            # Kota hatasi kontrolu
            if (Test-IsQuotaError $executorOutput) {
                if ($retryCount -lt $maxRetries) {
                    Write-Log "Kota hatasi alindi. Faz kesilmeyecek; yeni hesap/fallback icin 20 saniye bekleniyor..."
                    Start-Sleep -Seconds 20
                } else {
                    Write-Log "Kota hatasi deneme limiti doldu ($retryCount/$maxRetries). Faz ayni durumda korunacak."
                }
                continue
            }

            if (Test-IsExecutorDrift $executorOutput) {
                Write-Log "Gemini uygulama yerine plan/arama moduna girdi."
                if ($exitCode -eq 0) { $exitCode = 126 }
            }

            if ($executorOutput -match "Error executing tool read_file: File not found") {
                Write-Log "Gemini verilen baglam yerine read_file aracina gitti ve dosya bulamadi."
                if ($exitCode -eq 0) { $exitCode = 127 }
            }

            if ($exitCode -eq 0 -and $executorOutput -notmatch "(?m)^SUMMARY:\s+" -and $executorOutput -notmatch "(?m)^KOK_NEDEN:\s+") {
                Write-Log "Gemini gecerli sonuc formati uretmedi."
                $exitCode = 128
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
    $executorOutput = Repair-MojibakeText $executorOutput
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

    $afterFingerprints = Get-FileFingerprintMap -Paths $instructionFileList
    $actualChangedTargets = @()
    foreach ($path in $instructionFileList) {
        if ($beforeFingerprints.ContainsKey($path) -and $afterFingerprints.ContainsKey($path)) {
            if ($beforeFingerprints[$path] -ne $afterFingerprints[$path]) {
                $actualChangedTargets += $path
            }
        }
    }

    if ($exitCode -eq 0 -and $instructionFileList.Count -gt 0 -and $actualChangedTargets.Count -eq 0) {
        Write-Log "Gemini basarili gorundu ama hedef dosyalarda gercek degisiklik yok."
        $exitCode = 129
    }

    # Degisen dosyalari yakala (satir basinda mutlak path veya relative path)
    $changedFiles = [regex]::Matches($resultText, '(?m)^[-+]\s*(.+\.(cs|ts|tsx|json|csproj|txt))') |
        ForEach-Object { $_.Groups[1].Value.Trim() } | Select-Object -Unique

    if ($actualChangedTargets.Count -gt 0) {
        $changedFiles = @($changedFiles + $actualChangedTargets) | Select-Object -Unique
    }

    # Sadece 3 alan guncelle - is_complete'e dokunma
    Set-SP $stateObj "last_result"        ($resultText.Substring(0, [Math]::Min(800, $resultText.Length)))
    Set-SP $stateObj "last_exit_code"     $exitCode
    Set-SP $stateObj "last_files_changed" ($changedFiles -join ", ")
    Set-SP $stateObj "status"             ($(if ($exitCode -eq 0) { "executed" } else { "executor_failed" }))
    Set-SP $stateObj "last_error"         ($(if ($exitCode -eq 0) { $null } else { "Executor failed with exit code $exitCode" }))
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
