@echo off
cd XP-Hotkey\bin\Debug\net9.0-windows
echo Starting XP-Hotkey...
XP-Hotkey.exe 2>&1
echo.
echo Exit code: %ERRORLEVEL%
pause
