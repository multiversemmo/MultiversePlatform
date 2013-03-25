#!/bin/bash

MV_HOME=${MV_HOME:-"../../../../"}

RHINO=${RHINO:-"${MV_HOME}/other/rhino1_5R5/js.jar"}
GETOPT=${GETOPT:-"${MV_HOME}/other/java-getopt-1.0.11.jar"}
JYTHON=${JYTHON:-"${MV_HOME}/other/jython_2_1.jar"}
LOG4J=${LOG4J:-"${MV_HOME}/other/log4j-1.2.14.jar"}
BCEL=${BCEL:-"${MV_HOME}/other/bcel-5.2.jar"}
#MV_JAR=${MV_JAR:-"${MV_HOME}/dist/lib/multiverse.jar"}
MV_JAR=${MV_JAR:-"${MV_HOME}/build"}
#MARS_JAR=${MARS_JAR:-"${MV_HOME}/dist/lib/mars.jar"}
MARS_JAR=${MARS_JAR:-"${MV_HOME}/build"}

TESTHARNESS="."

rm "${TESTHARNESS}"/*.out*

if [ $(uname -o) == "Cygwin" ]; then
    MV_CLASSPATH="$RHINO;$JDBC;$MV_JAR;$MARS_JAR;$GETOPT;$JYTHON;$LOG4J;$BCEL;."
else
    MV_CLASSPATH="${RHINO}:${JDBC}:${MV_JAR}:${MARS_JAR}:${GETOPT}:${JYTHON}:${LOG4J}:${BCEL}:."
fi

echo $MV_CLASSPATH

exec java -cp ${MV_CLASSPATH} -Djava.system.class.loader=multiverse.server.marshalling.MarshallingClassLoader -Dmultiverse.logs=${TESTHARNESS} -Dmultiverse.loggername=testharness -Dmultiverse.propertyfile=testharness.properties multiverse.server.marshalling.Trampoline multiverse.tests.marshallingtests.TestHarness -m ${TESTHARNESS}/testharness.txt
