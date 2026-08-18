@echo off
setlocal enabledelayedexpansion

set "ROOT=%~dp0"
set "CONFIG=Release"
set "MSBUILD_EXE=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"

if not exist "%ROOT%Dlls\FanControl.Plugins.dll" (
  echo ERROR: Dlls\FanControl.Plugins.dll is missing.
  echo Copy it from your FanControl installation or release package before building.
  exit /b 1
)

if exist "%MSBUILD_EXE%" (
  set "BUILD_CMD=%MSBUILD_EXE%"
) else (
  set "BUILD_CMD=msbuild"
)

echo === Restore ===
dotnet restore "%ROOT%FanControl.ClevoPlugin.sln"
if errorlevel 1 goto error

echo === Build ===
"%BUILD_CMD%" "%ROOT%FanControl.ClevoPlugin.sln" /p:Configuration=%CONFIG%
if errorlevel 1 goto error

echo === Prepare release folder ===
if exist "%ROOT%release" rmdir /s /q "%ROOT%release"
mkdir "%ROOT%release"
mkdir "%ROOT%release\FanControl.ClevoPlugin"

copy /y "%ROOT%Plugin\bin\%CONFIG%\net48\FanControl.ClevoPlugin.dll" "%ROOT%release\" > nul
copy /y "%ROOT%ClevoHelper\bin\%CONFIG%\net48\FanControl.ClevoHelper.exe" "%ROOT%release\FanControl.ClevoPlugin\" > nul

if exist "%ROOT%Dlls\ClevoEcInfo.dll" (
  copy /y "%ROOT%Dlls\ClevoEcInfo.dll" "%ROOT%release\FanControl.ClevoPlugin\" > nul
) else (
  echo WARNING: Dlls\ClevoEcInfo.dll was not found. The release folder will not be runnable until you add it.
)

if exist "%ROOT%Dlls\InsydeDCHU.dll" (
  copy /y "%ROOT%Dlls\InsydeDCHU.dll" "%ROOT%release\FanControl.ClevoPlugin\" > nul
) else (
  echo WARNING: Dlls\InsydeDCHU.dll was not found. The release folder will not expose RPM until you add it.
)

echo.
echo Release folder ready:
echo   %ROOT%release
echo.
echo Copy its contents to FanControl\Plugins, or zip the contents of the release folder for GitHub Releases.
goto end

:error
echo.
echo Build failed. Review the error above.
exit /b 1

:end
endlocal
