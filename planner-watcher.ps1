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
Your job is to produce the NEXT instruction for the coding AI.
Return only one short next task that should be written into instruction.txt.
Do not mention plan.txt.
Do not return multiple phases.

User instruction:
$instruction
"@

        $prompt | Set-Clipboard

        Write-Host "[PLANNER] Gemini cevabini tamamen kopyalayip automation\queue\result.txt dosyasina yapistir, kaydet, sonra Enter'a bas."
        Read-Host "Planner cevabini result.txt icine yapistirdiktan sonra Enter"

        $plan = Get-Content $resultFile -Raw

        if (![string]::IsNullOrWhiteSpace($plan)) {
            Set-Content $instructionFile $plan -Encoding UTF8
            Clear-Content $resultFile
            Write-Host "[PLANNER] Yeni instruction yazildi -> instruction.txt"
        }
    }

    Start-Sleep 2
}