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
:: This batch file generates a public and private key for the Multiverse Master server 
:: and associated world login servers.  
:: DO NOT DISTRIBUTE THE PRIVATE KEYS OR YOUR SECURITY WILL BE COMPROMISED

if exist masterserverkeys.txt (
	echo !~Master Server Keys Already Exist~!
	pause
	exit
)

:: Check that script is being run from mv_home\master
if not defined MV_HOME (
  echo MV_HOME is not defined, using relative paths
  if exist gen-masterserver-keys.bat (
    set MV_HOME=..
  ) else (
    echo Batch script must be run from MV_HOME\master directory!
  )
) 

echo MV_HOME is %MV_HOME%

if not defined MV_JAR (
  set MV_JAR=%MV_HOME%\dist\lib\multiverse.jar
)
if not defined MARS_JAR (
  set MARS_JAR=%MV_HOME%\dist\lib\mars.jar
)
if not defined INJECTED_JAR (
  set INJECTED_JAR=%MV_HOME%\dist\lib\injected.jar
)

set JYTHON=%MV_HOME%\other\jython.jar
set RHINO=%MV_HOME%\other\rhino1_5R5\js.jar
set GETOPT=%MV_HOME%\other\java-getopt-1.0.11.jar
set LOG4J=%MV_HOME%\other\log4j-1.2.14.jar
set BCEL=%MV_HOME%\other\bcel-5.2.jar
set EXT_JAR=%MV_HOME%\dist\lib\%MV_WORLDNAME%.jar

set MV_CLASSPATH=%INJECTED_JAR%;%MV_JAR%;%MARS_JAR%;%EXT_JAR%;%RHINO%;%GETOPT%;%JYTHON%;%LOG4J%;%BCEL%

set JVM_TYPE=client
set JVM_HEAP_FLAGS=-Xms32m -Xmx256m

set JAVA_FLAGS=-%JVM_TYPE% %JVM_HEAP_FLAGS% -cp "%MV_CLASSPATH%" -Dmultiverse.propertyfile=%PROPFILE%
set JAVA_FLAGS=%JAVA_FLAGS% -Dmultiverse.logs=%MV_LOGS%

echo Java Flags are: %JAVA_FLAGS%

@ echo on
START /B java %JAVA_FLAGS% multiverse.server.util.SecureTokenManager > masterserverkeys.txt
echo Master Server keys stored in masterserverkeys.txt

pause