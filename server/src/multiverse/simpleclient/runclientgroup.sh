#!/bin/bash

MV_HOME=${MV_HOME:-"../../.."}
echo #MV_HOME
MV_BIN=${MV_HOME}/bin
DEFAULT_MV_PROPERTYFILE=${DEFAULT_MV_PROPERTYFILE:-"multiverse.properties"}
MV_PROPERTYFILE=${MV_PROPERTYFILE:-"${MV_BIN}/${DEFAULT_MV_PROPERTYFILE}"}

RHINO=${RHINO:-"${MV_HOME}/other/rhino1_5R5/js.jar"}
GETOPT=${GETOPT:-"${MV_HOME}/other/java-getopt-1.0.11.jar"}
JYTHON=${JYTHON:-"${MV_HOME}/other/jython.jar"}
LOG4J=${LOG4J:-"${MV_HOME}/other/log4j-1.2.14.jar"}
BCEL=${BCEL:-"${MV_HOME}/other/bcel-5.2.jar"}

# Determine if we should use .class files from the build hierarchy,
# or .jar files from the dist hierarchy.  To run the property getter
# before MV_JAR is set, we always use the dist version of the property
# getter.
USE_CLASS_FILES=${USE_CLASS_FILES:-$(java -cp ${MV_HOME}/dist/lib/multiverse.jar -Dmultiverse.propertyfile=${MV_PROPERTYFILE} multiverse.scripts.PropertyGetter multiverse.use_class_files)}

if  [ $USE_CLASS_FILES = "true" ]; then
	MV_JAR=${MV_JAR:-"${MV_HOME}/build"}
	MARS_JAR=${MARS_JAR:-"${MV_HOME}/build"}
	INJECTED_JAR=${INJECTED_JAR:-"${MV_HOME}/inject"}
else
	MV_JAR=${MV_JAR:-"${MV_HOME}/dist/lib/multiverse.jar"}
	MARS_JAR=${MARS_JAR:-"${MV_HOME}/dist/lib/mars.jar"}
	INJECTED_JAR=${INJECTED_JAR:-"${MV_HOME}/dist/lib/injected.jar"}
fi

if [ $(uname -o) == "Cygwin" ]; then
    MV_CLASSPATH="$RHINO;$JDBC;$INJECTED_JAR;$MV_JAR;$MARS_JAR;$GETOPT;$JYTHON;$LOG4J;$BCEL."
else
    MV_CLASSPATH="${RHINO}:${JDBC}:${INJECTED_JAR}:${MV_JAR}:${MARS_JAR}:${GETOPT}:${JYTHON}:${LOG4J}:${BCEL}:."
fi

MV_COMMON=${MV_COMMON:-"${MV_HOME}/config/common"}

echo $MV_CLASSPATH
mkdir -p logs

echo "Run" > ./simpleclient.txt

echo MV_COMMON is "${MV_COMMON}"

mkdir -p logs

start=1
count=1
seconds=5
group=1

arg=1
while [ $# != 0 ]; do
    flag="$1"
    case "$flag" in
	-*) break
	    ;;
    esac
	if [ $arg = 1 ]; then
	    start=$flag
	fi
	if [ $arg = 2 ]; then
	    count=$flag
	fi
	if [ $arg = 3 ]; then
	    seconds=$flag
	fi
        if [ $arg = 4 ]; then
            group=$flag
        fi
    shift
    arg=`expr $arg + 1`
done

echo start $start
echo count $count
echo seconds $seconds
echo group $group
echo FLAGS: "$@"

# Use SimpleClient plus start as the loggename, so different
# SimpleClients' log names don't collide
logname=SimpleClient$start

# runs the simple client to connect to the local server
exec java -client -Xmx128m -cp ${MV_CLASSPATH} -Dmultiverse.propertyfile=${MV_COMMON}/simpleclient.props -Dmultiverse.loggername=$logname multiverse.simpleclient.SimpleClient -t ${MV_COMMON}/typenumbers.txt -e ${MV_COMMON}/simpleclient.props -a ${start}${group}00 --count $count -f ./simpleclient.txt --seconds-between $seconds -s testclientgroups/group${group}.py "$@" &


