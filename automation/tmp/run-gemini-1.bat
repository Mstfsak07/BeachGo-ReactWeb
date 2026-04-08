@echo off
set GOOGLE_GEMINI_BASE_URL=http://127.0.0.1:8045
set GEMINI_API_KEY=sk-0392f1a407974e89912d8e22daca8d84
set BEACHGO_ANTHROPIC_KEY=sk-1ea2056fc84442c59efcd5fd6fe30f5b
set CI=true
set NO_COLOR=1
set TERM=dumb
cd /d "C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb"
gemini --approval-mode yolo --model gemini-3-pro -p "@C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\automation\tmp\executor-prompt-1.txt" > "C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\automation\tmp\gemini-out-1.txt" 2>&1
exit /b %ERRORLEVEL%
