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
    $planPath        = Join-Path $queueDir "plan.txt"
    $instructionPath = Join-Path $queueDir "instruction.txt"
    $coderPromptPath = Join-Path $queueDir "coder-prompt.txt"
    $resultPath      = Join-Path $queueDir "result.txt"
    $outputPath      = Join-Path $queueDir "executor-output.txt"

    if (-not (Test-Path $planPath))  { throw "plan.txt bulunamadi." }
    if (-not (Test-Path $statePath)) { throw "state.json bulunamadi." }

    try { $stateObj = Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json }
    catch { throw "state.json okunamadi." }

    $iteration = if ($stateObj.PSObject.Properties["current_iteration"]) { [int]$stateObj.current_iteration } else { 0 }

    Set-SP $stateObj "status"      "executing"
    Set-SP $stateObj "last_error"  $null
    Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8

    $planText        = Get-Content $planPath -Raw -Encoding UTF8
    $instructionText = if (Test-Path $instructionPath) { Get-Content $instructionPath -Raw -Encoding UTF8 } else { "" }

    $coderSystemPath = Join-Path $promptsDir "coder-system.txt"
    if (-not (Test-Path $coderSystemPath)) { throw "coder-system.txt bulunamadi." }
    $coderSystem = Get-Content $coderSystemPath -Raw -Encoding UTF8

    $coderPrompt = @"
$coderSystem

Task:

$instructionText

$planText

Final response requirements:
- Briefly list the files changed.
- Mention any remaining issue or follow-up work.
- Show the git commit message used.
"@
    $coderPrompt | Set-Content $coderPromptPath -Encoding UTF8
    Write-Log "coder-prompt.txt olusturuldu."

    $tempPrompt  = Join-Path $env:TEMP "executor-prompt-$iteration.txt"
    $tempOutput  = Join-Path $env:TEMP "gemini-out-$iteration.txt"
    $tempBat     = Join-Path $env:TEMP "run-gemini-$iteration.bat"

    $coderPrompt | Set-Content $tempPrompt -Encoding UTF8

    $maxRetries = 5
    $retryCount = 0
    $exitCode   = 1
    $executorOutput = ""

    while ($retryCount -lt $maxRetries) {
        $retryCount++
        if ($retryCount -gt 1) {
            Write-Log "Gemini yeniden deneniyor ($retryCount / $maxRetries)..."
            Start-Sleep -Seconds 5
        }

        # Her deneme icin temiz output dosyasi
        if (Test-Path $tempOutput) { Remove-Item $tempOutput -Force }

        # Bat dosyasini olustur - env variable'lar ve komut birlikte
 $batContent = @"
@echo off
set GOOGLE_GEMINI_BASE_URL=http://127.0.0.1:8045
set GEMINI_API_KEY=$($env:GEMINI_API_KEY)
set BEACHGO_ANTHROPIC_KEY=$($env:BEACHGO_ANTHROPIC_KEY)

gemini --approval-mode yolo --model gemini-3-flash < "$tempPrompt" > "$tempOutput" 2>&1
exit %ERRORLEVEL%
"@
$batContent | Set-Content $tempBat -Encoding ASCII

Write-Log "Gemini yeni terminalde baslatiliyor (iteration=$iteration, deneme=$retryCount)..."

$process = New-Object System.Diagnostics.Process
$process.StartInfo.FileName = "powershell.exe"
$process.StartInfo.Arguments = "-NoExit -Command `"cmd.exe /c '$tempBat'; exit`""
$process.StartInfo.UseShellExecute = $true
$process.StartInfo.CreateNoWindow = $false
$process.Start() | Out-Null
$process.WaitForExit()
$exitCode = $process.ExitCode

# Output dosyasını oku
if (Test-Path $tempOutput) {
    $executorOutput = Get-Content $tempOutput -Raw -Encoding UTF8
    # Konsola yaz
    $executorOutput -split "`n" | ForEach-Object {
        if ($_) { Write-Host "[GEMINI] $_" -ForegroundColor Cyan }
    }
} else {
    $executorOutput = ""
}

# kalanlar
while (-not $process.StandardOutput.EndOfStream) {
    $line = $process.StandardOutput.ReadLine()
    if ($line) {
        Write-Host "[GEMINI] $line" -ForegroundColor Cyan
    }
}

while (-not $process.StandardError.EndOfStream) {
    $line = $process.StandardError.ReadLine()
    if ($line) {
        Write-Host "[GEMINI ERROR] $line" -ForegroundColor Red
    }
}

$process.WaitForExit()
$exitCode = $process.ExitCode

            

            Write-Log "Gemini tamamlandi. ExitCode=$exitCode Cikti uzunlugu=$($executorOutput.Length) karakter"

            if ($exitCode -eq 0 -and -not [string]::IsNullOrWhiteSpace($executorOutput)) {
                break
            }

            # Kota hatasi kontrolu
            if ($executorOutput -match "QUOTA_EXHAUSTED|429|rate.limit|exhausted|All accounts exhausted") {
                Write-Log "Kota hatasi alindi, 10 saniye bekleniyor..."
                Start-Sleep -Seconds 10
            }
        }
        catch {
            $exitCode = 1
            $executorOutput = $_ | Out-String
            Write-Log "Gemini exception: $executorOutput"
        }
    

    $executorOutput | Set-Content $outputPath -Encoding UTF8

    # result.txt'e yaz
    $executorOutput | Set-Content $resultPath -Encoding UTF8

    # Gereksiz MCP loglarini temizle
    $raw = Get-Content $resultPath -Raw -Encoding UTF8
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
        throw "Gemini basarisiz. ExitCode=$exitCode`n$preview"
    }

    $resultText = Get-Content $resultPath -Raw -Encoding UTF8

    if ([string]::IsNullOrWhiteSpace($resultText)) {
        throw "Executor result.txt uretmedi."
    }

    $failPatterns = @(
        '"status"\s*:\s*"failed"',
        '"phase"\s*:\s*"failed"',
        'CRITICAL ERROR',
        'FATAL'
    )
    $isFailed = $false
    foreach ($p in $failPatterns) {
        if ($resultText -imatch $p) { $isFailed = $true; break }
    }

    if ($isFailed) {
        Set-SP $stateObj "status"     "failed"
        Set-SP $stateObj "last_error" ($resultText.Substring(0,[Math]::Min(500,$resultText.Length)))
        Write-Log "Executor basarisiz sonuc dondu."
    } else {
        Set-SP $stateObj "status"     "completed"
        Set-SP $stateObj "last_error" $null
        Write-Log "Executor tamamlandi."
    }

    $summary = if ($resultText.Length -gt 400) { $resultText.Substring(0,400) + "..." } else { $resultText }
    Set-SP $stateObj "last_result_summary" $summary
    Set-SP $stateObj "lastUpdated" (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    $stateObj | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
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
