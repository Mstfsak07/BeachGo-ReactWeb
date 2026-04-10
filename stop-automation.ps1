#Requires -Version 5.1
$ErrorActionPreference = "Continue"

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding  = [System.Text.Encoding]::UTF8
$OutputEncoding            = [System.Text.Encoding]::UTF8

$repoRoot = $PSScriptRoot
$queueDir = Join-Path $repoRoot "automation\queue"

Write-Host "[STOP] Automation/orchestrator processleri durduruluyor..."

$targets = Get-CimInstance Win32_Process | Where-Object {
    ($_.Name -eq 'node.exe' -and $_.CommandLine -match 'orchestrator\.js|@google\\gemini-cli') -or
    ($_.Name -eq 'powershell.exe' -and $_.CommandLine -match 'automation\\scripts\\run-loop\.ps1|automation\\scripts\\watch-loop\.ps1|automation\\scripts\\run-planner\.ps1') -or
    ($_.Name -eq 'cmd.exe' -and $_.CommandLine -match 'gemini')
}

foreach ($proc in $targets) {
    try {
        Stop-Process -Id $proc.ProcessId -Force
        Write-Host "[STOP] $($proc.Name) PID=$($proc.ProcessId)"
    } catch {
        Write-Host "[WARN] Process durdurulamadi: PID=$($proc.ProcessId)"
    }
}

Write-Host "[STOP] Lock dosyalari temizleniyor..."
Remove-Item (Join-Path $queueDir "planner.lock"), (Join-Path $queueDir "executor.lock") -Force -ErrorAction SilentlyContinue

Write-Host "[STOP] Tamam."
