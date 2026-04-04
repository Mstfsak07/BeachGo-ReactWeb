
$env:OLLAMA_API_BASE = "http://127.0.0.1:11434"
$env:OLLAMA_CONTEXT_LENGTH = "16384"
$env:OLLAMA_NUM_PARALLEL="1"
$env:OLLAMA_MAX_LOADED_MODELS="1"

$projectRoot = Resolve-Path "$PSScriptRoot\..\.."
Set-Location $projectRoot

$promptFile = "automation\queue\coder-prompt.txt"

if (!(Test-Path $promptFile)) {
    Write-Host "Missing $promptFile"
    exit 1
}

aider --model ollama_chat/glm-4.7-flash:latest --yes-always --auto-commits --message-file $promptFile