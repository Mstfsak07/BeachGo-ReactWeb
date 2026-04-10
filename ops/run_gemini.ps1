param(
    [Parameter(Mandatory = $true)]
    [string]$Model,
    [Parameter(Mandatory = $true)]
    [string]$PromptFile
)

$ErrorActionPreference = "Stop"
$promptText = Get-Content -LiteralPath $PromptFile -Raw -Encoding UTF8
$gemini = (Get-Command gemini).Source
& $gemini --approval-mode yolo --model $Model --prompt $promptText
exit $LASTEXITCODE
