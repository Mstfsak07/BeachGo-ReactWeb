$projectRoot = Resolve-Path "$PSScriptRoot\..\.."
Set-Location $projectRoot

while ($true) {
    Clear-Host

    Write-Host "[1/3] Running planner..."
    powershell -ExecutionPolicy Bypass -File "automation\scripts\run-planner.ps1"

    Write-Host ""
    Write-Host "[2/3] Building coder prompt..."
    powershell -ExecutionPolicy Bypass -File "automation\scripts\extract-coder-prompt.ps1"

    Write-Host ""
    Write-Host "[3/3] Running aider + ollama..."
    powershell -ExecutionPolicy Bypass -File "automation\scripts\run-coder.ps1"

    Write-Host ""
    Write-Host "Cycle complete. Waiting 10 seconds..."
    Start-Sleep -Seconds 10
}