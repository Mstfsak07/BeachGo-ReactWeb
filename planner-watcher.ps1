$queue = "automation\queue"
$instructionFile = "$queue\instruction.txt"
$resultFile = "$queue\result.txt"
$stateFile = "$queue\state.json"

while ($true) {
    $instruction = Get-Content $instructionFile -Raw

    if (![string]::IsNullOrWhiteSpace($instruction)) {
        Write-Host "[PLANNER] Yeni instruction bulundu"

        $prompt = @"
You are the planner AI for the BeachGo project.

Project rules:
- Break the task into small actionable coding steps.
- Return concise numbered steps.
- Mention affected files.
- If large, split into phases.
- Always tell the coding AI to create a git commit after completion.
- Always tell the coding AI to use UTF-8 encoding if Turkish characters may appear.

User instruction:
$instruction
"@

        $prompt | Set-Clipboard

        Write-Host "[PLANNER] Gemini cevabini tamamen kopyalayip automation\queue\result.txt dosyasina yapistir, kaydet, sonra Enter'a bas."
        Read-Host "Planner cevabini result.txt icine yapistirdiktan sonra Enter"

        $plan = Get-Content $resultFile -Raw

        if (![string]::IsNullOrWhiteSpace($plan)) {
            Set-Content "$queue\plan.txt" $plan
            Clear-Content $instructionFile
            Clear-Content $resultFile
            Write-Host "[PLANNER] Plan kaydedildi -> plan.txt"
        }
    }

    Start-Sleep 2
}