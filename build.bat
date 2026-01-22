@echo off
REM Script batch per eseguire la build di AllKeyShop Extension
REM Esegue lo script PowerShell build.ps1

echo ========================================
echo   AllKeyShop Extension Build
echo ========================================
echo.

REM Verifica che PowerShell sia disponibile
where powershell >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERRORE: PowerShell non trovato!
    echo Assicurati che PowerShell sia installato e nel PATH.
    pause
    exit /b 1
)

REM Esegui lo script PowerShell
powershell.exe -ExecutionPolicy Bypass -File "%~dp0build.ps1"

REM Controlla il codice di uscita
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERRORE: Build fallita!
    pause
    exit /b 1
)

echo.
echo Build completata con successo!
echo.
pause
