#!/bin/sh
(shopt -s igncr) 2>/dev/null && shopt -s igncr; # Workaround Cygwin line-ending issue

# This batch file runs the Multiverse sever processes on Linux in bash shell or on Windows/Cygwin
# You must have installed Java, a database, JDBC driver, and the Multiverse servers
# Copyright 2007 The Multiverse Network, Inc.
# Thanks to Judd-MGT for contributions.

# Optional: Set MV_HOME env. variable to be able to run this script from an arbitrary directory.
# Otherwise, this script assumes it is being run from MV_HOME/bin working directory
# NOTE: Doesn't work if you set MV_HOME.
export MV_HOME=${MV_HOME:-".."}

# Optional: Set DEFAULT_MV_PROPERTYFILE env. variable to set the default property file, otherwise use multiverse.properties
# when no property file is specified as final argument on command line.
DEFAULT_MV_PROPERTYFILE=${DEFAULT_MV_PROPERTYFILE:-"master.properties"}

### Set to true to enable JMX management and monitoring (either here or in env. variable.
ENABLE_MGMT=${ENABLE_MGMT:-"false"}

function kill_process () {
    if [ $verbose -gt 0 ]; then
        echo -en "stopping $1 "
    fi
    kill $2 > /dev/null 2>&1
    result=$?
    if [ $verbose -gt 0 ]; then
        if [ $result = 0 ]; then
            echo STOPPED
        else
            echo NOT RUNNING
        fi
    fi
}

function check_process () {
    ps -e ho pid | grep $1 > /dev/null 2>&1
    result=$?
    if [ $result = 1 ]; then
        echo FAILED
    else
        echo SUCCESS
    fi
}

function status_process () {
    if [ x$2 == x ]; then
	return
    fi
    ps -e ho pid | grep $2 > /dev/null 2>&1
    result=$?
    if [ $result = 0 ]; then
        echo -e "$1" RUNNING
    else
        echo -e "$1" NOT RUNNING
    fi
}

function archive_log_dir () {
    rm -rf "${MV_LOGS}.old"
    mv "${MV_LOGS}" "${MV_LOGS}.old"
}

