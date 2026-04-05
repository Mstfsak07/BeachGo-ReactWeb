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

while ($true) {
    Start-Sleep -Seconds 5

    # State oku
    $state = $null
    if (Test-Path $statePath) {
        try { $state = Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json }
        catch { Write-Log "state.json parse hatasi."; continue }
    }
    $currentStatus = if ($state) { $state.status } else { "idle" }

    # task.txt degistiyse ve sistem musait ise → run-loop.ps1 tetikle
    if (Test-Path $taskFile) {
        $currentTaskWrite = (Get-Item $taskFile).LastWriteTime
        $isIdle = $currentStatus -in @("idle", "done", "halted")

        if ($currentTaskWrite -gt $lastTaskWrite -and $isIdle -and -not $loopRunning) {
            Write-Log "task.txt degisti ve sistem '$currentStatus'. Recursive loop baslatiliyor..."
            $lastTaskWrite = $currentTaskWrite
            $loopRunning   = $true

            # run-loop.ps1'i arka planda baslat
            $loopScript = Join-Path $scriptsDir "run-loop.ps1"
            $job = Start-Job -ScriptBlock {
                param($script)
                & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $script
            } -ArgumentList $loopScript

            Write-Log "run-loop.ps1 Job ID=$($job.Id) basladi."
        }
    }

    # Calisan job bitti mi kontrol et
    if ($loopRunning) {
        $jobs = Get-Job -State Completed -ErrorAction SilentlyContinue
        foreach ($j in $jobs) {
            Write-Log "Loop job tamamlandi (ID=$($j.Id)). Cikti:"
            $out = Receive-Job -Job $j -ErrorAction SilentlyContinue
            if ($out) { Write-Log ($out | Out-String) }
            Remove-Job -Job $j -Force
            $loopRunning = $false
            Write-Log "Sistem yeni task.txt bekleniyor."
        }

        # Job basarisiz/durdu mu?
        $failedJobs = Get-Job -State Failed -ErrorAction SilentlyContinue
        foreach ($j in $failedJobs) {
            Write-Log "Loop job BASARISIZ (ID=$($j.Id)):"
            $out = Receive-Job -Job $j -ErrorAction SilentlyContinue
            if ($out) { Write-Log ($out | Out-String) }
            Remove-Job -Job $j -Force
            $loopRunning = $false
        }
    }
}
