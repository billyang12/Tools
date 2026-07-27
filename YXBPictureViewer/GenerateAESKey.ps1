# Generate AES-256 Key for YXBPictureViewer
# Run this script to generate a new encryption key

Write-Host "=== AES-256 Key Generator ===" -ForegroundColor Cyan
Write-Host ""

try {
    # Create AES provider
    $aes = [System.Security.Cryptography.Aes]::Create()
    $aes.KeySize = 256

    # Generate random key
    $aes.GenerateKey()

    # Convert to Base64
    $base64Key = [Convert]::ToBase64String($aes.Key)

    Write-Host "Your new AES-256 key has been generated!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Key:" -ForegroundColor Yellow
    Write-Host $base64Key -ForegroundColor White
    Write-Host ""
    Write-Host "IMPORTANT: Copy this key and save it securely!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Copy the key above (select and Ctrl+C)" -ForegroundColor White
    Write-Host "2. Open YXBPictureViewer\Form1.cs" -ForegroundColor White
    Write-Host "3. Find the line: public static string aesKey = ..." -ForegroundColor White
    Write-Host "4. Replace with: public static string aesKey = `"$base64Key`";" -ForegroundColor White
    Write-Host ""

    # Copy to clipboard if possible
    try {
        Set-Clipboard -Value $base64Key
        Write-Host "✓ Key has been copied to clipboard!" -ForegroundColor Green
    }
    catch {
        Write-Host "Note: Could not copy to clipboard automatically. Please copy manually." -ForegroundColor Yellow
    }

    # Cleanup
    $aes.Dispose()

    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}
catch {
    Write-Host "Error generating key: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}
