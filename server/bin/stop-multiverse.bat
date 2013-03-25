@echo off

:: Check that script is being run from mv_home\bin
if not defined MV_HOME (
  echo MV_HOME is not defined, using relative paths
  if exist .\start-multiverse.bat (
    set MV_HOME=..
  ) else (
    echo Batch script must be run from MV_HOME\bin directory!
    exit
  )
) 

if exist run\%MV_WORLDNAME%\combat.bat (
  call run\%MV_WORLDNAME%\combat.bat
)
echo Stopping combat server process ID %pid%
tskill %pid%

if exist run\%MV_WORLDNAME%\instance.bat (
 call run\%MV_WORLDNAME%\instance.bat
)
echo Stopping instance server process ID %pid%
tskill %pid%

if exist run\%MV_WORLDNAME%\login_manager.bat (
  call run\%MV_WORLDNAME%\login_manager.bat
)
echo Stopping login mangaer server process ID %pid%
tskill %pid%

if exist run\%MV_WORLDNAME%\mobserver.bat (
  call run\%MV_WORLDNAME%\mobserver.bat
)
echo Stopping mob server process ID %pid%
tskill %pid%

if exist run\%MV_WORLDNAME%\objmgr.bat (
  call run\%MV_WORLDNAME%\objmgr.bat
)
echo Stopping object manager server process ID %pid%
tskill %pid%

if exist run\%MV_WORLDNAME%\proxy_1.bat (
  call run\%MV_WORLDNAME%\proxy_1.bat
)
echo Stopping proxy server process ID %pid%
tskill %pid%

if exist run\%MV_WORLDNAME%\voiceserver.bat (
  call run\%MV_WORLDNAME%\voiceserver.bat
)
echo Stopping voice server process ID %pid%
tskill %pid%

if exist run\%MV_WORLDNAME%\wmgr_1.bat (
  call run\%MV_WORLDNAME%\wmgr_1.bat
)
echo Stopping world manager server process ID %pid%
tskill %pid%



if exist run\%MV_WORLDNAME%\combat.bat (
  call run\%MV_WORLDNAME%\domain.bat
)
echo Stopping message domain server process ID %pid%
tskill %pid%

:exit
