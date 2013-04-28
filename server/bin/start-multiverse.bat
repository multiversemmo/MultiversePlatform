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
::
:: This batch file runs the Multiverse sever processes on Windows
:: You must have installed Java, a database, JDBC driver, and the Multiverse servers
:: 
:: Set DEFAULT_MV_PROPERTYFILE if you want to use a different defult property file
:: 
::call winver.bat WINVER

:: **********************************************
:: SET DEFAULT_MV_PROPERTYFILE
:: **********************************************
if not defined DEFAULT_MV_PROPERTYFILE set DEFAULT_MV_PROPERTYFILE=multiverse.properties
echo DEFAULT_MV_PROPERTYFILE is %DEFAULT_MV_PROPERTYFILE%

:: **********************************************
:: SET PROPFILE
:: **********************************************
set PROPFILE=%1
if %1x==x (
  set PROPFILE=%DEFAULT_MV_PROPERTYFILE%
) else (
  set DEFAULT_MV_PROPERTYFILE=%PROPFILE%
)
echo Using property file %PROPFILE%

:: **********************************************
:: SET MV_HOME
:: **********************************************
:: Check that script is being run from mv_home\bin
if not defined MV_HOME (
  echo MV_HOME is not defined, using relative paths
  if exist .\start-multiverse.bat (
    set MV_HOME=..
  ) else (
    echo Batch script must be run from MV_HOME\bin directory!
  )
) 
echo MV_HOME is %MV_HOME%

:: **********************************************
:: SET MV_JAR
:: **********************************************
if not defined MV_JAR set MV_JAR=%MV_HOME%\dist\lib\multiverse.jar
echo MV_JAR is %MV_JAR%

:: **********************************************
:: SET MARS_JAR
:: **********************************************
if not defined MARS_JAR set MARS_JAR=%MV_HOME%\dist\lib\mars.jar
echo MARS_JAR is %MARS_JAR%

:: **********************************************
:: SET INJECTED_JAR
:: **********************************************
if not defined INJECTED_JAR set INJECTED_JAR=%MV_HOME%\dist\lib\injected.jar
echo INJECTED_JAR is %INJECTED_JAR%

:: **********************************************
:: SET ENABLE_MGMT
:: **********************************************
:: Set to true to enable JMX management and monitoring
java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=ENABLE_MGMT multiverse.scripts.PropertyGetter multiverse.enable_mgmt > tmp.bat
call tmp.bat
del tmp.bat

if ENABLE_MGMT==null set ENABLE_MGMT=false
echo ENABLE_MGMT is %ENABLE_MGMT%

:: **********************************************
:: SET MV_WORLDNAME
:: **********************************************
:: Set value of MV_WORLDNAME from multiverse.worldname in property file
java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=MV_WORLDNAME multiverse.scripts.PropertyGetter multiverse.worldname > tmp.bat
call tmp.bat
del tmp.bat

if not defined MV_WORLDNAME set MV_WORLDNAME=sampleworld
echo MV_WORLDNAME is %MV_WORLDNAME%

:: **********************************************
:: SET JVM_TYPE
:: **********************************************
:: Change to "server" to use the server Java VM
:: Get the Java Type to use for loading the servers
java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=JVM_TYPE multiverse.scripts.PropertyGetter java.type > tmp.bat
call tmp.bat
del tmp.bat

if not defined JVM_TYPE set JVM_TYPE=client
echo JVM_TYPE is %JVM_TYPE%

:: **********************************************
:: SET JVM_HEAP_FLAGS
:: **********************************************
:: Get the JVM Heap Flags
java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=JVM_HEAP_FLAGS multiverse.scripts.PropertyGetter java.jvm_heap_flags > tmp.bat
call tmp.bat
del tmp.bat

if not defined JVM_HEAP_FLAGS set JVM_HEAP_FLAGS=-Xms32m -Xmx256m
echo JVM_HEAP_FLAGS is %JVM_HEAP_FLAGS%

:: **********************************************
:: SET JDBC
:: **********************************************
:: Get path to JDBC JAR file from property file, unless set in env. var.
java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=JDBC multiverse.scripts.PropertyGetter multiverse.jdbcJarPath > tmp.bat
call tmp.bat
del tmp.bat

if not defined JDBC set JDBC=..\other\mysql-jdbc\mysql-connector-java-3.1.14-bin.jar
echo JDBC is %JDBC%

