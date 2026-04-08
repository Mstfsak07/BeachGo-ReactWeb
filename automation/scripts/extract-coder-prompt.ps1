$instructionFile = "automation\queue\instruction.txt"
$planFile = "automation\queue\plan.txt"
$outputFile = "automation\queue\coder-prompt.txt"

$instruction = ""
$plan = ""

if (Test-Path $instructionFile) {
    $instruction = Get-Content $instructionFile -Raw
}

if (Test-Path $planFile) {
    $plan = Get-Content $planFile -Raw
}

$template = @"
You are the implementation AI for the BeachGo project.

Task:
$instruction

$plan
"@

$template | Set-Content $outputFile -Encoding UTF8

Write-Host "Updated $outputFile"