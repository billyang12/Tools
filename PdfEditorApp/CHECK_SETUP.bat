@echo off
echo ========================================
echo   PDF Editor Pro - Setup Checker
echo ========================================
echo.

cd /d "%~dp0"

echo Checking installation...
echo.

echo [1/5] Checking application EXE...
if exist "bin\Debug\net10.0-windows\PdfEditorApp.exe" (
    echo     [OK] Application built successfully
) else (
    echo     [ERROR] Application not built - run: dotnet build
    goto end
)

echo.
echo [2/5] Checking native PDF rendering DLL...
if exist "bin\Debug\net10.0-windows\x64\pdfium.dll" (
    echo     [OK] pdfium.dll found in x64 folder
    dir /B "bin\Debug\net10.0-windows\x64\pdfium.dll" | find /v "" >/dev/null
) else (
    echo     [ERROR] pdfium.dll not found
    echo     Run: dotnet build
    goto end
)

echo.
echo [3/5] Checking iText7 libraries...
if exist "bin\Debug\net10.0-windows\itext.kernel.dll" (
    echo     [OK] iText7 libraries present
) else (
    echo     [ERROR] iText7 missing - run: dotnet restore
)

echo.
echo [4/5] Checking PdfiumViewer...
if exist "bin\Debug\net10.0-windows\PdfiumViewer.dll" (
    echo     [OK] PdfiumViewer present
) else (
    echo     [ERROR] PdfiumViewer missing
)

echo.
echo [5/5] Checking file paths...
echo     App directory: %CD%
echo     Exe location: %CD%\bin\Debug\net10.0-windows

echo.
echo ========================================
echo   Setup Check Complete
echo ========================================
echo.
echo RECOMMENDED: Double-click START_APP.bat to run
echo.

:end
pause
