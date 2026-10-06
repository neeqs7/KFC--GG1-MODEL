@echo off
setlocal
cd /d "%~dp0"
title KFC G/G/1 Queue Simulation

echo ================================================================================
echo                         KFC G/G/1 QUEUE SIMULATION
echo ================================================================================
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
  echo ERROR: .NET 8 SDK was not found.
  echo Please install the .NET 8 SDK, then run this file again.
  echo.
  pause
  exit /b 1
)

if not exist "%~dp0KFC_GG1.csproj" (
  echo ERROR: KFC_GG1.csproj was not found.
  echo Make sure you extracted the COMPLETE ZIP folder before running.
  echo.
  pause
  exit /b 1
)

if not exist "%~dp0kfc_data.csv" (
  echo ERROR: kfc_data.csv was not found.
  echo.
  pause
  exit /b 1
)

echo Project folder: %~dp0
echo.
echo Restoring project...
dotnet restore "%~dp0KFC_GG1.csproj"
if errorlevel 1 goto error

echo.
echo Building project...
dotnet build "%~dp0KFC_GG1.csproj" -c Release
if errorlevel 1 goto error

echo.
echo Running G/G/1 analysis...
echo.
dotnet run --project "%~dp0KFC_GG1.csproj" -c Release --no-build
if errorlevel 1 goto error

echo.
echo ================================================================================
echo                         RUN COMPLETED SUCCESSFULLY
echo ================================================================================
pause
exit /b 0

:error
echo.
echo ================================================================================
echo                              RUN FAILED
echo ================================================================================
pause
exit /b 1
