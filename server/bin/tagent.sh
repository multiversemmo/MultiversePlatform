#!/bin/sh

MAIN_CLASS=multiverse.management.TransientAgent

if [ "X$MV_HOME" = "X" ]; then
    export MV_HOME=`dirname $0`/..
fi
MV_COMMON="$MV_HOME/config/common"

MVJAR="$MV_HOME/dist/lib/multiverse.jar"
MARSJAR="$MV_HOME/dist/lib/mars.jar"
GETOPT="$MV_HOME/other/java-getopt-1.0.11.jar"
LOG4J="$MV_HOME/other/log4j-1.2.14.jar"
BCEL="$MV_HOME/other/bcel-5.2.jar"
INJECTED_JAR=${INJECTED_JAR:-"${MV_HOME}/dist/lib/injected.jar"}

if [ $(uname -o) == "Cygwin" ]; then
    MV_CLASSPATH="$INJECTED_JAR;$MVJAR;$MARSJAR;$BCEL;$GETOPT;$LOG4J;$JAVA_HOME/lib/tools.jar"
else
    MV_CLASSPATH="$INJECTED_JAR:$MVJAR:$MARSJAR:$BCEL:$GETOPT:$LOG4J:$JAVA_HOME/lib/tools.jar"
fi

echo $MV_CLASSPATH
DISABLE_LOG="-Dmultiverse.disable_logs=true"

MARSHALL_LIST="-t ${MV_COMMON}/typenumbers.txt"

JAVA_PROPS="$DISABLE_LOG"

if [ "X$JAVA_HOME" = "X" ]; then
    java -cp $MV_CLASSPATH $JAVA_PROPS $MAIN_CLASS $MARSHALL_LIST "$@"
else
    "$JAVA_HOME/bin/java" -cp "$MV_CLASSPATH" $JAVA_PROPS $MAIN_CLASS $MARSHALL_LIST "$@"
fi


