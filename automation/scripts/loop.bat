@echo off
set MAX_LOOPS=50
set /a COUNT=0

:loop
set /a COUNT+=1
if %COUNT% gtr %MAX_LOOPS% (
    echo [LOOP] Max iteration limit reached (%MAX_LOOPS%). Exiting.
    exit /b 0
)

cls
echo [%COUNT%/%MAX_LOOPS%] Extracting coder prompt...
powershell -ExecutionPolicy Bypass -File automation\scripts\extract-coder-prompt.ps1

echo [%COUNT%/%MAX_LOOPS%] Running executor...
call automation\scripts\run-coder.bat

if exist automation\queue\state.json (
    findstr /i "\"is_complete\":  true" automation\queue\state.json >nul 2>&1
    if not errorlevel 1 (
        echo [LOOP] System complete. Exiting.
        exit /b 0
    )
)

echo Waiting 10 seconds...
timeout /t 10 >nul

goto loop