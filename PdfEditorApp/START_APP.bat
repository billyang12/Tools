@echo off
echo ========================================
echo    PDF Editor Pro - Startup Script
echo ========================================
echo.
echo Setting up environment...
echo.

:: Change to the exe directory
cd /d "%~dp0bin\Debug\net10.0-windows"

:: Add x64 directory to PATH
set PATH=%CD%\x64;%PATH%

echo Running from: %CD%
echo.
echo Starting application...
echo.

:: Run the application
PdfEditorApp.exe

echo.
echo Application closed.
pause
