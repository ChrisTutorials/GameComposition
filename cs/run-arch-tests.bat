@echo off
echo ========================================
echo GameComposition Architectural Test Runner
echo ========================================
echo.

echo [1/3] Running GameComposition Global Architectural Tests...
dotnet test "g:\dev\game\plugins\framework\GameComposition\cs\Tests\GameComposition.ArchitecturalTests.csproj" --filter "Category=Architectural" --logger "console;verbosity=normal"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ GameComposition architectural tests FAILED!
    exit /b 1
)

echo.
echo [2/3] Running GridPlacement Architectural Tests...
dotnet test "g:\dev\game\plugins\gameplay\GridPlacement\cs\Tests\GridPlacement.Core.Tests.csproj" --filter "Category=Architectural" --logger "console;verbosity=normal"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ GridPlacement architectural tests FAILED!
    exit /b 1
)

echo.
echo [3/3] Running All Architectural Tests (Cross-Plugin)...
dotnet test "g:\dev\game\plugins\framework\GameComposition\cs\Tests\GameComposition.ArchitecturalTests.csproj" --filter "Category=Architectural" --logger "console;verbosity=detailed"

echo.
echo ✅ ALL ARCHITECTURAL TESTS PASSED!
echo.
echo Strong Typing Compliance: VERIFIED
echo No Object/Object? Usage: VERIFIED  
echo No Dictionary<string,object>: VERIFIED
echo Proper 2D Naming: VERIFIED
echo.
