#!/bin/sh
(shopt -s igncr) 2>/dev/null && shopt -s igncr; # Workaround Cygwin line-ending issue

# This batch file runs the Multiverse sever processes on Linux in bash shell or on Windows/Cygwin
# You must have installed Java, a database, JDBC driver, and the Multiverse servers
# Copyright 2007 The Multiverse Network, Inc.
# Thanks to Judd-MGT for contributions.

# Optional: Set MV_HOME env. variable to be able to run this script from an arbitrary directory.
# Otherwise, this script assumes it is being run from MV_HOME/bin working directory
MV_HOME=${MV_HOME:-".."}

# Optional: Set DEFAULT_MV_PROPERTYFILE env. variable to set the default property file, otherwise use multiverse.properties
# when no property file is specified as final argument on command line.
DEFAULT_MV_PROPERTYFILE=${DEFAULT_MV_PROPERTYFILE:-"multiverse.properties"}

function kill_process () {
    if [ $verbose -gt 0 ]; then
        echo -en "stopping $1:    \t"
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
    ps aho pid | grep $2 > /dev/null 2>&1
    result=$?
    if [ $result = 1 ]; then
        echo FAILED
    else
        echo SUCCESS
    fi
}

function status_process () {
    ps aho pid | grep $2 > /dev/null 2>&1
    result=$?
    if [ $result = 1 ]; then
        echo -e $1":     \t NOT RUNNING"
    else
        echo -e $1":     \t RUNNING"
    fi
}