set JYTHON=%MV_HOME%\other\jython.jar
echo JYTHON is %JYTHON%

set RHINO=%MV_HOME%\other\rhino1_5R5\js.jar
echo RHINO is %RHINO%

set GETOPT=%MV_HOME%\other\java-getopt-1.0.11.jar
echo GETOPT is %GETOPT%

set LOG4J=%MV_HOME%\other\log4j-1.2.14.jar
echo LOG4J is %LOG4J%

set BCEL=%MV_HOME%\other\bcel-5.2.jar
echo BCEL is %BCEL%

set EXT_JAR=%MV_HOME%\dist\lib\%MV_WORLDNAME%.jar
echo EXT_JAR is %EXT_JAR%

set MV_CLASSPATH=%INJECTED_JAR%;%MV_JAR%;%MARS_JAR%;%EXT_JAR%;%RHINO%;%GETOPT%;%JYTHON%;%JDBC%;%LOG4J%;%BCEL%
echo MV_CLASSPATH is %MV_CLASSPATH%

if defined MV_HOSTNAME set CMDLINE_PROPS=-Pmultiverse.hostname=%MV_HOSTNAME%

:: **********************************************
:: SET MVW_FILENAME
:: **********************************************
:: Set value of MVW_FILENAME from multiverse.mvwfile in property file if it is there, if not set based on MV_WORLDNAME
java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=MVW_FILENAME multiverse.scripts.PropertyGetter multiverse.mvw_filename > tmp.bat

call tmp.bat
del tmp.bat

if %MVW_FILENAME%==null set MVW_FILENAME="%MV_HOME%\config\%MV_WORLDNAME%\%MV_WORLDNAME%.mvw"
echo Using world file %MVW_FILENAME%

:: **********************************************
:: SET MV_COMMON
:: **********************************************
set MV_COMMON="%MV_HOME%\config\common"
echo Using common config file %MV_COMMON%

:: **********************************************
:: SET MV_WORLD
:: **********************************************
set MV_WORLD="%MV_HOME%\config\%MV_WORLDNAME%"
echo Using world config file %MV_WORLD%

:: **********************************************
:: SET MV_LOGS
:: **********************************************
if not defined MV_LOGS set MV_LOGS="%MV_HOME%\logs\%MV_WORLDNAME%"

if not exist %MV_LOGS% mkdir %MV_LOGS%
echo Using log folder %MV_LOGS%

:: **********************************************
:: SET DELETE_LOGS_ON_STARTUP
:: **********************************************
if not defined DELETE_LOGS_ON_STARTUP (
  java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=DELETE_LOGS_ON_STARTUP multiverse.scripts.PropertyGetter multiverse.delete_logs_on_startup > tmp.bat
  call tmp.bat
  del tmp.bat
)

if %DELETE_LOGS_ON_STARTUP%==true (
  if exist %MV_LOGS%\*.out (
	echo Deleting existing log files
  	del /Q %MV_LOGS%\*.out*
  ) else (
  	  echo No log files to delete
  )
)

:: **********************************************
:: SET RUN_FOLDER
:: **********************************************
set RUN_FOLDER=%MV_HOME%\run

if not exist %RUN_FOLDER% (
  echo Creating RUN_FOLDER %RUN_FOLDER%
  mkdir %RUN_FOLDER%
)
if not exist %RUN_FOLDER%\%MV_WORLDNAME% (
  echo Creating %RUN_FOLDER%\%MV_WORLDNAME% directory
  mkdir %RUN_FOLDER%\%MV_WORLDNAME%
)
if exist %RUN_FOLDER%\%MV_WORLDNAME%\*.bat (
  del /Q %RUN_FOLDER%\%MV_WORLDNAME%\*.bat
)

:: **********************************************
:: SET JAVA_FLAGS
:: **********************************************
set JAVA_FLAGS=%JAVA_FLAGS% -%JVM_TYPE% %JVM_HEAP_FLAGS% 
set JAVA_FLAGS=%JAVA_FLAGS% -cp "%MV_CLASSPATH%" 
set JAVA_FLAGS=%JAVA_FLAGS% -Dmultiverse.propertyfile=%PROPFILE%
set JAVA_FLAGS=%JAVA_FLAGS% -Dmultiverse.logs=%MV_LOGS%
set JAVA_FLAGS=%JAVA_FLAGS% -Dmultiverse.worldname=%MV_WORLDNAME%
set JAVA_FLAGS=%JAVA_FLAGS% -Dmultiverse.rundir=%RUN_FOLDER%\%MV_WORLDNAME%

