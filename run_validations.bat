@echo off
cd /d "D:\Projetos\Jogos\Cindars_hope\cindars_hope"

echo.
echo === Docs Validation ===
powershell -NoProfile -ExecutionPolicy Bypass -Command ".\tools\docs\validate_docs.ps1"
set docs_status=%ERRORLEVEL%

echo.
echo === Unity Compile Validation ===
powershell -NoProfile -ExecutionPolicy Bypass -Command ".\tools\unity\RunUnityCompileValidation.ps1 -UnityEditorPath 'C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe' -ProjectPath '.' -LogFile '.\Logs\unity-compile-validation.log'"
set unity_status=%ERRORLEVEL%

echo.
echo === Scan Unity Logs ===
powershell -NoProfile -ExecutionPolicy Bypass -Command ".\tools\unity\ScanUnityLogs.ps1 -LogFile '.\Logs\unity-compile-validation.log'"
set scan_status=%ERRORLEVEL%

echo.
echo Summary: docs=%docs_status% unity=%unity_status% scan=%scan_status%
