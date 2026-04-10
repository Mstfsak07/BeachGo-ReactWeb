$ErrorActionPreference = "Stop"

Set-Location c:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb

Write-Host "=== AI DOSYALARI VE GEREKSIZ YORUMLAR TEMIZLEME ===" -ForegroundColor Green
Write-Host ""

# 1. .claude ve .abacusai klasörlerini kaldır
Write-Host "1. AI klasörleri aranıyor..."
$aiDirs = @(".claude", ".abacusai")
$removedDirs = @()

foreach ($dir in $aiDirs) {
    if (Test-Path $dir) {
        Write-Host "   ! Bulundu: $dir"

        # Git'ten kaldır
        git rm -r --cached $dir 2>$null

        # Dosya sisteminden sil
        Remove-Item -Path $dir -Recurse -Force
        $removedDirs += $dir
        Write-Host "     ✓ Silindi: $dir"
    }
}

if ($removedDirs.Count -eq 0) {
    Write-Host "   ✓ AI klasörü bulunamadı"
}
Write-Host ""

# 2. Gereksiz yorum/yedek dosyalarını bul ve sil
Write-Host "2. Gereksiz yorum dosyaları aranıyor..."
$patterns = @("*.bak", "*.old", "*.backup", "*.tmp", "*~", "*.swp", ".backup*", "*-backup*", "*.commented", "*TODO*", "*WIP*")
$deletedFiles = @()

foreach ($pattern in $patterns) {
    $files = @(Get-ChildItem -Path . -Recurse -Filter $pattern -ErrorAction SilentlyContinue | Where-Object { !$_.PSIsContainer })

    foreach ($file in $files) {
        # Git'ten kaldır (takip ediliyor ise)
        git rm --cached $file.FullName 2>$null

        # Sil
        Remove-Item -Path $file.FullName -Force
        $deletedFiles += $file.FullName.Replace($PWD, ".")
        Write-Host "     ✓ Silindi: $($file.FullName.Replace($PWD, '.'))"
    }
}

if ($deletedFiles.Count -eq 0) {
    Write-Host "   ✓ Gereksiz dosya bulunamadı"
}
Write-Host ""

# 3. .gitignore'a AI klasörlerini ekle
Write-Host "3. .gitignore güncelleniyor..."
$gitignorePath = ".gitignore"

$newPatterns = @(
    ".claude/",
    ".abacusai/",
    "*.commented",
    "*TODO*",
    "*WIP*"
)

$existingContent = Get-Content $gitignorePath -Raw -Encoding UTF8
$added = 0

foreach ($pattern in $newPatterns) {
    if ($existingContent -notmatch [regex]::Escape($pattern)) {
        Add-Content $gitignorePath -Value $pattern -Encoding UTF8
        Write-Host "   ✓ Pattern eklendi: $pattern"
        $added++
    }
}

Write-Host ""

# 4. Commit oluştur
if ($removedDirs.Count -gt 0 -or $deletedFiles.Count -gt 0 -or $added -gt 0) {
    Write-Host "4. Değişiklikler commit ediliyor..."

    git add .gitignore
    git commit -m "chore: Remove AI folders and cleanup backup/comment files

- Removed: $($removedDirs -join ', ')
- Deleted: $($deletedFiles.Count) backup/comment files
- Updated .gitignore with AI folder patterns" 2>$null

    Write-Host "   ✓ Commit başarılı"
    Write-Host ""
    git log --oneline -3
} else {
    Write-Host "4. Temizlenecek dosya yok"
}

Write-Host ""
Write-Host "TAMAMLANDI!" -ForegroundColor Green