if %ENABLE_MGMT%==true (
  echo Enabling JMX mgmt and monitoring
  set JAVA_FLAGS=%JAVA_FLAGS% -Dcom.sun.management.jmxremote
) 

:: Agent Names and Plugin Types are passed to the Message Domain Server 
set AGENT_NAMES=-a combat -a wmgr -a mobserver -a objmgr -a login_manager -a proxy -a instance -a voiceserver
set PLUGIN_TYPES=-p Login,1 -p Proxy,1 -p ObjectManager,1 -p WorldManager,1 -p Inventory,1 -p MobManager,1 -p Quest,1 -p Instance,1 -p Voice,1 -p Trainer,1 -p Group,1 -p Combat,1 -p ClassAbility,1 -p Domain,1

echo Using world script directory %MV_WORLD%
echo Using log directory %MV_LOGS%
echo Using common directory %MV_COMMON%
echo Java Flags are: %JAVA_FLAGS%

echo Starting message domain server
@ echo off
START /B java  %JAVA_FLAGS% ^
    -Dmultiverse.loggername=domain ^
    :: 20130428 CobaltBlues * Adding patch so worldname is passed to jar modules
    -Dmultiverse.worldname=%MV_WORLDNAME% ^
    multiverse.msgsys.DomainServer ^
    %CMDLINE_PROPS% ^
    -t %MV_COMMON%\typenumbers.txt ^
    %AGENT_NAMES% ^
    %PLUGIN_TYPES%

echo Starting world manager
START /B java ^
    %JAVA_FLAGS% ^
    -Dmultiverse.agenttype=wmgr ^
    -Dmultiverse.loggername=wmgr ^
    :: 20130428 CobaltBlues * Adding patch so worldname is passed to jar modules
    -Dmultiverse.worldname=%MV_WORLDNAME% ^
    multiverse.server.engine.Engine ^
    %CMDLINE_PROPS% ^
    -i wmgr_local1.py ^
    -i %MV_COMMON%\mvmessages.py ^
    -i %MV_WORLD%\worldmessages.py ^
    -t %MV_COMMON%\typenumbers.txt ^
    %MV_COMMON%\global_props.py ^
    %MV_WORLD%\global_props.py ^
    %MV_COMMON%\world_mgr1.py ^
    %MV_WORLD%\extensions_wmgr.py
        
echo Starting combat server
START /B java ^
    %JAVA_FLAGS% ^
    -Dmultiverse.loggername=combat ^
    :: 20130428 CobaltBlues * Adding patch so worldname is passed to jar modules
    -Dmultiverse.worldname=%MV_WORLDNAME% ^    
    multiverse.server.engine.Engine ^
    %CMDLINE_PROPS% ^
    -i wmgr_local1.py ^
    -i %MV_COMMON%\mvmessages.py ^
    -i %MV_WORLD%\worldmessages.py ^
    -t %MV_COMMON%\typenumbers.txt ^
    %MV_COMMON%\global_props.py ^
    %MV_WORLD%\global_props.py ^
    %MV_COMMON%\skill_db.py ^
    %MV_WORLD%\skill_db.py ^
    %MV_COMMON%\ability_db.py ^
    %MV_WORLD%\ability_db.py ^
    %MV_WORLD%\classabilityplugin.py ^
    %MV_WORLD%\combat.py ^
    %MV_WORLD%\extensions_combat.py ^
    %MV_COMMON%\profession_db.py ^
    %MV_WORLD%\profession_db.py ^
    %MV_COMMON%\groupplugin.py ^
    %MV_WORLD%\group.py

echo Starting instance server
START /B java ^
    %JAVA_FLAGS% ^
    -Dmultiverse.loggername=instance ^
    :: 20130428 CobaltBlues * Adding patch so worldname is passed to jar modules
    -Dmultiverse.worldname=%MV_WORLDNAME% ^
    multiverse.server.engine.Engine ^
    %CMDLINE_PROPS% ^
    -i %MV_COMMON%\mvmessages.py ^
    -i %MV_WORLD%\worldmessages.py ^
    -t %MV_COMMON%\typenumbers.txt ^
    %MV_COMMON%\global_props.py ^
    %MV_WORLD%\global_props.py ^
    %MV_COMMON%\instance.py ^
    %MV_WORLD%\startup_instance.py

