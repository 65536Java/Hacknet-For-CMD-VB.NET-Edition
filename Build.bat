@echo off
echo Compile and reloading script assembly...

:: 1. 編譯 C# 檔案 (EngineAPI.dll)
:: 確保 .\Engine\*.cs 包含所有 C# 檔案
.\bin\csc.exe /target:library /out:".\lib\EngineAPI.dll" ".\Engine\*.cs" /reference:"System.dll"

:: 檢查 C# 編譯是否成功
if errorlevel 1 (
    echo.
    echo ERROR: C# compilation failed. Please check files in the .\Engine\ folder.
    goto :end
)

:: 2. 編譯 VB.NET 函式庫 (Assembly-VBNET.dll)
:: *** 確保正確引用 EngineAPI.dll (這是解決 BC30002 的關鍵) ***
.\bin\vbc.exe /target:library /reference:".\lib\EngineAPI.dll" /reference:"System.Runtime.dll" /out:".\lib\Assembly-VBNET.dll" /reference:"System.dll" /reference:"System.IO.dll" /reference:"System.Core.dll" .\assets\Scripts\*.vb /nowin32manifest

:: 3. 編譯最終 EXE
.\bin\vbc.exe /target:exe src\*.vb /reference:"lib\Assembly-VBNET.dll" /nologo /out:HacknetCMDBasic.exe /nowin32manifest

:: --------------------------------------------------------------------------------------------------
:: 複製原生 DLL
:: --------------------------------------------------------------------------------------------------
echo Copying discord-rpc.dll (Native) to executable directory...
copy .\lib\discord-rpc.dll . > nul

:end
pause