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
:: This batch file starts the Multiverse Master sever processes on Windows
:: You must have installed Java, a database, JDBC driver, and the Multiverse servers
:: Copyright 2012 The Multiverse Software Foundation
::

:: **********************************************
:: SET VERBOSE
:: **********************************************
set VERBOSE=true

echo *********************************************
echo *** Preparing Master Server Environment
echo ***

:: **********************************************
:: SET PROPFILE_MASTER
:: **********************************************
set PROPFILE_MASTER=%1
if %1x==x (
  set PROPFILE_MASTER=master.properties
)
if %VERBOSE%==true echo *** PROPFILE_MASTER is %PROPFILE_MASTER%

:: **********************************************
:: SET MV_HOME_MASTER
:: **********************************************
:: Check that script is being run from multiverse\master
if exist .\start-master.bat (
    set MV_HOME_MASTER=..
) else (
  echo !!! Batch script must be run from multiverse\master directory!
  goto:eof
)
if %VERBOSE%==true echo *** MV_HOME_MASTER is %MV_HOME_MASTER%
  
:: **********************************************
:: SET MV_JAR_MASTER
:: **********************************************
if not defined MV_JAR_MASTER (
  set MV_JAR_MASTER="%MV_HOME_MASTER%"\dist\lib\multiverse.jar
)
if %VERBOSE%==true echo *** MV_JAR_MASTER is %MV_JAR_MASTER%

:: **********************************************
:: SET MARS_JAR_MASTER
:: **********************************************
if not defined MARS_JAR_MASTER (
  set MARS_JAR_MASTER="%MV_HOME_MASTER%"\dist\lib\mars.jar
)
if %VERBOSE%==true echo *** MARS_JAR_MASTER is %MARS_JAR_MASTER%

:: **********************************************
:: SET INJECTED_JAR_MASTER
:: **********************************************
if not defined INJECTED_JAR_MASTER (
  set INJECTED_JAR_MASTER="%MV_HOME_MASTER%"\dist\lib\injected.jar
)
if %VERBOSE%==true echo *** INJECTED_JAR_MASTER is %INJECTED_JAR_MASTER%

:: **********************************************
:: SET JVM_TYPE_MASTER
:: **********************************************
java -cp %MV_JAR_MASTER% ^
	-Dmultiverse.propertyfile=%PROPFILE_MASTER% ^
  	-Dwin_env_var=JVM_TYPE_MASTER ^
  	multiverse.scripts.PropertyGetter java.jvm_type > tmp.bat

call tmp.bat
del tmp.bat

if %JVM_TYPE_MASTER%==null (
  if %VERBOSE%==true echo *** java.jvm_type not configured. Using batch file default
  set JVM_TYPE_MASTER=client
)
if %VERBOSE%==true echo *** JVM_TYPE_MASTER is %JVM_TYPE_MASTER%

:: **********************************************
:: SET JVM_HEAP_FLAGS_MASTER
:: **********************************************
java -cp %MV_JAR_MASTER% ^
	-Dmultiverse.propertyfile="%PROPFILE_MASTER%" ^
  	-Dwin_env_var=JVM_HEAP_FLAGS_MASTER1 ^
  	multiverse.scripts.PropertyGetter java.jvm_heap_flags1 > tmp.bat

call tmp.bat
del tmp.bat

java -cp %MV_JAR_MASTER% ^
	-Dmultiverse.propertyfile="%PROPFILE_MASTER%" ^
  	-Dwin_env_var=JVM_HEAP_FLAGS_MASTER2 ^
  	multiverse.scripts.PropertyGetter java.jvm_heap_flags2 > tmp.bat

call tmp.bat
del tmp.bat

if %JVM_HEAP_FLAGS_MASTER1%==null (
  if %VERBOSE%==true echo *** java.jvm_heap_flags1 not configured. Using batch file default
  set JVM_HEAP_FLAGS_MASTER1=-Xms32m
)
if %JVM_HEAP_FLAGS_MASTER2%==null (
  if %VERBOSE%==true echo *** java.jvm_heap_flags2 not configured. Using batch file default
  set JVM_HEAP_FLAGS_MASTER2=-Xmx256m
)
set JVM_HEAP_FLAGS_MASTER=%JVM_HEAP_FLAGS_MASTER1% %JVM_HEAP_FLAGS_MASTER2%

if %VERBOSE%==true echo *** JVM_HEAP_FLAGS_MASTER is %JVM_HEAP_FLAGS_MASTER%

:: **********************************************
:: SET ENABLE_MGMT_MASTER
:: **********************************************
:: Set to true to enable JMX management and monitoring
java -cp %MV_JAR_MASTER% ^
	-Dmultiverse.propertyfile=%PROPFILE_MASTER% ^
  	-Dwin_env_var=ENABLE_MGMT_MASTER ^
  	multiverse.scripts.PropertyGetter multiverse.enable_mgmt > tmp.bat

call tmp.bat
del tmp.bat

if %ENABLE_MGMT_MASTER%==null (
  if %VERBOSE%==true echo *** multiverse.enable_mgmt not configured. Using batch file default
  set ENABLE_MGMT_MASTER=false
)
if %VERBOSE%==true echo *** ENABLE_MGMT_MASTER is %ENABLE_MGMT_MASTER%

