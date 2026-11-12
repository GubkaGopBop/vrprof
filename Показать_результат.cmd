@echo off
chcp 65001 >nul
cd /d "%~dp0JavaAnalyzer"
echo COMPLETE ATTEMPT
java -Dfile.encoding=UTF-8 -jar build\vrprof-analyzer.jar examples\completed.json results\show-completed
if errorlevel 1 goto finish
echo.
type results\show-completed\summary.txt
echo.
echo INCOMPLETE ATTEMPT
java -Dfile.encoding=UTF-8 -jar build\vrprof-analyzer.jar examples\incomplete.json results\show-incomplete
:finish
echo.
pause
