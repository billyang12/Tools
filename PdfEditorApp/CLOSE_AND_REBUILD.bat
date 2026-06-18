@echo off
echo ========================================
echo   CLOSE APP AND REBUILD
echo ========================================
echo.
echo STEP 1: Checking for running instances...
echo.

tasklist | findstr /I "PdfEditorApp.exe"
if %ERRORLEVEL% EQU 0 (
    echo.
    echo [ERROR] PDF Editor is still running!
    echo.
    echo PLEASE:
    echo 1. Close ALL PDF Editor windows manually
    echo 2. Check taskbar for any PDF Editor instances
    echo 3. Then run this script again
    echo.
    pause
    exit /b 1
)

echo No running instances found - proceeding with build...
echo.

cd /d "%~dp0"

echo STEP 2: Cleaning project...
dotnet clean

echo.
echo STEP 3: Building project...
dotnet build

echo.
echo STEP 4: Checking build result...
if exist "bin\Debug\net10.0-windows\PdfEditorApp.exe" (
    echo.
    echo [SUCCESS] Build completed successfully!
    echo.
    echo To run the app:
    echo   1. Double-click: bin\Debug\net10.0-windows\PdfEditorApp.exe
    echo   2. OR run: dotnet run
    echo.
    echo When you click "Edit Content", you'll see the NEW reliable editor!
    echo.
) else (
    echo.
    echo [ERROR] Build failed - EXE not found
    echo.
)

pause
