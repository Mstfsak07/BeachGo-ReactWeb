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

Rules:
- Apply the requested changes directly in the repository.
- Follow the existing project architecture and coding style.
- Complete all requested phases in order.
- Do not stop after partial implementation.
- Preserve existing functionality.
- If a database change is needed, also create/update the migration.
- If backend changes are made, update the frontend integration if necessary.
- If frontend changes are made, ensure related admin panel or API usage still works.
- Add or update tests when relevant.
- After finishing, create a git commit.
- Use UTF-8 encoding for any file that may contain Turkish characters.
- Do not ask for confirmation unless a required file or dependency is missing.

Task:

$instruction

$plan

Final response requirements:
- Briefly list the files changed.
- Mention any remaining issue or follow-up work.
- Show the git commit message used.
"@

$template | Set-Content $outputFile -Encoding UTF8

Write-Host "Updated $outputFile"