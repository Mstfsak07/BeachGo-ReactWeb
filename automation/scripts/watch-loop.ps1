$ErrorActionPreference = "Continue"

$scriptsDir = $PSScriptRoot
$automationDir = Split-Path $scriptsDir -Parent
$queueDir = Join-Path $automationDir "queue"
$taskFile = Join-Path $queueDir "task.txt"
$planFile = Join-Path $queueDir "plan.txt"

Write-Host "Starting Autonomous Watch Loop..."
Write-Host "Watching for changes in task.txt and plan.txt"

$lastTaskWrite = if (Test-Path $taskFile) { (Get-Item $taskFile).LastWriteTime } else { [DateTime]::MinValue }
$lastPlanWrite = if (Test-Path $planFile) { (Get-Item $planFile).LastWriteTime } else { [DateTime]::MinValue }

while ($true) {
    Start-Sleep -Seconds 3
    
    if (Test-Path $taskFile) {
        $currentTaskWrite = (Get-Item $taskFile).LastWriteTime
        if ($currentTaskWrite -gt $lastTaskWrite) {
            Write-Host "`n[Watcher] task.txt changed. Triggering Planner..."
            & (Join-Path $scriptsDir "run-planner.ps1")
            $lastTaskWrite = $currentTaskWrite
            if (Test-Path $planFile) {
                $lastPlanWrite = (Get-Item $planFile).LastWriteTime
            }
        }
    }
    
    if (Test-Path $planFile) {
        $currentPlanWrite = (Get-Item $planFile).LastWriteTime
        if ($currentPlanWrite -gt $lastPlanWrite) {
            Write-Host "`n[Watcher] plan.txt changed. Triggering Executor..."
            & (Join-Path $scriptsDir "run-executor.ps1")
            $lastPlanWrite = $currentPlanWrite
            if (Test-Path $taskFile) {
                $lastTaskWrite = (Get-Item $taskFile).LastWriteTime
            }
        }
    }
}