:: **********************************************
:: SET JDBC_MASTER
:: **********************************************
:: Get path to JDBC JAR file from property file, unless set in env. var.
java -cp %MV_JAR_MASTER% ^
	-Dmultiverse.propertyfile=%PROPFILE_MASTER% ^
  	-Dwin_env_var=JDBC_MASTER ^
  	multiverse.scripts.PropertyGetter multiverse.jdbcJarPath > tmp.bat

call tmp.bat
del tmp.bat

if %JDBC_MASTER%==null (
  if %VERBOSE%==true echo *** multiverse.jdbcJarPath not configured. Using batch file default
  set JDBC_MASTER=..\other\mysql-jdbc\mysql-connector-java-3.1.14-bin.jar
)
if %VERBOSE%==true echo *** JDBC_MASTER is %JDBC_MASTER%

:: **********************************************
:: SET MV_LOGS_MASTER
:: **********************************************
set MV_LOGS_MASTER=%MV_HOME_MASTER%\logs\master

if not exist %MV_LOGS_MASTER% (
  mkdir %MV_LOGS_MASTER%
)

:: **********************************************
:: SET DELETE_LOGS_ON_STARTUP_MASTER
:: **********************************************
java -cp %MV_JAR_MASTER% ^
	-Dmultiverse.propertyfile=%PROPFILE_MASTER% ^
  	-Dwin_env_var=DELETE_LOGS_ON_STARTUP_MASTER ^
  	multiverse.scripts.PropertyGetter multiverse.delete_logs_on_startup > tmp.bat

call tmp.bat
del tmp.bat

if %DELETE_LOGS_ON_STARTUP_MASTER%==null (
	if %VERBOSE%==true echo *** multiverse.delete_logs_on_startup not configured. Using batch file default
  	set DELETE_LOGS_ON_STARTUP_MASTER=true
)

if %DELETE_LOGS_ON_STARTUP_MASTER%==true (
  echo *** Deleting existing log files
  del /Q %MV_LOGS_MASTER%\*.out*
)

:: **********************************************
:: SET RUN
:: **********************************************
if not exist run (
  echo *** Creating run directory
  mkdir run
)

del .\run\*.* /Q

set CMDLINE_PROPS_MASTER=
if defined MV_HOSTNAME_MASTER (
  set CMDLINE_PROPS_MASTER=-Pmultiverse.hostname=%MV_HOSTNAME_MASTER%
)
if %VERBOSE%==true echo *** CMDLINE_PROPS_MASTER is %CMDLINE_PROPS_MASTER% 

set MV_COMMON_MASTER=%MV_HOME_MASTER%\config\common
if %VERBOSE%==true echo *** MV_COMMON_MASTER is %MV_COMMON_MASTER%

set JYTHON_MASTER=%MV_HOME_MASTER%\other\jython.jar
if %VERBOSE%==true echo *** JYTHON_MASTER is %JYTHON_MASTER%

set RHINO_MASTER=%MV_HOME_MASTER%\other\rhino1_5R5\js.jar
if %VERBOSE%==true echo *** RHINO_MASTER is %RHINO_MASTER%

set GETOPT_MASTER=%MV_HOME_MASTER%\other\java-getopt-1.0.11.jar
if %VERBOSE%==true echo *** GETOPT_MASTER is %GETOPT_MASTER%

set LOG4J_MASTER=%MV_HOME_MASTER%\other\log4j-1.2.14.jar
if %VERBOSE%==true echo *** LOG4J_MASTER is %LOG4J_MASTER%

set BCEL_MASTER=%MV_HOME_MASTER%\other\bcel-5.2.jar
if %VERBOSE%==true echo *** BCEL_MASTER is %BCEL_MASTER%

set MV_CLASSPATH_MASTER=%INJECTED_JAR_MASTER%;%MV_JAR_MASTER%;%MARS_JAR_MASTER%;%EXT_JAR_MASTER%;%RHINO_MASTER%;%GETOPT_MASTER%;%JYTHON_MASTER%;%JDBC_MASTER%;%LOG4J_MASTER%;%BCEL_MASTER%

set MV_BIN_MASTER=%MV_HOME_MASTER%\master
if %VERBOSE%==true echo *** MV_BIN_MASTER is %MV_BIN_MASTER%

set JAVA_FLAGS_MASTER=-%JVM_TYPE_MASTER% %JVM_HEAP_FLAGS_MASTER% -cp "%MV_CLASSPATH_MASTER%" -Dmultiverse.propertyfile=%PROPFILE_MASTER%
set JAVA_FLAGS_MASTER=%JAVA_FLAGS_MASTER% -Dmultiverse.logs=%MV_LOGS_MASTER%

if %VERBOSE%==true echo *** MV_HOME_MASTER is %MV_HOME_MASTER%
if %VERBOSE%==true echo *** MV_LOGS_MASTER is %MV_LOGS_MASTER%

echo *** Master Server Environment Prepared
echo *********************************************
echo *** Starting master server
echo ***

START /B java %JAVA_FLAGS_MASTER% ^
  -Dmultiverse.loggername=master ^
  multiverse.server.engine.MasterServer ^
  %CMDLINE_PROPS_MASTER% ^
  %MV_BIN_MASTER%\master_server.py 

rem echo $! > %MV_RUN%\master.pid
     
echo *** Master Server Started
echo *********************************************
