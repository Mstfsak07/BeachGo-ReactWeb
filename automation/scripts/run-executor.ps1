$ErrorActionPreference = "Stop"

$scriptsDir = $PSScriptRoot
$automationDir = Split-Path $scriptsDir -Parent
$queueDir = Join-Path $automationDir "queue"
$promptsDir = Join-Path $automationDir "prompts"

$lockFile = Join-Path $queueDir "executor.lock"
if (Test-Path $lockFile) {
    Write-Host "Executor is currently locked."
    exit
}
New-Item -ItemType File -Path $lockFile | Out-Null

try {
    $planPath = Join-Path $queueDir "plan.txt"
    if (-not (Test-Path $planPath)) {
        Write-Host "plan.txt not found."
        exit
    }
    
    $planText = Get-Content $planPath -Raw -Encoding UTF8
    $systemPrompt = Get-Content (Join-Path $promptsDir "executor-system.txt") -Raw -Encoding UTF8
    
    $fullPrompt = "$systemPrompt`n`n--- PLAN ---`n$planText"

    Write-Host "Running Executor (Gemini)..."
    
    try {
        & gemini "$fullPrompt"
        $exitCode = $LASTEXITCODE
    } catch {
        $exitCode = 1
        Write-Host "Error running gemini CLI: $_"
    }

    if ($exitCode -ne 0 -and $exitCode -ne $null) {
        $statePath = Join-Path $queueDir "state.json"
        if (Test-Path $statePath) {
            $stateText = Get-Content $statePath -Raw -Encoding UTF8
            $stateObj = $stateText | ConvertFrom-Json
            $stateObj.status = "failed"
            $stateObj.error = "Executor CLI execution failed."
            $stateObj | ConvertTo-Json | Set-Content $statePath -Encoding UTF8
        }
        Write-Host "Executor failed."
    } else {
        Write-Host "Executor process completed."
    }
} finally {
    Remove-Item $lockFile -Force
}
