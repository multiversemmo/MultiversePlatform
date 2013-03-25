#!/bin/bash

MV_HOME=${MV_HOME:-".."}

RHINO=${RHINO:-"${MV_HOME}/other/rhino1_5R5/js.jar"}
GETOPT=${GETOPT:-"${MV_HOME}/other/java-getopt-1.0.11.jar"}
JYTHON=${JYTHON:-"${MV_HOME}/other/jython_2_1.jar"}
INJECTED_JAR=${INJECTED_JAR:-"${MV_HOME}/dist/lib/injected.jar"}
MV_JAR=${MV_JAR:-"${MV_HOME}/dist/lib/multiverse.jar"}
MARS_JAR=${MARS_JAR:-"${MV_HOME}/dist/lib/mars.jar"}
LOG4J=${LOG4J:-"${MV_HOME}/other/log4j-1.2.14.jar"}
BCEL=${BCEL:-"${MV_HOME}/other/bcel-5.2.jar"}


if [ $(uname -o) == "Cygwin" ]; then
    MV_CLASSPATH="$RHINO;$JDBC;$INJECTED_JAR;$MV_JAR;$MARS_JAR;$EXT_JAR;$GETOPT;$JYTHON;$LOG4J;$BCEL;."
else
    MV_CLASSPATH="${RHINO}:${JDBC}:${INJECTED_JAR}:${MV_JAR}:${MARS_JAR}:${EXT_JAR}:${GETOPT}:${JYTHON}:${LOG4J}:${BCEL}:."
fi

echo $MV_CLASSPATH

# runs the simple client to connect to the local server
exec java -client -cp ${MV_CLASSPATH} -Dmultiverse.propertyfile=../config/common/simpleclient.props multiverse.simpleclient.SimpleClient -t ${MV_COMMON}/typenumbers.txt -e ../config/common/simpleclient.props -s ../config/common/simpleclient.py "$@"
