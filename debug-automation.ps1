$ErrorActionPreference = "Stop"

Write-Host "=== AUTOMATION DEBUG ===" -ForegroundColor Cyan
Write-Host ""

$repoRoot = "c:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb"
$agentScript = "$repoRoot\automation\scripts\run-loop.ps1"
$queueDir = "$repoRoot\automation\queue"

# 1. Dizin ve dosya kontrolü
Write-Host "1. Dosya Kontrolu:" -ForegroundColor Yellow
Write-Host "   Repo root: $(if (Test-Path $repoRoot) { '✓' } else { '✗' }) $repoRoot"
Write-Host "   Agent script: $(if (Test-Path $agentScript) { '✓' } else { '✗' }) $agentScript"
Write-Host "   Queue dir: $(if (Test-Path $queueDir) { '✓' } else { '✗' }) $queueDir"
Write-Host ""

# 2. Node.js kontrolü
Write-Host "2. Node.js Version:" -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "   ✓ $nodeVersion"
} catch {
    Write-Host "   ✗ Node.js yüklü degil!"
    exit 1
}
Write-Host ""

# 3. NPM modülleri kontrolü
Write-Host "3. NPM Modules:" -ForegroundColor Yellow
$modules = "exists" -eq (if (Test-Path "$repoRoot\node_modules") { "exists" } else { $null })
Write-Host "   node_modules: $(if ($modules) { '✓' } else { '✗' })"
Write-Host ""

# 4. orchestrator.js kontrolü
Write-Host "4. Orchestrator.js:" -ForegroundColor Yellow
$orchFile = "$repoRoot\orchestrator.js"
if (Test-Path $orchFile) {
    Write-Host "   ✓ Dosya var"
    $size = (Get-Item $orchFile).Length
    Write-Host "   Boyut: $size bytes"
} else {
    Write-Host "   ✗ Dosya yok!"
    exit 1
}
Write-Host ""

# 5. run-loop.ps1 içeriği kontrol
Write-Host "5. run-loop.ps1 First Lines:" -ForegroundColor Yellow
$loopFile = "$repoRoot\automation\scripts\run-loop.ps1"
if (Test-Path $loopFile) {
    $head = Get-Content $loopFile -Head 5
    $head | ForEach-Object { Write-Host "   $_" }
} else {
    Write-Host "   ✗ Dosya yok!"
}
Write-Host ""

# 6. Backend projesini bul
Write-Host "6. Backend Projesi:" -ForegroundColor Yellow
$csproj = Get-ChildItem -Path $repoRoot -Recurse -Filter "*.csproj" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" } |
    Select-Object -First 1

if ($csproj) {
    Write-Host "   ✓ Bulundu: $($csproj.FullName)"
} else {
    Write-Host "   ✗ Proje bulunamadi"
}
Write-Host ""

# 7. Frontend kontrolü
Write-Host "7. Frontend Dizini:" -ForegroundColor Yellow
$frontendDir = "$repoRoot\beach-ui"
if (Test-Path $frontendDir) {
    Write-Host "   ✓ $frontendDir"
    if (Test-Path "$frontendDir\package.json") {
        Write-Host "   ✓ package.json var"
    }
} else {
    Write-Host "   ✗ Frontend yok"
}
Write-Host ""

# 8. Processes kontrol et
Write-Host "8. Çalisan Processes:" -ForegroundColor Yellow
$node = Get-Process node -ErrorAction SilentlyContinue
$ps = Get-Process powershell -ErrorAction SilentlyContinue | Where-Object {
    $_.CommandLine -match 'run-loop|orchestrator'
}
$dotnet = Get-Process dotnet -ErrorAction SilentlyContinue

Write-Host "   Node.js: $(if ($node) { "✓ $($node.Count) process(es)" } else { "✗ Degil" })"
Write-Host "   PowerShell (automation): $(if ($ps) { "✓ $($ps.Count) process(es)" } else { "✗ Degil" })"
Write-Host "   Dotnet: $(if ($dotnet) { "✓ $($dotnet.Count) process(es)" } else { "✗ Degil" })"
Write-Host ""

# 9. Portlar kontrol et
Write-Host "9. Port Availability:" -ForegroundColor Yellow
$ports = @(
    @{ Port = 3000; Name = "Frontend" },
    @{ Port = 5144; Name = "Backend" },
    @{ Port = 8045; Name = "Anthropic Proxy" }
)

foreach ($portInfo in $ports) {
    $socket = New-Object System.Net.Sockets.TcpClient
    try {
        $socket.Connect("127.0.0.1", $portInfo.Port)
        $status = "✓ Open"
        $socket.Close()
    } catch {
        $status = "✗ Closed"
    }
    Write-Host "   Port $($portInfo.Port) ($($portInfo.Name)): $status"
}
Write-Host ""

# 10. Queue dosyaları kontrol et
Write-Host "10. Queue Dosyalari:" -ForegroundColor Yellow
$queueFiles = @(
    "state.json",
    "instruction.txt",
    "result.txt",
    "phases.txt"
)

foreach ($file in $queueFiles) {
    $fullPath = Join-Path $queueDir $file
    if (Test-Path $fullPath) {
        $size = (Get-Item $fullPath).Length
        Write-Host "   ✓ $file ($size bytes)"
    } else {
        Write-Host "   ✗ $file (yok)"
    }
}
Write-Host ""

Write-Host "=== DEBUG TAMAMLANDI ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Sonraki adim: PowerShell'de calistir:" -ForegroundColor Green
Write-Host "  cd $repoRoot"
Write-Host "  node orchestrator.js"
