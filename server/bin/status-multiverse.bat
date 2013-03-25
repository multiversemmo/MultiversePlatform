@ echo off
SETLOCAL ENABLEDELAYEDEXPANSION

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

echo +-----------------------------------------------------------------------------+
echo : NOTE: This script works only on Windows XP Pro and Media Center Edition     :
echo : If you see an error such as                                                 :
echo : " 'tasklist' is not recognized as an internal or external command..."       :
echo : Then your OS doesn't support this script.                                   :
echo : You can download a utility to run the script with XP Home Edition.  See     :
echo : http://update.multiverse.net/wiki/index.php/Running_the_Servers             :
echo +-----------------------------------------------------------------------------+

pause

if not defined DEFAULT_MV_PROPERTYFILE (
   set DEFAULT_MV_PROPERTYFILE=multiverse.properties
)

if not defined MV_JAR (
  set MV_JAR=%MV_HOME%\dist\lib\multiverse.jar
)

set PROPFILE=%1
if %1x==x (
  set PROPFILE=%DEFAULT_MV_PROPERTYFILE%
)

set JAVA_FLAGS=-cp "%MV_JAR%;." -Dmultiverse.propertyfile=%PROPFILE%

:: Set value of MV_WORLDNAME from multiverse.worldname in property file
java %JAVA_FLAGS% -Dwin_env_var=MV_WORLDNAME ^
 multiverse.scripts.PropertyGetter ^
 multiverse.worldname > status-tmp.bat
call status-tmp.bat
del status-tmp.bat

for %%v in (run/%MV_WORLDNAME%/*.bat) do  (
  call run\%MV_WORLDNAME%\%%v
  tasklist /FI "PID eq !pid!" /NH > run\%MV_WORLDNAME%\status.txt 2>NUL
  java %JAVA_FLAGS% multiverse.scripts.ServerStatus %MV_WORLDNAME% %%~nv  
)

if exist run\%MV_WORLDNAME%\status.txt (
   del run\%MV_WORLDNAME%\status.txt
)
