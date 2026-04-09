#Requires -Version 5.1
$ErrorActionPreference = "Continue"

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding  = [System.Text.Encoding]::UTF8
$OutputEncoding            = [System.Text.Encoding]::UTF8

$scriptsDir    = $PSScriptRoot
$automationDir = Split-Path $scriptsDir -Parent
$queueDir      = Join-Path $automationDir "queue"
$logFile       = Join-Path $queueDir "automation.log"
$statePath     = Join-Path $queueDir "state.json"
$taskFile      = Join-Path $queueDir "task.txt"

function Write-Log($msg) {
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [WATCHER] $msg"
    Write-Host $line
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

# Stale lock temizle
foreach ($lockName in @("planner.lock", "executor.lock")) {
    $lf = Join-Path $queueDir $lockName
    if (Test-Path $lf) {
        $age = (Get-Date) - (Get-Item $lf).LastWriteTime
        if ($age.TotalMinutes -gt 10) {
            Remove-Item $lf -Force
            Write-Log "Stale $lockName temizlendi."
        }
    }
}

Write-Log "Watch Loop baslatildi. task.txt degisikligini izliyor..."

$lastTaskWrite = if (Test-Path $taskFile) { (Get-Item $taskFile).LastWriteTime } else { [DateTime]::MinValue }
$loopRunning   = $false
$job           = $null

while ($true) {
    Start-Sleep -Seconds 5

    # State oku
    $state = $null
    if (Test-Path $statePath) {
        try { $state = Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json }
        catch { Write-Log "state.json parse hatasi."; continue }
    }
    $currentStatus = if ($state) { $state.status } else { "idle" }

    # task.txt degistiyse ve sistem musait ise -> run-loop.ps1 tetikle
    if (Test-Path $taskFile) {
        $currentTaskWrite = (Get-Item $taskFile).LastWriteTime

        # Job hala calisip calismadığını guvenlice kontrol et
        $hasActiveJob = $false
        if ($loopRunning -and $null -ne $job) {
            $runningJob = Get-Job -Id $job.Id -ErrorAction SilentlyContinue
            $hasActiveJob = ($null -ne $runningJob -and $runningJob.State -eq 'Running')
        }

        # "running" veya "executing" durumunda 15 dakika lastUpdated timeout'u
        $stateTimedOut = $false
        if ($currentStatus -in @("running", "executing") -and $state.lastUpdated) {
            try {
                $lastUp = [DateTime]::Parse($state.lastUpdated)
                $stateTimedOut = ((Get-Date) - $lastUp).TotalMinutes -gt 15
                if ($stateTimedOut) { Write-Log "State '$currentStatus' 15 dakikadir guncellenmedi, timeout sayildi." }
            } catch {}
        }

        $isIdle = ($currentStatus -in @("idle", "done", "halted", "error", "failed")) -or
                  ($currentStatus -in @("running", "executing") -and (-not $hasActiveJob -or $stateTimedOut))

        if ($currentTaskWrite -gt $lastTaskWrite -and $isIdle -and -not $loopRunning) {
            Write-Log "task.txt degisti (yeni: $currentTaskWrite, onceki: $lastTaskWrite) ve sistem '$currentStatus'. Recursive loop baslatiliyor..."
            $lastTaskWrite = $currentTaskWrite
            $loopRunning   = $true

            # run-loop.ps1'i arka planda baslat
            $loopScript = Join-Path $scriptsDir "run-loop.ps1"
            $keyVal = $env:BEACHGO_ANTHROPIC_KEY
            $job = Start-Job -ScriptBlock {
                param($script, $key)
                $env:BEACHGO_ANTHROPIC_KEY = $key
                $env:ANTHROPIC_API_KEY     = $key
                $env:ANTHROPIC_BASE_URL    = "http://127.0.0.1:8045"
                & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $script -ApiKey $key
            } -ArgumentList $loopScript, $keyVal

            Write-Log "run-loop.ps1 Job ID=$($job.Id) basladi."
        }
    }

    # Calisan job bitti mi kontrol et
    if ($loopRunning -and $null -ne $job) {
        $currentJob = Get-Job -Id $job.Id -ErrorAction SilentlyContinue
        if ($null -eq $currentJob) {
            Write-Log "Job bulunamadi, loopRunning sifirlaniyor."
            $loopRunning = $false
            $job = $null
        }
        elseif ($currentJob.State -in @('Completed', 'Failed', 'Stopped')) {
            $stateLabel = $currentJob.State
            Write-Log "Loop job $stateLabel (ID=$($currentJob.Id)). Cikti:"
            $out = Receive-Job -Job $currentJob -ErrorAction SilentlyContinue
            if ($out) { Write-Log ($out | Out-String) }
            Remove-Job -Job $currentJob -Force
            $loopRunning = $false
            $job = $null
            Write-Log "Sistem yeni task.txt bekleniyor."
        }
    }
}
