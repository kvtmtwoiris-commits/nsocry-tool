@echo off
setlocal
chcp 65001 >nul
title NSOCry Pro - Update and Build
cd /d "%~dp0"

echo.
echo ==========================================
echo       NSOCry Pro - Update and Build
echo ==========================================
echo.

where git >nul 2>nul
if errorlevel 1 (
    echo [LOI] Chua cai Git hoac Git chua co trong PATH.
    pause
    exit /b 1
)

set "DOTNET_CMD=dotnet"
where dotnet >nul 2>nul
if errorlevel 1 (
    if exist "%ProgramFiles%\dotnet\dotnet.exe" (
        set "DOTNET_CMD=%ProgramFiles%\dotnet\dotnet.exe"
    ) else (
        echo [LOI] Khong tim thay .NET 8 SDK.
        echo Tai tai: https://dotnet.microsoft.com/download/dotnet/8.0
        pause
        exit /b 1
    )
)

echo [1/4] Dang cap nhat ma nguon...
git pull --ff-only
if errorlevel 1 goto :failed

echo [2/4] Dang khoi phuc goi phu thuoc...
"%DOTNET_CMD%" restore NSOCryPro.sln
if errorlevel 1 goto :failed

echo [3/4] Dang build NSOCry Pro...
"%DOTNET_CMD%" publish src\NSOCryPro\NSOCryPro.csproj -c Release -r win-x64 --self-contained false -o dist
if errorlevel 1 goto :failed

echo [4/4] Dang chuan bi runtime...
if not exist "dist\runtime" mkdir "dist\runtime"
if exist "runtime\microemulator.jar" copy /Y "runtime\microemulator.jar" "dist\runtime\microemulator.jar" >nul
if exist "runtime\game.jar" copy /Y "runtime\game.jar" "dist\runtime\game.jar" >nul

echo.
echo [THANH CONG] File chay:
echo %CD%\dist\NSOCryPro.exe
echo.
if not exist "dist\runtime\microemulator.jar" echo [CHU Y] Can chep microemulator.jar vao thu muc runtime.
if not exist "dist\runtime\game.jar" echo [CHU Y] Can chep client va doi ten thanh game.jar trong thu muc runtime.
echo.
pause
exit /b 0

:failed
echo.
echo [LOI] Cap nhat hoac build that bai. Xem thong bao phia tren.
pause
exit /b 1
