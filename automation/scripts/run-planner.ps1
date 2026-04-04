$ErrorActionPreference = "Stop"

$scriptsDir = $PSScriptRoot
$automationDir = Split-Path $scriptsDir -Parent
$queueDir = Join-Path $automationDir "queue"
$promptsDir = Join-Path $automationDir "prompts"

$lockFile = Join-Path $queueDir "planner.lock"
if (Test-Path $lockFile) {
    Write-Host "Planner is currently locked."
    exit
}
New-Item -ItemType File -Path $lockFile | Out-Null

try {
    $taskText = Get-Content (Join-Path $queueDir "task.txt") -Raw -Encoding UTF8
    
    $stateText = "{}"
    if (Test-Path (Join-Path $queueDir "state.json")) {
        $stateText = Get-Content (Join-Path $queueDir "state.json") -Raw -Encoding UTF8
    }

    $resultText = ""
    $resultPath = Join-Path $queueDir "result.txt"
    if (Test-Path $resultPath) {
        $resultText = Get-Content $resultPath -Raw -Encoding UTF8
    }

    $systemPrompt = Get-Content (Join-Path $promptsDir "planner-system.txt") -Raw -Encoding UTF8
    $fullPrompt = "$systemPrompt`n`n--- MEVCUT DURUM ---`n$stateText`n`n--- ÖNCEKİ SONUÇ ---`n$resultText`n`n--- GÖREV ---`n$taskText"

    Write-Host "Running Planner (Claude)..."
    
    # Executing Claude CLI. We use try-catch to avoid script crash if claude is missing.
    try {
        & claude -p $fullPrompt | Set-Content -Path (Join-Path $queueDir "plan.txt") -Encoding UTF8
        $exitCode = $LASTEXITCODE
    } catch {
        $exitCode = 1
        Write-Host "Error running claude CLI: $_"
    }

    $stateObj = $stateText | ConvertFrom-Json
    if ($exitCode -eq 0 -or $exitCode -eq $null) {
        $stateObj.status = "planned"
        $stateObj | ConvertTo-Json | Set-Content (Join-Path $queueDir "state.json") -Encoding UTF8
        Write-Host "Plan created successfully."
    } else {
        $stateObj.status = "failed"
        $stateObj.error = "Planner execution failed."
        $stateObj | ConvertTo-Json | Set-Content (Join-Path $queueDir "state.json") -Encoding UTF8
        Write-Host "Planner failed."
    }
} finally {
    Remove-Item $lockFile -Force
}
