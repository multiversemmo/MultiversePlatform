#!/bin/bash

MV_HOME=${MV_HOME:-"../../../bin"}

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

# runs the simple client to connect to the local server
java -agentlib:hprof=cpu=samples,depth=20 -cp ${MV_CLASSPATH} multiverse.networktests.TestDatagramServer $1 $2
