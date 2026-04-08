@echo off
set GOOGLE_GEMINI_BASE_URL=http://127.0.0.1:8045
set GEMINI_API_KEY=sk-0392f1a407974e89912d8e22daca8d84
set BEACHGO_ANTHROPIC_KEY=
set CI=true
set NO_COLOR=1
set TERM=dumb
cd /d "C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb"
gemini --approval-mode yolo --model gemini-3-flash -p "@C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\automation\queue\executor-prompt.txt" > "C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\automation\queue\gemini-out-5.txt" 2>&1
exit /b %ERRORLEVEL%
