@echo off

ver | find "2003" > nul
if %ERRORLEVEL% == 0 goto ver_2003

ver | find "XP" > nul
if %ERRORLEVEL% == 0 goto ver_xp

ver | find "2000" > nul
if %ERRORLEVEL% == 0 goto ver_2000

ver | find "NT" > nul
if %ERRORLEVEL% == 0 goto ver_nt

if not exist %SystemRoot%\system32\systeminfo.exe goto warnthenexit

systeminfo | find "OS Name" > %TEMP%\osname.txt
FOR /F "usebackq delims=: tokens=2" %%i IN (%TEMP%\osname.txt) DO set vers=%%i

echo %vers% | find "Windows 7" > nul
if %ERRORLEVEL% == 0 goto ver_7

echo %vers% | find "Windows Server 2008" > nul
if %ERRORLEVEL% == 0 goto ver_2008

echo %vers% | find "Windows Vista" > nul
if %ERRORLEVEL% == 0 goto ver_vista

goto warnthenexit

:ver_7
:Run Windows 7 specific commands here.
echo Windows 7
goto exit

:ver_2008
:Run Windows Server 2008 specific commands here.
echo Windows Server 2008
goto exit

:ver_vista
:Run Windows Vista specific commands here.
echo Windows Vista
goto exit

:ver_2003
:Run Windows Server 2003 specific commands here.
echo Windows Server 2003
goto exit

:ver_xp
:Run Windows XP specific commands here.
echo Windows XP
goto exit

:ver_2000
:Run Windows 2000 specific commands here.
echo Windows 2000
goto exit

:ver_nt
:Run Windows NT specific commands here.
echo Windows NT
goto exit

:warnthenexit
echo Machine undetermined.

:exit