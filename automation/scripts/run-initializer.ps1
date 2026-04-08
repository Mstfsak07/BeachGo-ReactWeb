#Requires -Version 5.1
$ErrorActionPreference = "Continue"

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding  = [System.Text.Encoding]::UTF8
$OutputEncoding            = [System.Text.Encoding]::UTF8

$scriptsDir    = $PSScriptRoot
$automationDir = Split-Path $scriptsDir -Parent
$queueDir      = Join-Path $automationDir "queue"
$promptsDir    = Join-Path $automationDir "prompts"
$logFile       = Join-Path $queueDir "automation.log"
$phasesFile    = Join-Path $queueDir "phases.txt"
$repoRoot      = Split-Path $automationDir -Parent

$INITIALIZER_MODEL = if ($env:BEACHGO_INITIALIZER_MODEL) { $env:BEACHGO_INITIALIZER_MODEL } else { "claude-sonnet-4-6" }

function Write-Log($msg) {
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [INITIALIZER] $msg"
    Write-Host $line
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

function Invoke-ClaudeAPI {
    param([string]$Prompt, [string]$SystemPrompt = "")

    $apiKey  = $env:ANTHROPIC_API_KEY
    $baseUrl = if ($env:ANTHROPIC_BASE_URL) { $env:ANTHROPIC_BASE_URL.TrimEnd('/') } else { "https://api.anthropic.com" }

    $bodyObj = [ordered]@{
        model      = $INITIALIZER_MODEL
        max_tokens = 8192
        messages   = @(@{ role = "user"; content = [string]$Prompt })
    }
    if (-not [string]::IsNullOrWhiteSpace($SystemPrompt)) {
        $bodyObj["system"] = [string]$SystemPrompt
    }

    $bodyJson  = $bodyObj | ConvertTo-Json -Depth 10 -Compress
    $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($bodyJson)

    $headers = @{
        "x-api-key"         = $apiKey
        "anthropic-version" = "2023-06-01"
        "content-type"      = "application/json"
    }

    $response = Invoke-RestMethod `
        -Uri     "$baseUrl/v1/messages" `
        -Method  POST `
        -Headers $headers `
        -Body    $bodyBytes

    if ($response.content -and $response.content[0].text) {
        return $response.content[0].text
    } elseif ($response.text) {
        return $response.text
    } else {
        Write-Log "HATA: API beklenmedik format dondu. Yanit: $($response | ConvertTo-Json -Compress)"
        return $null
    }
}

Write-Log "Initializer baslatiliyor - proje analizi yapilacak..."

$env:ANTHROPIC_API_KEY  = $env:BEACHGO_ANTHROPIC_KEY
$env:ANTHROPIC_BASE_URL = "http://127.0.0.1:8045"

# Proje dosya yap�s�n� topla
$frontendPages = try {
    (Get-ChildItem -Path (Join-Path $repoRoot "beach-ui\src\pages") -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }) -join ", "
} catch { "(alinamadi)" }

$frontendServices = try {
    (Get-ChildItem -Path (Join-Path $repoRoot "beach-ui\src\services") -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }) -join ", "
} catch { "(alinamadi)" }

$frontendComponents = try {
    (Get-ChildItem -Path (Join-Path $repoRoot "beach-ui\src\components") -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }) -join ", "
} catch { "(alinamadi)" }

$backendControllers = try {
    (Get-ChildItem -Path (Join-Path $repoRoot "BeachRehberi.API\BeachRehberi.API\Controllers") -Filter "*Controller.cs" -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }) -join ", "
} catch { "(alinamadi)" }

$backendServices = try {
    (Get-ChildItem -Path (Join-Path $repoRoot "BeachRehberi.API\BeachRehberi.API\Services") -Filter "*.cs" -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }) -join ", "
} catch { "(alinamadi)" }

$backendModels = try {
    (Get-ChildItem -Path (Join-Path $repoRoot "BeachRehberi.API\BeachRehberi.API\Models") -Filter "*.cs" -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }) -join ", "
} catch { "(alinamadi)" }

$lastMigration = try {
    (Get-ChildItem -Path (Join-Path $repoRoot "BeachRehberi.API\BeachRehberi.API\Migrations") -Filter "*.cs" -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -notmatch "Designer|Snapshot" } |
        Sort-Object Name -Descending |
        Select-Object -First 1).Name
} catch { "(alinamadi)" }

$appJs = try {
    Get-Content (Join-Path $repoRoot "beach-ui\src\App.js") -Raw -Encoding UTF8 -ErrorAction SilentlyContinue
} catch { "(okunamadi)" }

