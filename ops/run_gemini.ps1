param(
    [Parameter(Mandatory = $true)]
    [string]$Model,
    [Parameter(Mandatory = $true)]
    [string]$PromptFile
)

$ErrorActionPreference = "Stop"

$localEnvFile = Join-Path $PSScriptRoot "local\gemini.env.ps1"
if (Test-Path -LiteralPath $localEnvFile) {
    . $localEnvFile
}

$promptText = Get-Content -LiteralPath $PromptFile -Raw -Encoding UTF8
$gemini = (Get-Command gemini).Source
$promptText | & $gemini --approval-mode yolo --model $Model --prompt " "
exit $LASTEXITCODE
