@echo off
echo Running Cross-Plugin Architecture Tests...
echo.

cd /d "g:\dev\game\plugins\framework\GameComposition\cs\Core"

echo Building GameComposition.Core...
dotnet build --verbosity quiet
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Running architectural tests...
dotnet test --filter "Category=Architectural" --verbosity normal --no-build

echo.
echo Test run completed!
pause
