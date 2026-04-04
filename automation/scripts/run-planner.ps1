$env:DISABLE_MCP = "1"
$env:GOOGLE_GEMINI_BASE_URL = "http://127.0.0.1:8045"
[Console]::InputEncoding = [System.Text.UTF8Encoding]::new()
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()
$env:PYTHONIOENCODING = "utf-8"
$env:LANG = "en_US.UTF-8"
$projectRoot = Resolve-Path "$PSScriptRoot\..\.."
Set-Location $projectRoot
$task = Get-Content "automation\queue\instruction.txt" -Raw
@"
You are the planner AI for the BeachGo project.
Rules:
- Return only a numbered implementation plan.
- Do not write code.
- Do not call tools.
- Do not execute commands.
- Do not mention run_shell_command.
- Split the work into small phases and steps.
Task:
$task
"@ | Set-Content "automation\queue\planner-input.txt" -Encoding utf8
Get-Content "automation\queue\planner-input.txt" -Raw |
    gemini --model gemini-3-pro --approval-mode yolo |
    Out-File "automation\queue\plan.txt" -Encoding utf8

Write-Host "Planner run successfully!"