echo Starting object manager
START /B java ^
    %JAVA_FLAGS% ^
    -Dmultiverse.loggername=objmgr ^
    :: 20130428 CobaltBlues * Adding patch so worldname is passed to jar modules
    -Dmultiverse.worldname=%MV_WORLDNAME% ^
    multiverse.server.engine.Engine ^
    %CMDLINE_PROPS% ^
    -i wmgr_local1.py ^
    -i %MV_COMMON%\mvmessages.py ^
    -i %MV_WORLD%\worldmessages.py ^
    -t %MV_COMMON%\typenumbers.txt ^
    %MV_COMMON%\global_props.py ^
    %MV_WORLD%\global_props.py ^
    %MV_WORLD%\templates.py ^
    %MV_COMMON%\obj_manager.py ^
    %MV_WORLD%\mobs_db.py ^
    %MV_WORLD%\items_db.py ^
    %MV_WORLD%\extensions_objmgr.py

echo Starting login manager
START /B java ^
    %JAVA_FLAGS% ^
    -Dmultiverse.loggername=login_manager ^
    :: 20130428 CobaltBlues * Adding patch so worldname is passed to jar modules
    -Dmultiverse.worldname=%MV_WORLDNAME% ^
    multiverse.server.engine.Engine ^
    %CMDLINE_PROPS% ^
    -i %MV_COMMON%\mvmessages.py ^
    -i %MV_WORLD%\worldmessages.py ^
    -t %MV_COMMON%\typenumbers.txt ^
    login_manager.py ^
    %MV_COMMON%\login_manager.py ^
    %MV_COMMON%\character_factory.py ^
    %MV_WORLD%\character_factory.py ^
    %MV_WORLD%\extensions_login.py

echo Starting proxy server
START /B java  ^
    %JAVA_FLAGS% ^
    -Dmultiverse.loggername=proxy ^
    -Dmultiverse.agenttype=proxy ^
    :: 20130428 CobaltBlues * Adding patch so worldname is passed to jar modules
    -Dmultiverse.worldname=%MV_WORLDNAME% ^
    multiverse.server.engine.Engine ^
    %CMDLINE_PROPS% ^
    -i proxy.py ^
    -i %MV_COMMON%\events.py ^
    -i %MV_COMMON%\mvmessages.py ^
    -i %MV_WORLD%\worldmessages.py ^
    -t %MV_COMMON%\typenumbers.txt ^
    %MV_COMMON%\proxy.py ^
    %MV_COMMON%\global_props.py ^
    %MV_WORLD%\global_props.py ^
    %MV_WORLD%\extensions_proxy.py

echo Starting mob server
START /B java ^
    %JAVA_FLAGS% ^
    -Dmultiverse.loggername=mobserver ^
    :: 20130428 CobaltBlues * Adding patch so worldname is passed to jar modules
    -Dmultiverse.worldname=%MV_WORLDNAME% ^
    multiverse.server.engine.Engine ^
    %CMDLINE_PROPS% ^
    -i mobserver_local.py ^
    -i %MV_COMMON%\mvmessages.py ^
    -i %MV_WORLD%\worldmessages.py ^
    -t %MV_COMMON%\typenumbers.txt ^
    %MV_COMMON%\global_props.py ^
    %MV_WORLD%\global_props.py ^
    %MV_COMMON%\mobserver_init.py ^
    %MV_WORLD%\mobserver_init.py ^
    %MV_COMMON%\questplugin.py ^
    %MV_COMMON%\trainerplugin.py ^
    %MV_COMMON%\mobserver.py ^
    %MV_WORLD%\mobserver.py ^
    %MV_WORLD%\extensions_mobserver.py

echo Starting voice server
START /B java ^
    %JAVA_FLAGS% ^
    -Dmultiverse.loggername=voiceserver ^
    :: 20130428 CobaltBlues * Adding patch so worldname is passed to jar modules
    -Dmultiverse.worldname=%MV_WORLDNAME% ^
    multiverse.server.engine.Engine ^
    %CMDLINE_PROPS% ^
    -i %MV_COMMON%\mvmessages.py ^
    -i %MV_WORLD%\worldmessages.py ^
    -t %MV_COMMON%\typenumbers.txt ^
    %MV_COMMON%\voice.py ^
    %MV_WORLD%\voice.py 
     
echo Wait for finished initializing msg...
 
