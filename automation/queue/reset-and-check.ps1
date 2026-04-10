#Requires -Version 5.1
$ErrorActionPreference = "Continue"

$queueDir = $PSScriptRoot
$statePath = Join-Path $queueDir "state.json"
$lockFile = Join-Path $queueDir "executor.lock"
$phasesFile = Join-Path $queueDir "phases.txt"

Write-Host "=== AUTOMATION LOOP - RESET & CHECK ===" -ForegroundColor Green

# 1. Temizlik
Write-Host "1. Temizlik yapiliyor..."
if (Test-Path $lockFile) {
    Remove-Item $lockFile -Force
    Write-Host "   ✓ executor.lock silindi"
}

# 2. State'i sıfırla
Write-Host "2. State sifirlanıyor..."
$newState = [PSCustomObject]@{
    current_iteration  = 0
    max_iterations     = 50
    status             = "idle"
    is_complete        = $false
    consecutive_errors = 0
    last_error         = $null
    last_error_hash    = $null
    current_phase      = 1
    total_phases       = 22
    lastUpdated        = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
}
$newState | ConvertTo-Json -Depth 10 | Set-Content $statePath -Encoding UTF8
Write-Host "   ✓ State = idle olarak sifirlandi"

# 3. Phases dosyası kontrol
Write-Host "3. Phases dosyasi kontrol ediliyor..."
if (-not (Test-Path $phasesFile)) {
    Write-Host "   ✗ phases.txt BULUNAMADI - initializer calistirmak gerekebilir"
} else {
    Write-Host "   ✓ phases.txt var ($(Get-Item $phasesFile | Measure-Object -Line).Lines satir)"
}

# 4. Process cleanup
Write-Host "4. Orphan process'ler temizleniyor..."
Get-Process -Name "node" -ErrorAction SilentlyContinue | ForEach-Object {
    try {
        $_.Kill()
        Write-Host "   ✓ Orphan node.exe durduruldu (PID=$($_.Id))"
    } catch {}
}

Write-Host ""
Write-Host "RESETLEME TAMAMLANDI - Loop'u baslatabilirsiniz" -ForegroundColor Green
Write-Host ""
Write-Host "Sonraki adim:"
Write-Host "  PowerShell'de sunu calistirin:"
Write-Host "  & '$($queueDir -replace '\\queue$', '\scripts\run-loop.ps1')'"
