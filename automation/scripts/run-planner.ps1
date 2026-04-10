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

$PLANNER_MODEL = if ($env:BEACHGO_PLANNER_MODEL) { $env:BEACHGO_PLANNER_MODEL } else { "claude-sonnet-4-6" }
$PLANNER_MAX_RESULT_CHARS = if ($env:BEACHGO_PLANNER_RESULT_CHARS) { [int]$env:BEACHGO_PLANNER_RESULT_CHARS } else { 1200 }
$PLANNER_MAX_HISTORY_LINES = if ($env:BEACHGO_PLANNER_HISTORY_LINES) { [int]$env:BEACHGO_PLANNER_HISTORY_LINES } else { 8 }

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

function Test-IsQuotaError {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) { return $false }
    return $Text -match "QUOTA_EXHAUSTED|RATE_LIMIT_EXCEEDED|429|RESOURCE_EXHAUSTED|All accounts exhausted|quotaReset|Too Many Requests"
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

function Normalize-PlannerOutput {
    param(
        [string]$Text,
        [string]$RepoRoot,
        [string]$CurrentPhaseText
    )

    if ([string]::IsNullOrWhiteSpace($Text)) { return $Text }
    if ($Text -imatch "^\s*SYSTEM_COMPLETE\s*$") { return "SYSTEM_COMPLETE" }

    $normalized = $Text -replace "`r`n", "`n"
    $phasePaths = New-Object System.Collections.Generic.List[string]
    $phaseInline = [regex]::Match($CurrentPhaseText, '(?im)^\s*DOSYALAR:\s*(.+?)\s*$')
    if ($phaseInline.Success) {
        foreach ($item in ($phaseInline.Groups[1].Value -split ',')) {
            $candidate = $item.Trim().Trim('`')
            if ($candidate) { $phasePaths.Add($candidate) }
        }
    }
    foreach ($match in [regex]::Matches($CurrentPhaseText, '(?im)^\s*-\s*`?([^`\r\n]+?\.(cs|ts|tsx|json|txt|csproj|sql))`?\s*$')) {
        $candidate = $match.Groups[1].Value.Trim()
        if ($candidate) { $phasePaths.Add($candidate) }
    }
    $phasePaths = @($phasePaths | Select-Object -Unique)

    $validPathMap = @{}
    foreach ($phasePath in $phasePaths) {
        $candidate = if ([System.IO.Path]::IsPathRooted($phasePath)) { $phasePath } else { Join-Path $RepoRoot $phasePath }
        if (Test-Path $candidate) {
            $validPathMap[$candidate.ToLowerInvariant()] = $candidate
            $validPathMap[[System.IO.Path]::GetFileName($candidate).ToLowerInvariant()] = $candidate
        } else {
            $resolvedByName = Get-ChildItem -Path $RepoRoot -Recurse -File -ErrorAction SilentlyContinue |
                Where-Object {
                    $_.Name -ieq $phasePath -and
                    $_.FullName -notmatch "\\obj\\" -and
                    $_.FullName -notmatch "\\bin\\" -and
                    $_.FullName -notmatch "\\node_modules\\"
                } |
                Select-Object -First 1
            if ($resolvedByName) {
                $validPathMap[$resolvedByName.FullName.ToLowerInvariant()] = $resolvedByName.FullName
                $validPathMap[$resolvedByName.Name.ToLowerInvariant()] = $resolvedByName.FullName
            }
        }
    }
    $lines = $normalized -split "`n"

    $priority = ""
    $scope = ""
    $build = ""
    $commit = ""
    $fileLines = New-Object System.Collections.Generic.List[string]
    $taskLines = New-Object System.Collections.Generic.List[string]
    $testLines = New-Object System.Collections.Generic.List[string]

    $section = ""
    foreach ($line in $lines) {
        $trim = $line.Trim().Trim('`')
        if ([string]::IsNullOrWhiteSpace($trim)) { continue }

        if ($trim -match '^(ONCELIK|ÖNCELIK|ONCELiK):') { $priority = $trim; $section = ""; continue }
        if ($trim -match '^SCOPE:') { $scope = $trim; $section = ""; continue }
        if ($trim -match '^DOSYALAR:\s*(.+)$') {
            $section = "files"
            $inlineFiles = $Matches[1] -split ','
            foreach ($inlineFile in $inlineFiles) {
                $candidate = $inlineFile.Trim().Trim('`')
                if ($candidate) { $fileLines.Add("- $candidate") }
            }
            continue
        }
        if ($trim -eq 'DOSYALAR:') { $section = "files"; continue }
        if ($trim -eq 'GOREV:') { $section = "tasks"; continue }
        if ($trim -match '^BUILD:') { $build = $trim; $section = ""; continue }
        if ($trim -eq 'TEST:') { $section = "tests"; continue }
        if ($trim -match '^<commit_message>') { $commit = $trim; $section = ""; continue }

        switch ($section) {
            "files" {
                if ($trim -match '^-') {
                    $fileLines.Add($trim)
                } elseif ($trim -match '^[^:]+\.(cs|ts|tsx|json|txt|csproj|sql)$') {
                    $fileLines.Add("- $trim")
                }
            }
            "tasks" {
                if ($trim -match '^\d+\.') {
                    $taskLines.Add($trim)
                } elseif ($trim -match '^-') {
                    $taskLines.Add(("{0}. {1}" -f ($taskLines.Count + 1), $trim.TrimStart('-').Trim()))
                } else {
                    $taskLines.Add(("{0}. {1}" -f ($taskLines.Count + 1), $trim))
                }
            }
            "tests" {
                if ($trim -match '^\d+\.') {
                    $testLines.Add($trim)
                } elseif ($trim -match '^-') {
                    $testLines.Add(("{0}. {1}" -f ($testLines.Count + 1), $trim.TrimStart('-').Trim()))
                } else {
                    $testLines.Add(("{0}. {1}" -f ($testLines.Count + 1), $trim))
                }
            }
        }
    }

    if ($fileLines.Count -eq 0) {
        $fallbackFiles = [regex]::Matches($normalized, '(?im)([A-Za-z0-9_./\\-]+\.(cs|ts|tsx|json|txt|csproj|sql))') |
            ForEach-Object { $_.Groups[1].Value.Trim() } |
            Select-Object -Unique
        foreach ($file in ($fallbackFiles | Select-Object -First 2)) {
            $fileLines.Add("- $file")
        }
    }

    $resolvedFileLines = New-Object System.Collections.Generic.List[string]
    foreach ($line in $fileLines) {
        $rawPath = $line.Trim().TrimStart('-').Trim().Trim('`')
        if (-not $rawPath) { continue }
        $resolved = $null
        if ($validPathMap.ContainsKey($rawPath.ToLowerInvariant())) {
            $resolved = $validPathMap[$rawPath.ToLowerInvariant()]
        } elseif ($validPathMap.ContainsKey([System.IO.Path]::GetFileName($rawPath).ToLowerInvariant())) {
            $resolved = $validPathMap[[System.IO.Path]::GetFileName($rawPath).ToLowerInvariant()]
        } else {
            $candidate = if ([System.IO.Path]::IsPathRooted($rawPath)) { $rawPath } else { Join-Path $RepoRoot $rawPath }
            if (Test-Path $candidate) { $resolved = $candidate }
        }
        if ($resolved) { $resolvedFileLines.Add("- $resolved") }
    }

    if ($resolvedFileLines.Count -eq 0) {
        foreach ($phasePath in $phasePaths) {
            $candidate = $null
            if ($validPathMap.ContainsKey($phasePath.ToLowerInvariant())) {
                $candidate = $validPathMap[$phasePath.ToLowerInvariant()]
            } elseif ($validPathMap.ContainsKey([System.IO.Path]::GetFileName($phasePath).ToLowerInvariant())) {
                $candidate = $validPathMap[[System.IO.Path]::GetFileName($phasePath).ToLowerInvariant()]
            }
            if ($candidate) { $resolvedFileLines.Add("- $candidate") }
            if ($resolvedFileLines.Count -ge 2) { break }
        }
    }

    $fileLines = @($resolvedFileLines | Select-Object -First 2)
    $taskLines = @($taskLines | Select-Object -First 3)
    $testLines = @($testLines | Select-Object -First 2)

    if (-not $priority) { $priority = "ONCELIK: BUGFIX" }
    if (-not $scope) { $scope = "SCOPE: api" }
    if ($build) {
        $quotedBuildPath = [regex]::Match($build, '"([^"]+\.csproj)"')
        if ($quotedBuildPath.Success) {
            $candidateBuildPath = $quotedBuildPath.Groups[1].Value
            $resolvedBuildPath = if ([System.IO.Path]::IsPathRooted($candidateBuildPath)) { $candidateBuildPath } else { Join-Path $RepoRoot $candidateBuildPath }
            if (-not (Test-Path $resolvedBuildPath)) {
                $build = ""
            }
        }
    }
    if ($build -match 'src/Api|Api\.csproj|cd src/Api') { $build = "" }
    if (-not $build) {
        $buildSource = [regex]::Match($CurrentPhaseText, '(?im)^\s*BUILD:\s*(.+?)\s*$')
        if ($buildSource.Success) {
            $build = "BUILD: " + $buildSource.Groups[1].Value.Trim()
        } else {
            $defaultCsproj = Get-ChildItem -Path $RepoRoot -Recurse -Filter "*.csproj" -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" } |
                Select-Object -First 1
            if ($defaultCsproj) {
                $relativeCsproj = $defaultCsproj.FullName.Substring($RepoRoot.Length).TrimStart('\')
                $build = "BUILD: dotnet build `"$relativeCsproj`""
            } else {
                $build = "BUILD: dotnet build"
            }
        }
    }
    if (-not $commit) { $commit = "<commit_message>chore: planner normalized slice</commit_message>" }

    $out = New-Object System.Collections.Generic.List[string]
    $out.Add($priority)
    $out.Add($scope)
    $out.Add("DOSYALAR:")
    foreach ($line in $fileLines) { $out.Add($line) }
    $out.Add("")
    $out.Add("GOREV:")
    foreach ($line in $taskLines) { $out.Add($line) }
    $out.Add("")
    $out.Add($build)
    $out.Add("")
    $out.Add("TEST:")
    foreach ($line in $testLines) { $out.Add($line) }
    $out.Add("")
    $out.Add($commit)

    return ($out -join "`r`n")
}

function Invoke-ClaudeAPI {
    param([string]$Prompt, [string]$SystemPrompt = "")

    $apiKey  = $env:ANTHROPIC_API_KEY
    $baseUrl = if ($env:ANTHROPIC_BASE_URL) { $env:ANTHROPIC_BASE_URL.TrimEnd('/') } else { "https://api.anthropic.com" }

    $bodyObj = [ordered]@{
        model      = $PLANNER_MODEL
        max_tokens = 2200
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

    Write-Log "Anthropic API cagrisi: $baseUrl/v1/messages (model=$PLANNER_MODEL)"

    $response = Invoke-WebRequest `
        -Uri     "$baseUrl/v1/messages" `
        -Method  POST `
        -Headers $headers `
        -Body    $bodyBytes `
        -UseBasicParsing

    $stream = $response.RawContentStream
    $stream.Position = 0
    $reader = [System.IO.StreamReader]::new($stream, [System.Text.Encoding]::UTF8, $true)
    $responseJson = $reader.ReadToEnd()
    $reader.Dispose()

    $parsed = $responseJson | ConvertFrom-Json
    if ($parsed.content -and $parsed.content[0].text) {
        return $parsed.content[0].text
    } elseif ($parsed.text) {
        return $parsed.text
    } else {
        Write-Log "HATA: API beklenmedik format dondu. Yanit: $responseJson"
        return $null
    }
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
    $instructionPath     = Join-Path $queueDir "instruction.txt"
   
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

    $phasesContent = Repair-MojibakeText (Get-Content $phasesFile -Raw -Encoding UTF8)

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
        "SYSTEM_COMPLETE" | Set-Content $instructionPath -Encoding UTF8
        exit 0
    }

    # Diger verileri oku
    $resultText = if (Test-Path $resultPath) { Repair-MojibakeText (Get-Content $resultPath -Raw -Encoding UTF8) } else { "(bos)" }
    if ($resultText.Length -gt $PLANNER_MAX_RESULT_CHARS) {
        $resultText = $resultText.Substring(0, $PLANNER_MAX_RESULT_CHARS) + "`n...(truncated)"
    }

    $gitDiff = try {
        $d = & git -C $repoRoot diff --stat HEAD 2>&1 | Out-String
        if ([string]::IsNullOrWhiteSpace($d)) { "Git diff temiz." } else { $d.Trim() }
    } catch { "Git diff alinamadi." }

    $historySnippet = if (Test-Path $historyFile) {
        $lines = Get-Content $historyFile -Encoding UTF8 -ErrorAction SilentlyContinue
        if ($lines.Count -gt $PLANNER_MAX_HISTORY_LINES) { $lines[-$PLANNER_MAX_HISTORY_LINES..-1] -join "`n" } else { $lines -join "`n" }
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
    $systemPrompt = Repair-MojibakeText (Get-Content $systemPromptPath -Raw -Encoding UTF8)

    $buildSummary = if ($stateObj.PSObject.Properties["last_build_summary"] -and $stateObj.last_build_summary) {
        [string]$stateObj.last_build_summary
    } else { "(yok)" }
    $lastBuildExit = if ($stateObj.PSObject.Properties["last_build_exit_code"]) { $stateObj.last_build_exit_code } else { "(yok)" }

    $fullPrompt = @"
--- MEVCUT DURUM ---
Iteration: $iteration / $maxIter
Faz: $currentPhase / $totalPhases
Scope: $scope
Ardisik Hata Sayisi: $consErrors
Son Hata: $lastError
Son Build ExitCode: $lastBuildExit
Son Build Ozeti:
$buildSummary

--- SIRADAKI FAZ ---
$currentPhaseText

--- ÖNCEKİ SONUÇ (Gemini ne yaptı) ---
$resultText

--- GIT DIFF ---
$gitDiff

--- SON GECMIS ---
$historySnippet
"@

    Write-Log "Planner baslatiliyor (iteration=$iteration, faz=$currentPhase/$totalPhases)..."

    $env:ANTHROPIC_API_KEY  = $env:BEACHGO_ANTHROPIC_KEY
    $env:ANTHROPIC_BASE_URL = "http://127.0.0.1:8045"
    $env:CLAUDE_CONFIG_DIR  = "$env:USERPROFILE\.claude-antigravity"

    Remove-Item Env:MSYSTEM -ErrorAction SilentlyContinue
    Remove-Item Env:MSYS    -ErrorAction SilentlyContinue

    $maxRetries    = if ($env:BEACHGO_PLANNER_MAX_RETRIES) { [int]$env:BEACHGO_PLANNER_MAX_RETRIES } else { 12 }
    $retryCount    = 0
    $success       = $false
    $plannerOutput = ""
    $lastPlannerError = ""

    while ($retryCount -lt $maxRetries -and -not $success) {
        $retryCount++
        if ($retryCount -gt 1) {
            Write-Log "Yeniden deneniyor ($retryCount / $maxRetries)..."
            Write-Log "20 saniye bekleniyor..."
            Start-Sleep -Seconds 20
        }

        try {
            $plannerOutput = Invoke-ClaudeAPI -Prompt $fullPrompt -SystemPrompt $systemPrompt
            $plannerOutput = Repair-MojibakeText $plannerOutput
            Write-Log "Claude API yaniti (deneme $retryCount): $plannerOutput"
            Write-Host "=== CLAUDE YANITI (deneme $retryCount) ===`n$plannerOutput`n================================" -ForegroundColor Cyan
            if (-not [string]::IsNullOrWhiteSpace($plannerOutput)) {
                $success = $true
            } else {
                Write-Log "API bos cikti dondu. (Deneme $retryCount / $maxRetries)"
            }
        } catch {
            $lastPlannerError = $_ | Out-String
            Write-Log "API hatasi: $lastPlannerError (Deneme $retryCount / $maxRetries)"
            Write-Host "=== CLAUDE HATA (deneme $retryCount) ===`n$_`n================================" -ForegroundColor Red
            if (Test-IsQuotaError $lastPlannerError) {
                Set-SP $stateObj "status" "waiting_quota"
                Set-SP $stateObj "last_error" "Planner quota/rate-limit nedeniyle yeniden deniyor."
                Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
                $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
            }
        }
    }

    if (-not $success) {
        if (Test-IsQuotaError $lastPlannerError) {
            Write-Log "Planner kota nedeniyle plan uretemedi; faz korunuyor ve instruction degistirilmiyor."
            Set-SP $stateObj "status" "waiting_quota"
            Set-SP $stateObj "last_error" "Planner quota/rate-limit nedeniyle beklemede."
            Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
            $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
            return
        }
        Write-Log "KRITIK HATA: Claude API $maxRetries deneme sonrasinda yanit vermedi. Otomasyon durduruluyor."
        throw "Planner Claude API hatasi ($maxRetries deneme basarisiz oldu). Otomasyon durduruluyor."
    }

    $plannerOutput = Normalize-PlannerOutput -Text (Repair-MojibakeText $plannerOutput) -RepoRoot $repoRoot -CurrentPhaseText $currentPhaseText
    Write-Log "Normalize edilmis planner cikti:`n$plannerOutput"
    $plannerOutput | Set-Content $instructionPath    -Encoding UTF8
    

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