function start_server () {
	# Use these flags for profiling
        HPROF_FLAGS="-agentlib:hprof=heap=sites,depth=8"
	GCDETAILS_FLAGS="-XX:-PrintGC -XX:-PrintGCDetails"

	echo "*** Starting world $MV_WORLDNAME ***"
	if [ ! -d "${MV_RUN}" ]
	  then
		  mkdir -p "${MV_RUN}"
	fi

	if [ ! -d "${MV_LOGS}" ]
	  then
		  mkdir -p "${MV_LOGS}"
	fi

	if [ $DELETE_LOGS_ON_STARTUP = "true" ]; then
		rm "${MV_LOGS}"/*.out*
	fi

	if [ $verbose -gt 0 ]; then
		echo MV_HOME is $MV_HOME
		echo Using property file $MV_PROPERTYFILE  
		echo Using world file $MVW_FILENAME
		echo Using world script directory $MV_WORLD
		echo Using log directory $MV_LOGS
		echo Using common directory $MV_COMMON, bin directory $MV_BIN     
		echo Dual World Manager Flag = $DUALWMGRS
		echo "JAVA_FLAGS=\"${JAVA_FLAGS}\""
		echo -en "Starting message server: \t"
	fi

	JIT_FLAGS="-XX:+CITime -XX:+PrintCompilation"

	echo java \
		${JAVA_FLAGS} \
		-Dmultiverse.loggername=domain \
		multiverse.msgsys.DomainServer \
        -m "${MV_COMMON}"/typenumbers.txt "$@"
	java \
		${JAVA_FLAGS} \
		-Dmultiverse.loggername=domain \
		multiverse.msgsys.DomainServer \
        -t "${MV_COMMON}"/typenumbers.txt "$@"

	echo "pid" $! 

}

function stop_server () {
	echo "*** Stopping world $MV_WORLDNAME ***"
	kill_process "login server" $(cat "${MV_RUN}"/login_manager.pid)
	kill_process "animation server" $(cat "${MV_RUN}"/anim.pid)
	kill_process "combat server" $(cat "${MV_RUN}"/combat.pid)
	kill_process "object manager" $(cat "${MV_RUN}"/objmgr.pid)
	kill_process "world manager 1" $(cat "${MV_RUN}"/wmgr1.pid)
  if [ $DUALWMGRS -gt 0 ]; then
	    kill_process "world manager 2" $(cat "${MV_RUN}"/wmgr2.pid)
	fi
	kill_process "proxy server" $(cat "${MV_RUN}"/proxy.pid)
	kill_process "mob server" $(cat "${MV_RUN}"/mobserver.pid)
	kill_process "message server" $(cat "${MV_RUN}"/msgsvr.pid)
	kill_process "world reader" $(cat "${MV_RUN}"/worldreader.pid)
kill_process "startup plugin" $(cat "${MV_RUN}"/startup.pid)
}

function status_server () {
	down=0
	status_process "message server" $(cat "${MV_RUN}"/msgsvr.pid)  
	status_process "login server" $(cat "${MV_RUN}"/login_manager.pid) 
	status_process "animation server" $(cat "${MV_RUN}"/anim.pid) 
	status_process "combat server" $(cat "${MV_RUN}"/combat.pid)                
	status_process "object manager" $(cat "${MV_RUN}"/objmgr.pid)
	status_process "world manager 1" $(cat "${MV_RUN}"/wmgr1.pid)
  if [ $DUALWMGRS -gt 0 ]; then
	    status_process "world manager 2" $(cat "${MV_RUN}"/wmgr2.pid)
	fi	
	status_process "proxy server" $(cat "${MV_RUN}"/proxy.pid)
	status_process "world reader" $(cat "${MV_RUN}"/worldreader.pid)
	status_process "mob server" $(cat "${MV_RUN}"/mobserver.pid)
	exit ${down}
}

function test_server () {
	if [ $verbose -gt 0 ]; then
		java $JAVA_FLAGS multiverse.simpleclient.SimpleClient -e ${MV_COMMON}/simpleclient.props
	else
		java $JAVA_FLAGS multiverse.simpleclient.SimpleClient -e ${MV_COMMON}/simpleclient.props > /dev/null 2>&1
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

verbose=0
while getopts "hvw:p:" arg; do
    case "$arg" in
        h)
            echo "$0: usage: $0 [-hv] [-w worldname] [-p propertyfilename] (start|stop|status|test)"
            ;;
        v)
            let verbose++
            ;;
		w)
		    JAVA_FLAGS=-Dmultiverse.worldname=$OPTARG
			MV_WORLDNAME=$OPTARG
			;;
		p)
		    MV_PROPERTYFILE=$OPTARG
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

# Determine if we should use .class files from the build hierarchy,
# or .jar files from the dist hierarchy.  To run the property getter
# before MV_JAR is set, we always use the dist version of the property
# getter.
echo "Property File: " ${MV_PROPERTYFILE}
echo "MV_HOME: " ${MV_HOME}

USE_CLASS_FILES=${USE_CLASS_FILES:-$(java -cp ${MV_HOME}/dist/lib/multiverse.jar -Dmultiverse.propertyfile=${MV_PROPERTYFILE} multiverse.scripts.PropertyGetter multiverse.use_class_files)}

RHINO=${RHINO:-"${MV_HOME}/other/rhino1_5R5/js.jar"}
GETOPT=${GETOPT:-"${MV_HOME}/other/java-getopt-1.0.11.jar"}
JYTHON=${JYTHON:-"${MV_HOME}/other/jython_2_1.jar"}
LOG4J=${LOG4J:-"${MV_HOME}/other/log4j-1.2.14.jar"}
BCEL=${BCEL:-"${MV_HOME}/other/bcel-5.2.jar"}

if [ $USE_CLASS_FILES = "true" ]; then
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
        
JAVA_FLAGS="-cp ${MV_CLASSPATH} -Djava.system.class.loader=multiverse.server.marshalling.MarshallingClassLoader -Dmultiverse.propertyfile=${MV_PROPERTYFILE} ${JAVA_FLAGS}"

#Get world name from properties file, and construct path to world script dir if not set from env var.
MV_WORLDNAME=${MV_WORLDNAME:-mtest}
MV_WORLD=${MV_WORLD:-"$MV_HOME/config/$MV_WORLDNAME"}

# Get path to mvw file if set explicitly in multiverse.mvwfile, otherwise, construct path to mvwfile.
MVW_FILENAME=$(java $JAVA_FLAGS multiverse.scripts.PropertyGetter multiverse.mvwfile)

if [ "$MVW_FILENAME" = "null" ]; then
    MVW_FILENAME=$MV_HOME/config/$MV_WORLDNAME/$MV_WORLDNAME.mvw
fi

# This is in local OS format
MV_LOGS=${MV_LOGS:-"${MV_HOME}/logs/${MV_WORLDNAME}"}
DELETE_LOGS_ON_STARTUP=${MV_DELETE_LOGS_ON_STARTUP:-$(java $JAVA_FLAGS multiverse.scripts.PropertyGetter multiverse.delete_logs_on_startup)}

# This should always be in "unix" format
if [ $(uname -o) = "Cygwin" ]; then
    MV_RUN=${MV_RUN:-$(cygpath -w ${MV_BIN}/run/${MV_WORLDNAME})}
else
    MV_RUN=${MV_RUN:-${MV_BIN}/run/${MV_WORLDNAME}}
fi

DUALWMGRS=${DUALWMGRS:-$(java $JAVA_FLAGS multiverse.scripts.PropertyGetter multiverse.dualworldmanagers 0)}

JAVA_FLAGS="${JAVA_FLAGS} -Dmultiverse.logs=${MV_LOGS}"

start_server "$@"

 
