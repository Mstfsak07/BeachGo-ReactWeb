@echo off
:loop
cls

echo [1/2] Extracting coder prompt...
powershell -ExecutionPolicy Bypass -File automation\scripts\extract-coder-prompt.ps1

echo [2/2] Running aider...
call automation\scripts\run-coder.bat

echo Waiting 10 seconds...
timeout /t 10 >nul

goto loop