function start_server () {

    if [ X$ARCHIVE_LOG_DIR = X"true" ]; then
	if [ -d "${MV_LOGS}" ]; then
	    archive_log_dir
	fi
    fi

    # Use these flags for profiling
    HPROF_FLAGS="-agentlib:hprof=heap=sites,depth=8"
    GCDETAILS_FLAGS="-XX:+PrintGC -XX:+PrintGCDetails"

    # Linux: Use strace with the following flags to monitor one of the
    # multiverse server processes.
    #    strace -f -e trace=\!futex,gettimeofday,clock_gettime java \

    echo "*** Starting master server ***"
    if [ ! -d "${MV_RUN}" ]
        then
        mkdir -p "${MV_RUN}"
    fi

    if [ ! -d "${MV_LOGS}" ]
        then
        mkdir -p "${MV_LOGS}"
    fi

    rm -f ${MV_RUN}/*.pid

    if [ $DELETE_LOGS_ON_STARTUP = "true" ]; then
        rm "${MV_LOGS}"/*.out*
    fi

    if $ENABLE_MGMT = "true"; then        
        echo "Enabling JMX mgmt & monitoring"
        JAVA_FLAGS="${JAVA_FLAGS} $JMX_FLAGS"
    fi        

    if [ $verbose -gt 0 ]; then
        echo MV_HOME is $MV_HOME
        if [ $USE_CLASS_FILES = "true" ]; then
            echo "Using .class files from the /build hierarchy"
        else
            echo "Using .jar files from the /dist hierarchy"
        fi
        echo Using property file $MV_PROPERTYFILE  
        echo Using log directory $MV_LOGS
        echo Using common directory $MV_COMMON, bin directory $MV_BIN     
        echo "JAVA_FLAGS=\"${JAVA_FLAGS}\""
    fi

    # Increase the file descriptor limit up to the hard limit
    # Linux: Use /etc/security/limits.conf to set the hard limit
    ulimit -n hard

    if [ $verbose -gt 0 ]; then
        echo -en "Starting master server: \t"
    fi

    java ${JAVA_FLAGS} \
        -Dmultiverse.loggername=master \
        multiverse.server.engine.MasterServer \
	$CMDLINE_PROPS \
        $MV_BIN/master_server.py \
        &

    echo $! > "${MV_RUN}"/master.pid

    if [ $verbose -gt 0 ]; then
        check_process $(cat "${MV_RUN}"/master.pid)
    fi

    echo "Wait for finished initializing msg... "
}

function stop_server () {
    echo "*** Stopping master server ***"
    kill_process "master server  " $(cat "${MV_RUN}"/master.pid)
}

function status_server () {
    down=0
    status_process "master server  " $(cat "${MV_RUN}"/master.pid)  
    exit ${down}
}

function test_server () {
    echo "JAVA_FLAGS=\"${JAVA_FLAGS}\""
    java $JAVA_FLAGS multiverse.server.util.SecureTokenManager
    exit 0
    if [ $verbose -gt 0 ]; then
        java $JAVA_FLAGS multiverse.simpleclient.SimpleClient -e ${MV_COMMON}/simpleclient.props -s $MV_COMMON/simpleclient.py --exit-after-login
    else
        java $JAVA_FLAGS multiverse.simpleclient.SimpleClient -e ${MV_COMMON}/simpleclient.props -s ${MV_COMMON}/simpleclient.py --exit-after-login > /dev/null 2>&1
    fi
    result=$?
    if [ $verbose -gt 0 ]; then
        if [ $result == 0 ]; then
            echo "login test: PASS"
        else
            echo "login test: FAIL"
        fi
    fi
    exit ${result}
}

function import_property_file () {
    for file
    do
        if [ -f $file ]; then
            files="$files $file"
        fi
    done
    if [ -n "$files" ]; then
        awk -f "$MV_BIN/prop2sh.awk" $files > "$MV_BIN/_javaprops_"
        . "$MV_BIN/_javaprops_"
        rm -f "$MV_BIN/_javaprops_"
    fi
}

verbose=0
while getopts "hvw:p:CMP:Aa" arg; do
    case "$arg" in
        h)
            echo "$0: usage: $0 [-hvCM] [-p propertyfilename] (start|stop|status|test)"
            ;;
        v)
            let verbose++
            ;;
	p)
	    MV_PROPERTYFILE=$OPTARG
	    ;;
	C)
	    JVM_FLAG=-client
	    ;;
	M)
	    ENABLE_MGMT=true
	    JMX_FLAGS="-Dcom.sun.management.jmxremote"
	    ;;
	P)
	    CMDLINE_PROPS="$CMDLINE_PROPS -P$OPTARG"
	    ;;
	A)
	    AGGRESIVE="-XX:CompileThreshold=200 -Xnoclassgc -XX:+RelaxAccessControlCheck"
	    ;;
	a)
	    ARCHIVE_LOG_DIR=true
	    ;;
	esac
done
shift $((OPTIND-1))

if [ $(uname -o) = "Cygwin" ]; then
    MV_HOME_UNIX=$(cygpath -u "${MV_HOME}")
else
    MV_HOME_UNIX="$MV_HOME"
fi

# where the local startup configs are stored, such as the port number
# and log level
MV_BIN=${MV_BIN:-"${MV_HOME}/bin"}

# where common config files are stored, such as plugin logic
MV_COMMON=${MV_COMMON:-"${MV_HOME}/config/common"}

MV_PROPERTYFILE=${MV_PROPERTYFILE:-"${MV_BIN}/${DEFAULT_MV_PROPERTYFILE}"}

import_property_file $MV_PROPERTYFILE

if [ -z "$ARCHIVE_LOG_DIR" -a -n "$multiverse_archive_logs_on_startup" ] ; then
    ARCHIVE_LOG_DIR=$multiverse_archive_logs_on_startup
fi

# Determine if we should use .class files from the build hierarchy,
# or .jar files from the dist hierarchy.  To run the property getter
# before MV_JAR is set, we always use the dist version of the property
# getter.
USE_CLASS_FILES=${USE_CLASS_FILES:-$(java -cp ${MV_HOME}/dist/lib/multiverse.jar -Dmultiverse.propertyfile=${MV_PROPERTYFILE} multiverse.scripts.PropertyGetter multiverse.use_class_files)}

RHINO=${RHINO:-"${MV_HOME}/other/rhino1_5R5/js.jar"}
GETOPT=${GETOPT:-"${MV_HOME}/other/java-getopt-1.0.11.jar"}
JYTHON=${JYTHON:-"${MV_HOME}/other/jython.jar"}
LOG4J=${LOG4J:-"${MV_HOME}/other/log4j-1.2.14.jar"}
BCEL=${BCEL:-"${MV_HOME}/other/bcel-5.2.jar"}

if  [ $USE_CLASS_FILES = "true" ]; then
    MV_JAR=${MV_JAR:-"${MV_HOME}/build"}
    MARS_JAR=${MARS_JAR:-"${MV_HOME}/build"}
    INJECTED_JAR=${INJECTED_JAR:-"${MV_HOME}/inject"}
else
    MV_JAR=${MV_JAR:-"${MV_HOME}/dist/lib/multiverse.jar"}
    MARS_JAR=${MARS_JAR:-"${MV_HOME}/dist/lib/mars.jar"}
    INJECTED_JAR=${INJECTED_JAR:-"${MV_HOME}/dist/lib/injected.jar"}
fi

JDBC=${JDBC:-$(java -cp $MV_JAR -Dmultiverse.propertyfile=${MV_PROPERTYFILE} multiverse.scripts.PropertyGetter multiverse.jdbcJarPath)}

if [ $(uname -o) == "Cygwin" ]; then
    export PATH=$(cygpath "$JAVA_HOME"/bin):.:$PATH
    MV_CLASSPATH="$RHINO;$JDBC;$INJECTED_JAR;$MV_JAR;$MARS_JAR;$GETOPT;$JYTHON;$LOG4J;$BCEL;."
else
    MV_CLASSPATH="${RHINO}:${JDBC}:${INJECTED_JAR}:${MV_JAR}:${MARS_JAR}:${GETOPT}:${JYTHON}:${LOG4J}:${BCEL}:."
fi
        
# HotSpot tracking flags: -XX:+PrintCompilation -XX:+CITime
JVM_FLAG="${JVM_FLAG:-"-server"} $AGGRESIVE"
JAVA_FLAGS="-cp ${MV_CLASSPATH} -Dmultiverse.propertyfile=${MV_PROPERTYFILE} ${JAVA_FLAGS}"
JAVA_FLAGS="${JVM_FLAG} ${JAVA_FLAGS}"

if [ X"$MV_HOSTNAME" != "X" ]; then
    CMDLINE_PROPS="$CMDLINE_PROPS -Pmultiverse.hostname=${MV_HOSTNAME}"
fi

# This is in local OS format
MV_LOGS=${MV_LOGS:-"${MV_HOME}/logs/master"}
DELETE_LOGS_ON_STARTUP=${MV_DELETE_LOGS_ON_STARTUP:-$(java $JAVA_FLAGS multiverse.scripts.PropertyGetter multiverse.delete_logs_on_startup)}

# This should always be in "unix" format
if [ $(uname -o) = "Cygwin" ]; then
    MV_RUN=${MV_RUN:-$(cygpath -w ${MV_BIN}/run/master)}
else
    MV_RUN=${MV_RUN:-${MV_BIN}/run/master}
fi

JAVA_FLAGS="${JAVA_FLAGS} -Dmultiverse.logs=${MV_LOGS}"

case "$1" in

    start)
        start_server
        ;;

    stop)
        stop_server
        ;;

    status)
        status_server
        ;;

    restart)
        stop_server
        start_server
        ;;

    test)
        test_server
        ;;
esac 
 
