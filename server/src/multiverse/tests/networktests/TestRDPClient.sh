#!/bin/bash

MV_HOME=${MV_HOME:-"../../../bin"}
MV_BIN=${MV_HOME}/bin
DEFAULT_MV_PROPERTYFILE=${DEFAULT_MV_PROPERTYFILE:-"multiverse.properties"}
MV_PROPERTYFILE=${MV_PROPERTYFILE:-"${MV_BIN}/${DEFAULT_MV_PROPERTYFILE}"}

RHINO=${RHINO:-"${MV_HOME}/other/rhino1_5R5/js.jar"}
GETOPT=${GETOPT:-"${MV_HOME}/other/java-getopt-1.0.11.jar"}
JYTHON=${JYTHON:-"${MV_HOME}/other/jython_2_1.jar"}
MV_JAR=${MV_JAR:-"${MV_HOME}/dist/lib/multiverse.jar"}
MARS_JAR=${MARS_JAR:-"${MV_HOME}/dist/lib/mars.jar"}

if [ $(uname -o) == "Cygwin" ]; then
    MV_CLASSPATH="$RHINO;$JDBC;$MV_JAR;$MARS_JAR;$GETOPT;$JYTHON;."
else
    MV_CLASSPATH="${RHINO}:${JDBC}:${MV_JAR}:${MARS_JAR}:${GETOPT}:${JYTHON}:."
fi

echo $MV_CLASSPATH

# usage: java TestRDP hostname remotePort localPort loglevel <messageCount>
java -cp ${MV_CLASSPATH} -Dmultiverse.propertyfile=${MV_PROPERTYFILE} multiverse.networktests.TestRDPClient 127.0.0.1 $1 $2 100000000 $3
