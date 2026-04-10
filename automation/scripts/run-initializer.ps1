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

function Get-MojibakeScore {
    param([string]$Text)

    if ([string]::IsNullOrEmpty($Text)) { return 0 }
    return ([regex]::Matches($Text, 'Ã|Ä|Å|â|�|ğŸ|Â')).Count
}

function Repair-MojibakeText {
    param([string]$Text)

    if ([string]::IsNullOrEmpty($Text)) { return $Text }

    $current = $Text
    for ($round = 0; $round -lt 3; $round++) {
        $originalScore = Get-MojibakeScore $current
        if ($originalScore -eq 0) { break }

        $candidates = @()
        foreach ($encName in @("windows-1254", "windows-1252")) {
            try {
                $enc = [System.Text.Encoding]::GetEncoding($encName)
                $candidate = [System.Text.Encoding]::UTF8.GetString($enc.GetBytes($current))
                $candidates += [PSCustomObject]@{
                    Text  = $candidate
                    Score = Get-MojibakeScore $candidate
                }
            } catch {}
        }

        $best = $candidates | Sort-Object Score | Select-Object -First 1
        if ($best -and $best.Score -lt $originalScore) {
            $current = $best.Text
        } else {
            break
        }
    }

    return $current
}

function Invoke-ClaudeAPI {
    param([string]$Prompt, [string]$SystemPrompt = "")
    
    $apiKey  = $env:ANTHROPIC_API_KEY
    $baseUrl = if ($env:ANTHROPIC_BASE_URL) { $env:ANTHROPIC_BASE_URL.TrimEnd('/') } else { "https://api.anthropic.com" }
    
    $bodyObj = @{
        model      = $INITIALIZER_MODEL
        max_tokens = 8192
        messages   = @(@{ role = "user"; content = $Prompt })
    }
    if ($SystemPrompt) { $bodyObj["system"] = $SystemPrompt }
    
    $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes(
        ($bodyObj | ConvertTo-Json -Depth 10 -Compress)
    )

    try {
        $response = Invoke-WebRequest `
            -Uri "$baseUrl/v1/messages" `
            -Method Post `
            -Headers @{
                "x-api-key"         = $apiKey
                "anthropic-version" = "2023-06-01"
                "Content-Type"      = "application/json"
            } `
            -Body $bodyBytes `
            -UseBasicParsing

        $stream = $response.RawContentStream
        $stream.Position = 0
        $reader = [System.IO.StreamReader]::new($stream, [System.Text.Encoding]::UTF8, $true)
        $responseJson = $reader.ReadToEnd()
        $reader.Dispose()
        $parsed = $responseJson | ConvertFrom-Json

        if ($parsed.content -and $parsed.content.Count -gt 0) {
            return $parsed.content[0].text
        }
    } catch {
        Write-Log "HATA: API cagrisi basarisiz: $_"
    }
    return $null
}
Write-Log "DEBUG - BEACHGO_KEY: $env:BEACHGO_ANTHROPIC_KEY"
Write-Log "DEBUG - ANTHROPIC_KEY: $env:ANTHROPIC_API_KEY"
Write-Log "DEBUG - BASE_URL: $env:ANTHROPIC_BASE_URL"
# ENV degiskenlerini fonksiyon taninmadan ONCE set et
$env:ANTHROPIC_API_KEY  = $env:BEACHGO_ANTHROPIC_KEY
$env:ANTHROPIC_BASE_URL = "http://127.0.0.1:8045"

Write-Log "Initializer baslatiliyor - proje analizi yapilacak..."

# Proje dosya yapisini topla
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
Sen BeachGo projesinin kıdemli mimarısın. Projeyi derinlemesine analiz edecek ve öncelikli bir geliştirme planı çıkaracaksın.

Kurallar:
1. Sadece projeye ait bilgilere dayanarak karar ver.
2. Her faz bağımsız, test edilebilir ve net olsun.
3. Fazlar küçük ve somut olsun; Gemini CLI'nin uygulayabileceği büyüklükte olsun.
4. Her fazda hangi dosyalar değişecek, ne yapılacak, nasıl test edilecek yaz.
5. Toplam faz sayısı 15-25 arasında olsun.
6. Mobil fazları en sona koy, önce web/api eksikliklerini gider.
7. Önce web ve api tarafını mobile-ready hale getir: auth, form akışları, hata yönetimi, validation, API tutarlılığı, build, deploy hazırlığı, tema, responsive eksikleri, veri akışları ve kritik UX boşlukları kapatılsın.
8. Mobile fazlarına sadece web/api tarafındaki kritik eksikler kapandıktan sonra geç. Mobile fazlarında ekranlar, servis entegrasyonu, auth akışları, state yönetimi, offline/dayanıklılık, build ve release hazırlığı olsun.
9. İlk fazlar analiz ve inventory değil, doğrudan uygulanabilir teknik görevler olsun.
10. Aynı problemi tekrar eden veya çok geniş fazlar yazma. Her faz tek net çıktısı olan bir iş olsun.
11. Çıktını SADECE aşağıdaki formatta yaz, başka hiçbir şey yazma:

FAZ_LISTESI_BASLANGIC
FAZ 1: [Başlık]
SCOPE: web|api|mobile
DOSYALAR: [etkilenecek dosyalar]
GÖREV: [net, uygulanabilir görev açıklaması]
TEST: [nasıl doğrulanacak]
---
FAZ 2: [Başlık]
...
FAZ_LISTESI_BITIS
"@

$analysisPrompt = @"
Kullanıcının isteği: $userGoal

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

## App.js Route Yapısı:
$appJs

## Program.cs Middleware/DI:
$programCs

Bu bilgilere göre öncelikli faz listesini çıkar.
"@

Write-Log "Claude API cagrisi yapiliyor - proje analizi..."

$phases = $null
$retries = 3
for ($i = 1; $i -le $retries; $i++) {
    try {
        $phases = Repair-MojibakeText (Invoke-ClaudeAPI -Prompt $analysisPrompt -SystemPrompt $systemPrompt)
        if (-not [string]::IsNullOrWhiteSpace($phases)) { break }
    } catch {
        Write-Log "Deneme $i basarisiz: $_"
        Start-Sleep -Seconds 10
    }
}

if ([string]::IsNullOrWhiteSpace($phases)) {
    Write-Log "HATA: Claude analiz yapamiadi."
    exit 1
}

# phases.txt'e yaz
(Repair-MojibakeText $phases) | Set-Content $phasesFile -Encoding UTF8
Write-Log "phases.txt olusturuldu. Faz listesi hazir."

# Kac faz oldugunu say
$fazCount = ([regex]::Matches($phases, '(?m)^FAZ\s+\d+:')).Count
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

