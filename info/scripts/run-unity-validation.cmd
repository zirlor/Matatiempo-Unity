@echo off
REM === Validation script: unity test en batchmode ===
REM Editor debe estar instalado previamente via: unity install 6000.5.0f1 --no-elevate --accept-eula
REM La licencia Unity es REQUERIDA para batchmode; si no hay, usar -manualLicenseFile o saltar.

set UNITY="%LocalAppData%\Unity\editors\6000.5.0f1\Editor\Unity.exe"
set PROJECT="C:\Users\zirow\source\repos\Matatiempo-Unity"

echo === Compilacion y test en batchmode ===
%UNITY% -batchmode -nographics -projectPath %PROJECT% -executeMethod TestUtility.RunAll -quit -returnlicense 2>&1 | tee "%~dp0unity-test-output.log"

if %ERRORLEVEL% NEQ 0 (
    echo === ERROR: compilacion o test fallo (ver unity-test-output.log) ===
    echo Si el error menciona "license", activar una licencia Unity primero.
    exit /b 1
)
echo === VALIDATION OK ===
