@echo off
REM Проверка прав администратора
net session >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo.
    echo Программа требует прав администратора!
    echo Пожалуйста, запустите командную строку от имени администратора.
    echo.
    pause
    exit /b 1
)

chcp 65001 > nul
title WiFi Bruteforce Tool

cd /d "%~dp0"
dotnet restore
dotnet build -c Release
dotnet run -c Release --no-build
