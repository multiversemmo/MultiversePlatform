@echo off
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
taskkill /PID %pid% /F

if exist run\%MV_WORLDNAME%\instance.bat (
 call run\%MV_WORLDNAME%\instance.bat
)
echo Stopping instance server process ID %pid%
taskkill /PID %pid% /F

if exist run\%MV_WORLDNAME%\login_manager.bat (
  call run\%MV_WORLDNAME%\login_manager.bat
)
echo Stopping login mangaer server process ID %pid%
taskkill /PID %pid% /F

if exist run\%MV_WORLDNAME%\mobserver.bat (
  call run\%MV_WORLDNAME%\mobserver.bat
)
echo Stopping mob server process ID %pid%
taskkill /PID %pid% /F

if exist run\%MV_WORLDNAME%\objmgr.bat (
  call run\%MV_WORLDNAME%\objmgr.bat
)
echo Stopping object manager server process ID %pid%
taskkill /PID %pid% /F

if exist run\%MV_WORLDNAME%\proxy_1.bat (
  call run\%MV_WORLDNAME%\proxy_1.bat
)
echo Stopping proxy server process ID %pid%
taskkill /PID %pid% /F

if exist run\%MV_WORLDNAME%\voiceserver.bat (
  call run\%MV_WORLDNAME%\voiceserver.bat
)
echo Stopping voice server process ID %pid%
taskkill /PID %pid% /F

if exist run\%MV_WORLDNAME%\wmgr_1.bat (
  call run\%MV_WORLDNAME%\wmgr_1.bat
)
echo Stopping world manager server process ID %pid%
taskkill /PID %pid% /F



if exist run\%MV_WORLDNAME%\combat.bat (
  call run\%MV_WORLDNAME%\domain.bat
)
echo Stopping message domain server process ID %pid%
taskkill /PID %pid% /F

:exit
