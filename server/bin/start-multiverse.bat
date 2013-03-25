@ echo off
:: This batch file runs the Multiverse sever processes on Windows
:: You must have installed Java, a database, JDBC driver, and the Multiverse servers
:: Copyright 2007 The Multiverse Network, Inc.

:: Set DEFAULT_MV_PROPERTYFILE if you want to use a different defult property file
if defined DEFAULT_MV_PROPERTYFILE (
   echo DEFAULT_MV_PROPERTYFILE is %DEFAULT_MV_PROPERTYFILE%
) else (
   echo DEFAULT_MV_PROPERTYFILE is not defined using multiverse.properties
   set DEFAULT_MV_PROPERTYFILE=multiverse.properties
)

:: Set to true to enable JMX management and monitoring
if not defined ENABLE_MGMT set ENABLE_MGMT=false

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
echo ENABLE_MGMT is %ENABLE_MGMT%

:: Change to "server" to use the server Java VM
set JVM_TYPE=client
set JVM_HEAP_FLAGS=-Xms32m -Xmx256m

set PROPFILE=%1
if %1x==x (
  set PROPFILE=%DEFAULT_MV_PROPERTYFILE%
)
echo Using properties file %PROPFILE%

if not defined MV_JAR (
  set MV_JAR=%MV_HOME%\dist\lib\multiverse.jar
)
if not defined MARS_JAR (
  set MARS_JAR=%MV_HOME%\dist\lib\mars.jar
)
if not defined INJECTED_JAR (
  set INJECTED_JAR=%MV_HOME%\dist\lib\injected.jar
)

:: Set value of MV_WORLDNAME from multiverse.worldname in property file
java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=MV_WORLDNAME multiverse.scripts.PropertyGetter multiverse.worldname > tmp.bat
call tmp.bat
del tmp.bat

set JYTHON=%MV_HOME%\other\jython.jar
set RHINO=%MV_HOME%\other\rhino1_5R5\js.jar
set GETOPT=%MV_HOME%\other\java-getopt-1.0.11.jar
set LOG4J=%MV_HOME%\other\log4j-1.2.14.jar
set BCEL=%MV_HOME%\other\bcel-5.2.jar
set EXT_JAR=%MV_HOME%\dist\lib\%MV_WORLDNAME%.jar

:: Get path to JDBC JAR file from property file, unless set in env. var.
if not defined JDBC (
  java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=JDBC multiverse.scripts.PropertyGetter multiverse.jdbcJarPath > tmp.bat
  call tmp.bat
  del tmp.bat
)
echo JDBC is %JDBC%

set MV_CLASSPATH=%INJECTED_JAR%;%MV_JAR%;%MARS_JAR%;%EXT_JAR%;%RHINO%;%GETOPT%;%JYTHON%;%JDBC%;%LOG4J%;%BCEL%

set CMDLINE_PROPS=
if defined MV_HOSTNAME (
  set CMDLINE_PROPS=-Pmultiverse.hostname=%MV_HOSTNAME%
)

set JAVA_FLAGS=-%JVM_TYPE% %JVM_HEAP_FLAGS% -cp "%MV_CLASSPATH%" -Dmultiverse.propertyfile=%PROPFILE%

:: Set value of MVW_FILENAME from multiverse.mvwfile in property file if it is there, if not set based on MV_WORLDNAME
java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=MVW_FILENAME multiverse.scripts.PropertyGetter multiverse.mvwfile > tmp.bat
call tmp.bat
del tmp.bat

if %MVW_FILENAME%==null (
    set MVW_FILENAME="%MV_HOME%\config\%MV_WORLDNAME%\%MV_WORLDNAME%.mvw"
)
echo Using world file %MVW_FILENAME%

set MV_LOGS=%MV_HOME%\logs\%MV_WORLDNAME%
set JAVA_FLAGS=%JAVA_FLAGS% -Dmultiverse.logs=%MV_LOGS%

if not exist %MV_LOGS% (
  mkdir %MV_LOGS%
)

if not defined DELETE_LOGS_ON_STARTUP (
  java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=DELETE_LOGS_ON_STARTUP multiverse.scripts.PropertyGetter multiverse.delete_logs_on_startup > tmp.bat
  call tmp.bat
  del tmp.bat
)

if %DELETE_LOGS_ON_STARTUP%==true (
  echo Deleting existing log files
  del %MV_LOGS%\*.out*
)

if not exist run (
  echo Creating run directory
  mkdir run
)

if not exist run\%MV_WORLDNAME% (
  echo Creating run\%MV_WORLDNAME% directory
  mkdir run\%MV_WORLDNAME%
)
del run\%MV_WORLDNAME%\*.bat

set JAVA_FLAGS=-Dmultiverse.rundir=run\%MV_WORLDNAME% %JAVA_FLAGS%

if %ENABLE_MGMT%==true (
  echo Enabling JMX mgmt and monitoring
  set JAVA_FLAGS=-Dcom.sun.management.jmxremote %JAVA_FLAGS%
) 

set MV_COMMON=%MV_HOME%\config\common
set MV_WORLD=%MV_HOME%\config\%MV_WORLDNAME%
set AGENT_NAMES=-a combat -a wmgr_1 -a mobserver -a objmgr -a login_manager -a proxy_1 -a instance -a voiceserver
set PLUGIN_TYPES=-p Login,1 -p Proxy,1 -p ObjectManager,1 -p WorldManager,1 -p Inventory,1 -p MobManager,1 -p Quest,1 -p Instance,1 -p Voice,1 -p Trainer,1 -p Group,1 -p Combat,1 -p ClassAbility,1 -p Domain,1

echo Using world script directory %MV_WORLD%
echo Using log directory %MV_LOGS%
echo Using common directory %MV_COMMON%
echo Java Flags are: %JAVA_FLAGS%

echo Starting message domain server
@ echo on
START /B java  %JAVA_FLAGS% ^
    -Dmultiverse.loggername=domain ^
    multiverse.msgsys.DomainServer ^
    %CMDLINE_PROPS% ^
    -t %MV_COMMON%\typenumbers.txt ^
    %AGENT_NAMES% ^
    %PLUGIN_TYPES%

@ echo off
echo Starting world manager
START /B java ^
    %JAVA_FLAGS% ^
    -Dmultiverse.agenttype=wmgr ^
    -Dmultiverse.loggername=wmgr_1 ^
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
    -Dmultiverse.loggername=proxy_1 ^
    -Dmultiverse.agenttype=proxy ^
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
    multiverse.server.engine.Engine ^
    %CMDLINE_PROPS% ^
    -i %MV_COMMON%\mvmessages.py ^
    -i %MV_WORLD%\worldmessages.py ^
    -t %MV_COMMON%\typenumbers.txt ^
    %MV_COMMON%\voice.py ^
    %MV_WORLD%\voice.py 
     
echo Wait for finished initializing msg... 
