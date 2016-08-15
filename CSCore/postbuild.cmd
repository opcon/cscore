setlocal ENABLEDELAYEDEXPANSION

set errorCode=0

set target=%1
set project=%2
set solutiondir=%3
set configname=%4

set sdk=%PROGRAMFILES(x86)%\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools
set framework=%WINDIR%\Microsoft.NET\Framework\v2.0.50727
set inlineILCompiler=%solutiondir%Tools\InlineILCompiler\InlineILCompiler\bin\%configname%\InlineILCompiler.exe
set cscli=%solutiondir%Tools\CSCli\bin\%configname%\CSCli.exe

IF exist "%sdk%\." (
echo The Microsoft Windows SDK was found.
) else (
echo The Microsoft Windows SDK Dir was not found. Check the following path: "%sdk%"
goto EXIT_ERR
)
IF exist "%framework%\." (
echo The .NET Framework Dir was found.
) else (
echo The Framework-Dir was not found. Check the following path: "%framework%"
goto EXIT_ERR
)
IF exist "%inlineILCompiler%" (
echo Found the inline-il-compiler.
) else (
echo The inline-il-compiler was not found. Check the following path: "%inlineILCompiler%"
goto EXIT_ERR
)
IF exist "%cscli%" (
echo Found the cscli-compiler.
) else (
echo The cscli was not found. Check the following path: "%cscli%"
goto EXIT_ERR
)

if %configname% == Debug (
echo Build-Configuration: DEBUG
echo    DEBUG=IMPL
echo    OPTIMIZE
set ilasm_args=/DLL /DEBUG=IMPL
) else (
if %configname% == Release (
echo Build-Configuration: RELEASE
echo    DEBUG
echo    OPTIMIZE
set ilasm_args=/DLL /OPTIMIZE
) else (
echo Invalid Configuration.
goto EXIT_ERR
)
)

echo.
echo.
echo Calling the inline-il-compiler ...
call "%inlineILCompiler%"

echo.
echo.
echo Calling CSCli ...
call "%cscli%" -file:"%target%" -r:"RemoveObjAttribute" -c:"CSCalliAttribute"

:EXIT
EXIT /B %errorCode%

:EXIT_ERR
set errorCode=-1
goto EXIT