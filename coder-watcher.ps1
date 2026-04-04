$promptFile = "automation\queue\coder-prompt.txt"
$logFile = "automation\queue\aider-log.txt"

Write-Host "[CODER WATCHER] $promptFile izleniyor..."

while ($true) {
    if (Test-Path $promptFile) {
        $content = Get-Content $promptFile -Raw

        if (![string]::IsNullOrWhiteSpace($content)) {
            Write-Host "[CODER WATCHER] Yeni görev bulundu, Aider başlatılıyor..."
            Write-Host "[CODER WATCHER] Loglar $logFile dosyasına yazılıyor..."
            
            $env:OLLAMA_API_BASE = "http://127.0.0.1:11434"
            $env:OLLAMA_CONTEXT_LENGTH = "8192"  # RAM'i rahatlatmak icin context'i 8K'ya indirdik
            $env:OLLAMA_NUM_PARALLEL="1"
            $env:OLLAMA_MAX_LOADED_MODELS="1"
            
            # Terminal hatalarını önlemek için ortam değişkenleri
            $env:TERM="dumb"
            
            # Aider'i hafif Qwen3 modelimizle çalıştırıyoruz
            cmd.exe /c "aider --model ollama_chat/qwen3:8b --yes-always --auto-commits --no-pretty --message-file ""$promptFile"" > ""$logFile"" 2>&1"
            
            Write-Host "[CODER WATCHER] Görev tamamlandı, dosya temizleniyor..."
            Clear-Content $promptFile
            Write-Host "[CODER WATCHER] Yeni görev bekleniyor..."
        }
    }

    Start-Sleep -Seconds 2
}