$programCs = try {
    Get-Content (Join-Path $repoRoot "BeachRehberi.API\BeachRehberi.API\Program.cs") -Raw -Encoding UTF8 -ErrorAction SilentlyContinue |
        Select-String -Pattern "builder\.|app\." -AllMatches |
        ForEach-Object { $_.Line.Trim() } |
        Select-Object -First 50 |
        Out-String
} catch { "(okunamadi)" }

$taskPath = Join-Path $queueDir "task.txt"
$userGoal = if (Test-Path $taskPath) { Get-Content $taskPath -Raw -Encoding UTF8 } else { "BeachGo projesini mobil oncesi hazir hale getir." }

$systemPrompt = @"
Sen BeachGo projesinin k�demli mimar�s�n. Projeyi derinlemesine analiz edecek ve �ncelikli bir geliştirme plan� ç�karacaks�n.

Kurallar:
1. Sadece projeye ait bilgilere dayanarak karar ver.
2. Her faz bağ�ms�z, test edilebilir ve net olsun.
3. Fazlar küçük ve somut olsun — Gemini CLI'nin uygulayabileceği büyüklükte.
4. Her fazda: hangi dosyalar değişecek, ne yap�lacak, nas�l test edilecek yaz.
5. Toplam faz say�s� 15-25 aras�nda olsun.
6. Mobil fazlar� en sona koy, �nce web/api eksiliklerini gider.
7. Ç�kt�n� SADECE aşağ�daki formatta yaz, başka hiçbir şey yazma:

FAZ_LISTESI_BASLANGIC
FAZ 1: [Başl�k]
SCOPE: web|api|mobile
DOSYALAR: [etkilenecek dosyalar]
GÖREV: [net, uygulanabilir g�rev aç�klamas�]
TEST: [nas�l doğrulanacak]
---
FAZ 2: [Başl�k]
...
FAZ_LISTESI_BITIS
"@

$analysisPrompt = @"
Kullan�c�n�n isteği: $userGoal

PROJE YAPISI:

## Frontend (React)
Sayfalar: $frontendPages
Servisler: $frontendServices
Componentler: $frontendComponents

## Backend (.NET)
Controllers: $backendControllers
Services: $backendServices
Models: $backendModels
Son Migration: $lastMigration

## App.js Route Yap�s�:
$appJs

## Program.cs Middleware/DI:
$programCs

Bu bilgilere göre öncelikli faz listesini çıkar.
"@

Write-Log "Claude API cagrisi yapiliyor - proje analizi..."

$phases = $null
$retries = 5
for ($i = 1; $i -le $retries; $i++) {
    try {
        Write-Log "Claude API cagrisi yapiliyor (deneme $i / $retries)..."
        $phases = Invoke-ClaudeAPI -Prompt $analysisPrompt -SystemPrompt $systemPrompt
        Write-Log "Claude API yaniti (deneme $i): $phases"
        Write-Host "=== CLAUDE YANITI (deneme $i) ===`n$phases`n================================" -ForegroundColor Cyan
        if (-not [string]::IsNullOrWhiteSpace($phases)) { break }
        Write-Log "Deneme $i bos cikti dondu."
    } catch {
        Write-Log "Deneme $i basarisiz: $_"
        Write-Host "=== CLAUDE HATA (deneme $i) ===`n$_`n================================" -ForegroundColor Red
        if ($i -lt $retries) {
            Write-Log "30 saniye bekleniyor..."
            Start-Sleep -Seconds 30
        }
    }
}

if ([string]::IsNullOrWhiteSpace($phases)) {
    Write-Log "KRITIK HATA: Claude API $retries deneme sonrasinda yanit vermedi. Otomasyon durduruluyor."
    exit 1
}

# phases.txt'e yaz
$phases | Set-Content $phasesFile -Encoding UTF8
Write-Log "phases.txt olusturuldu. Faz listesi hazir."

# Kac faz oldugunu say
$fazCount = ($phases | Select-String -Pattern "^FAZ \d+:" -AllMatches).Matches.Count
Write-Log "Toplam $fazCount faz tespit edildi."

# State'e yaz
$statePath = Join-Path $queueDir "state.json"
$state = [PSCustomObject]@{
    current_iteration  = 0
    max_iterations     = 50
    status             = "initialized"
    is_complete        = $false
    consecutive_errors = 0
    last_error         = $null
    last_error_hash    = $null
    current_phase      = 1
    total_phases       = $fazCount
    lastUpdated        = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
}
$state | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8

Write-Log "Initializer tamamlandi. Loop baslatilabilir."
