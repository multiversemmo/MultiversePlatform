:: This command file creates the \inject directory containing the 
:: class files modified by byte code insertion of methods to marshal
:: and unmarshal instances of the classes.

@ echo off

echo Starting batch injection of marshalling methods ...

:: Set DEFAULT_MV_PROPERTYFILE if you want to use a different defult property file
if defined DEFAULT_MV_PROPERTYFILE (
   echo DEFAULT_MV_PROPERTYFILE is %DEFAULT_MV_PROPERTYFILE%
) else (
   echo DEFAULT_MV_PROPERTYFILE is not defined using multiverse.properties
   set DEFAULT_MV_PROPERTYFILE=multiverse.properties
)
set PROPFILE=%1
if %1x==x (
  set PROPFILE=%DEFAULT_MV_PROPERTYFILE%
)
echo Using properties file %PROPFILE%

:: Check that script is being run from mv_home\bin
if not defined MV_HOME (
  echo MV_HOME is not defined, using relative paths
  if exist .\performinjection.bat (
    set MV_HOME=..
  ) else (
    echo Batch script must be run from MV_HOME\bin directory!
  )
) 

set MV_JAR=%MV_HOME%\build

set MV_LOGS=%MV_HOME%\logs\inject
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
  del /Q %MV_LOGS%\*.out*
)

set MV_COMMON=%MV_HOME%\config\common

if %1x==x (
	:: Set value of MV_WORLDNAME from multiverse.worldname in property file
	java -cp %MV_JAR% -Dmultiverse.propertyfile=%PROPFILE% -Dwin_env_var=MV_WORLDNAME multiverse.scripts.PropertyGetter multiverse.worldname > tmp.bat
	call tmp.bat
	del tmp.bat
) else (
    set MV_WORLDNAME=%1
)

set MV_WORLD=%MV_HOME%\config\%MV_WORLDNAME%

set LOG4J=%MV_HOME%\other\log4j-1.2.14.jar
set BCEL=%MV_HOME%\other\bcel-5.2.jar
set EXT_JAR=%MV_HOME%\dist\lib\%MV_WORLDNAME%.jar
set MV_CLASSPATH=%MV_JAR%;%EXT_JAR%;%LOG4J%;%BCEL%
set JAVA_FLAGS=%JAVA_FLAGS% -cp %MV_CLASSPATH% -Dmultiverse.logs=%MV_LOGS%

rmdir /S /Q %MV_HOME%\inject\multiverse

java -Dmultiverse.log_level=0 %JAVA_FLAGS% -ea multiverse.server.marshalling.InjectClassFiles -m %MV_COMMON%\mvmarshallers.txt -m %MV_WORLD%\worldmarshallers.txt -t %MV_COMMON%\typenumbers.txt -i %MV_HOME%\build -o %MV_HOME%\inject\

echo Finished batch injection of marshalling methods


