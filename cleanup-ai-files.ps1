#Requires -Version 5.1
$ErrorActionPreference = "Stop"

Set-Location c:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb

Write-Host "=== GIT AI DOSYALARI TEMIZLEME ===" -ForegroundColor Green
Write-Host ""

# 1. Git'te takip edilen AI dosyalarını bul
Write-Host "1. Git'te takip edilen AI dosyaları aranıyor..."
$aiExtensions = @("ai", "psd", "sketch", "model", "pkl", "h5", "pt", "pth", "onnx", "pb", "tflite")
$trackedFiles = @()

foreach ($ext in $aiExtensions) {
    $files = git ls-files | Where-Object { $_ -match "\.$ext$" }
    if ($files) {
        $trackedFiles += $files
    }
}

if ($trackedFiles.Count -eq 0) {
    Write-Host "   ✓ Takip edilen AI dosyası bulunamadi"
    Write-Host ""
    Write-Host "Git'te takip edilen TÜMO dosyalar:" -ForegroundColor Cyan
    $allFiles = git ls-files
    Write-Host "   Toplam: $($allFiles.Count) dosya"
    Write-Host ""
    Write-Host "Hangisini kaldirmak istiyorsunuz? Lütfen dosya adlarini yazin (tab ile ayirarak)"
    exit 0
}

Write-Host "   Bulunan dosyalar:"
$trackedFiles | ForEach-Object { Write-Host "     - $_" -ForegroundColor Yellow }
Write-Host ""

# 2. Dosyaları Git'ten kaldır
Write-Host "2. Git'ten kaldiriliyor..."
foreach ($file in $trackedFiles) {
    git rm --cached $file
    Write-Host "   ✓ $file"
}
Write-Host ""

# 3. .gitignore'a ekle
Write-Host "3. .gitignore'a model patterns ekleniyor..."

$gitignorePath = ".gitignore"
if (-not (Test-Path $gitignorePath)) {
    New-Item -Path $gitignorePath -ItemType File -Force | Out-Null
}

$ignorePatterns = @(
    "# AI & Machine Learning"
    "*.ai"
    "*.psd"
    "*.sketch"
    "*.model"
    "*.pkl"
    "*.h5"
    "*.pt"
    "*.pth"
    "*.onnx"
    "*.pb"
    "*.tflite"
    "*.joblib"
    "*.pickle"
)

$existingContent = Get-Content $gitignorePath -Raw -Encoding UTF8
foreach ($pattern in $ignorePatterns) {
    if ($existingContent -notmatch [regex]::Escape($pattern)) {
        Add-Content $gitignorePath -Value $pattern -Encoding UTF8
        Write-Host "   ✓ $pattern"
    }
}
Write-Host ""

# 4. Commit oluştur
Write-Host "4. Commit olusturuluyor..."
git add .gitignore
git commit -m "Remove AI model files from tracking, add .gitignore"
Write-Host "   ✓ Commit tamamlandi"
Write-Host ""

Write-Host "TAMAMLANDI!" -ForegroundColor Green
Write-Host ""
Write-Host "Ozetleme:"
Write-Host "  - $($trackedFiles.Count) AI dosyasi Git'ten kaldirildi"
Write-Host "  - .gitignore guncellendi"
Write-Host "  - Commit olusturuldu"
