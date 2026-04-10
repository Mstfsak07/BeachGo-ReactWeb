#Requires -Version 5.1
Set-Location c:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb

Write-Host "=== GIT AI DOSYALARI TEMIZLEME ===" -ForegroundColor Green
Write-Host ""

# 1. .gitignore değişikliklerini hazırla
Write-Host "1. .gitignore eklenecek..."
git add .gitignore
Write-Host "   ✓ .gitignore staged"

# 2. AI model dosyalarını bulup kaldır (takibi bırak)
Write-Host ""
Write-Host "2. Git'te takip edilen AI dosyaları aranıyor..."

$aiExtensions = @("ai", "psd", "sketch", "model", "pkl", "pickle", "h5", "pt", "pth", "onnx", "pb", "tflite", "keras", "safetensors")
$foundFiles = @()

foreach ($ext in $aiExtensions) {
    $files = @(git ls-files | Where-Object { $_ -match "\.$ext$" })
    if ($files.Count -gt 0) {
        $foundFiles += $files
    }
}

if ($foundFiles.Count -gt 0) {
    Write-Host "   Bulundu: $($foundFiles.Count) dosya"
    foreach ($file in $foundFiles) {
        git rm --cached "$file"
        Write-Host "     ✓ $file (tracking stopped)"
    }
} else {
    Write-Host "   ✓ Takip edilen AI dosyası bulunamadı"
}

# 3. Commit
Write-Host ""
Write-Host "3. Commit oluşturuluyor..."
git commit -m "chore: Remove AI model files from tracking, add .gitignore patterns

- Added .gitignore patterns for: *.ai, *.psd, *.model, *.pkl, *.h5, *.pt, *.pth, *.onnx, *.pb, *.tflite, *.keras, *.safetensors
- Removed $($foundFiles.Count) model files from Git tracking"

Write-Host "   ✓ Commit tamamlandi"
Write-Host ""
Write-Host "BASARILI!" -ForegroundColor Green
Write-Host ""
git log --oneline -3
