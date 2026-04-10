Set-Location c:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb

Write-Host "=== GIT COMMIT: AI DOSYALARI ===" -ForegroundColor Green
Write-Host ""

# .gitignore'ı stage et
git add .gitignore
Write-Host "✓ .gitignore staged"

# Commit'i oluştur
git commit -m "chore: Add AI model files to .gitignore"
Write-Host "✓ Commit completed"

# Son 3 commit'i göster
Write-Host ""
Write-Host "Recent commits:" -ForegroundColor Cyan
git log --oneline -3

Write-Host ""
Write-Host "DONE!" -ForegroundColor Green
