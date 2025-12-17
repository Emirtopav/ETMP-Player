@echo off
echo Starting ETMP...
dotnet run --project ETMPClient
if %errorlevel% neq 0 (
    echo.
    echo Application failed to start.
    pause
)
