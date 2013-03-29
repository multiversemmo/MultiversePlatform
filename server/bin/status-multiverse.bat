@ echo off
::*******************************************************************
::
:: The Multiverse Platform is made available under the MIT License.
::
:: Copyright (c) 2012 The Multiverse Foundation
:: 
:: Permission is hereby granted, free of charge, to any person 
:: obtaining a copy of this software and associated documentation 
:: files (the "Software"), to deal in the Software without restriction, 
:: including without limitation the rights to use, copy, modify, 
:: merge, publish, distribute, sublicense, and/or sell copies 
:: of the Software, and to permit persons to whom the Software 
:: is furnished to do so, subject to the following conditions:
:: 
:: The above copyright notice and this permission notice shall be 
:: included in all copies or substantial portions of the Software.
:: 
:: THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
:: EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
:: OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
:: NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
:: HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
:: WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
:: FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
:: OR OTHER DEALINGS IN THE SOFTWARE.
:: 
:: ********************************************************************

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
echo : and Vista and 7.  If you see an error such as                               :
echo : " 'tasklist' is not recognized as an internal or external command..."       :
echo : Then your OS doesn't support this script.                                   :
echo : You can download a utility to run the script with XP Home Edition.  See     :
echo : http://www.multiversemmo.com/wiki/Running_the_Servers                       :
echo +-----------------------------------------------------------------------------+

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
