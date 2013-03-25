#!/bin/bash

MV_HOME=${MV_HOME:-"../.."}
echo MV_HOME is ${MV_HOME}
MV_BIN=${MV_HOME}/bin
DEFAULT_MV_PROPERTYFILE=${DEFAULT_MV_PROPERTYFILE:-"multiverse.properties"}
MV_PROPERTYFILE=${MV_PROPERTYFILE:-"${MV_BIN}/${DEFAULT_MV_PROPERTYFILE}"}

RHINO=${RHINO:-"${MV_HOME}/other/rhino1_5R5/js.jar"}
GETOPT=${GETOPT:-"${MV_HOME}/other/java-getopt-1.0.11.jar"}
JYTHON=${JYTHON:-"${MV_HOME}/other/jython_2_1.jar"}
LOG4J=${LOG4J:-"${MV_HOME}/other/log4j-1.2.14.jar"}
BCEL=${BCEL:-"${MV_HOME}/other/bcel-5.2.jar"}
MV_JAR=${MV_JAR:-"${MV_HOME}/dist/lib/multiverse.jar"}
#MV_JAR=${MV_JAR:-"${MV_HOME}/build"}
MARS_JAR=${MARS_JAR:-"${MV_HOME}/dist/lib/mars.jar"}
#MARS_JAR=${MARS_JAR:-"${MV_HOME}/build"}

if [ $(uname -o) == "Cygwin" ]; then
    MV_CLASSPATH="$RHINO;$JDBC;$MV_JAR;$MARS_JAR;$GETOPT;$JYTHON;$LOG4J;$BCEL."
else
    MV_CLASSPATH="${RHINO}:${JDBC}:${MV_JAR}:${MARS_JAR}:${GETOPT}:${JYTHON}:${LOG4J}:${BCEL}:."
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
    shift
    arg=`expr $arg + 1`
done

echo start $start
echo count $count
echo seconds $seconds
echo FLAGS: "$@"

# Use SimpleClient plus start as the loggename, so different
# SimpleClients' log names don't collide
logname=SimpleClient$start

CHAR_PROPS="-P model=casual06_f_mediumpoly.mesh,casual07_f_mediumpoly.mesh,casual13_f_mediumpoly.mesh,casual15_f_mediumpoly.mesh,casual19_f_mediumpoly.mesh,casual21_f_mediumpoly.mesh,business04_f_mediumpoly.mesh,sportive01_f_mediumpoly.mesh,sportive02_f_mediumpoly.mesh,sportive05_f_mediumpoly.mesh,sportive07_f_mediumpoly.mesh,casual03_m_mediumpoly.mesh,casual04_m_mediumpoly.mesh,casual07_m_mediumpoly.mesh,casual10_m_mediumpoly.mesh,casual16_m_mediumpoly.mesh,casual21_m_mediumpoly.mesh,business03_m_mediumpoly.mesh,business05_m_mediumpoly.mesh,sportive01_m_mediumpoly.mesh,sportive09_m_mediumpoly.mesh"

# runs the simple client to connect to the local server
exec java -client -Xmx128m -cp ${MV_CLASSPATH} -Dmultiverse.propertyfile=${MV_COMMON}/simpleclient.props -Dmultiverse.loggername=$logname multiverse.server.marshalling.Trampoline multiverse.simpleclient.SimpleClient -e ${MV_COMMON}/simpleclient.props -a $start --count $count -f ./simpleclient.txt --seconds-between $seconds -s friendworldplayerclient.py $CHAR_PROPS "$@" &


