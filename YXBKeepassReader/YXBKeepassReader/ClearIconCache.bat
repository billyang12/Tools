@echo off
echo Clearing Windows icon cache...
taskkill /IM explorer.exe /F
del /A /Q "%localappdata%\IconCache.db"
del /A /F /Q "%localappdata%\Microsoft\Windows\Explorer\iconcache*"
echo Icon cache cleared. Restarting Explorer...
start explorer.exe
echo Done! The icon should now update.
pause
