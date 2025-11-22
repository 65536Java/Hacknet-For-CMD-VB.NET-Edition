@echo off
echo Compile and reloading script assembly...

:: 1. 編譯 C# 檔案 (EngineAPI.dll)
.\bin\csc.exe /target:library /out:".\lib\EngineAPI.dll" ".\Engine\*.cs" /reference:System.dll
if errorlevel 1 (
    echo.
    echo ERROR: C# compilation failed.
    goto :end
)

:: 2. 遞迴收集所有 VB 檔（包含子資料夾）
setlocal EnableDelayedExpansion
set "VBFILES="
for /R ".\assets\Scripts" %%F in (*.vb) do (
    set "VBFILES=!VBFILES! "%%F""
)
:: 若還有其他要一起編譯的檔案，可在此 append

:: 3. 編譯 VB 函式庫 (Assembly-VBNET.dll)
:: 加上 System.Runtime.Serialization 參考（視需要）
.\bin\vbc.exe /target:library /reference:".\lib\EngineAPI.dll" /reference:"System.Linq.dll" /out:".\lib\Assembly-VBNET.dll" %VBFILES% /nowin32manifest
if errorlevel 1 (
    echo.
    echo ERROR: VB library compilation failed.
    goto :end
)

:: 4. 編譯最終 EXE（src\*.vb）
.\bin\vbc.exe /target:exe src\*.vb /reference:".\lib\Assembly-VBNET.dll" /nologo /win32icon:icon.ico /out:HacknetCMD.exe /nowin32manifest
if errorlevel 1 (
    echo.
    echo ERROR: EXE compilation failed.
    goto :end
)

:: 複製原生 DLL
copy .\lib\discord-rpc.dll . > nul

:end
echo.
echo Done.
echo Press Enter to Run Application.
pause > nul
start HacknetCMD.exe