#!/bin/sh
(shopt -s igncr) 2>/dev/null && shopt -s igncr; # Workaround Cygwin line-ending issue

# This batch file runs the Multiverse sever processes on Linux in bash shell or on Windows/Cygwin
# You must have installed Java, a database, JDBC driver, and the Multiverse servers
# Copyright 2006 The Multiverse Network, Inc.
# Thanks to Judd-MGT for contributions.

### You MUST set this to the location of the JDBC driver, either here or in env. variable.
JDBC=${JDBC:-"/mysql/mysql-connector-java-3.1.14-production-bin.jar"}

# Optional: Set MV_HOME env. variable to be able to run this script from an arbitrary directory.
# Otherwise, this script assumes it is being run from MV_HOME/bin working directory
MV_HOME=${MV_HOME:-".."}

# Optional: Set DEFAULT_MV_PROPERTYFILE env. variable to set the default property file, otherwise use multiverse.properties
# when no property file is specified as final argument on command line.
DEFAULT_MV_PROPERTYFILE=${DEFAULT_MV_PROPERTYFILE:-"multiverse.properties"}

### Set to true to enable JMX management and monitoring (either here or in env. variable.
ENABLE_MGMT=${ENABLE_MGMT:-"false"}

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
	echo "*** Starting world $MV_WORLDNAME ***"
	if [ ! -d "${MV_RUN}" ]
	  then
		  mkdir -p "${MV_RUN}"
	fi

	if [ ! -d "${MV_LOGS}" ]
	  then
		  mkdir -p "${MV_LOGS}"
	fi

	if $ENABLE_MGMT = "true"; then        
	  echo "Enabling JMX mgmt & monitoring"
	  JAVA_FLAGS="-Dcom.sun.management.jmxremote ${JAVA_FLAGS}"
	fi        

	if [ $verbose -gt 0 ]; then
		echo MV_HOME is $MV_HOME
		echo Using property file $MV_PROPERTYFILE  
		echo Using world file $MVW_FILENAME
		echo Using world script directory $MV_WORLD
		echo Using log directory $MV_LOGS
		echo Using common directory $MV_COMMON, bin directory $MV_BIN     
		echo "JAVA_FLAGS=\"${JAVA_FLAGS}\""
		echo -en "Starting message server: \t"
	fi

	java ${JAVA_FLAGS} multiverse.msgsvr.MessageServer > "${MV_LOGS}/msg_svr.out" &
	echo $! > "${MV_RUN}"/msgsvr.pid

	sleep 2;

	if [ $verbose -gt 0 ]; then
		check_process "message server" $(cat "${MV_RUN}"/msgsvr.pid)
		echo -en "Starting animation server: \t"
	fi
	java \
		${JAVA_FLAGS} \
		multiverse.server.engine.Engine \
		-w ${MV_WORLDNAME} \
		-i "${MV_BIN}"/wmgr_local1.js \
		"${MV_COMMON}"/global_props.py \
		"${MV_WORLD}"/global_props.py \
		"${MV_COMMON}"/anim.py \
		"${MV_WORLD}"/extensions_anim.py \
		> "${MV_LOGS}"/anim.out 2>&1 &
	echo $! > "${MV_RUN}"/anim.pid

	if [ $verbose -gt 0 ]; then
		check_process "animation server" $(cat "${MV_RUN}"/anim.pid)
		echo -en "Starting combat server: \t"
	fi
	java \
		$JAVA_FLAGS \
		multiverse.server.engine.Engine \
		-w ${MV_WORLDNAME} \
		-i "${MV_BIN}"/wmgr_local1.js \
		"${MV_COMMON}"/global_props.py \
		"${MV_WORLD}"/global_props.py \
		"${MV_WORLD}"/ability_db.py \
		"${MV_COMMON}"/combat.py \
		"${MV_WORLD}"/extensions_combat.py \
		> "${MV_LOGS}"/combat.out 2>&1 &
	echo $! > "${MV_RUN}"/combat.pid

	if [ $verbose -gt 0 ]; then
		check_process "combat server" $(cat "${MV_RUN}"/combat.pid)
		echo -en "Starting object manager: \t"
	fi
	java \
		${JAVA_FLAGS} \
		multiverse.server.engine.Engine \
		-w ${MV_WORLDNAME} \
		-i "${MV_BIN}"/wmgr_local1.js \
		"${MV_COMMON}"/global_props.py \
		"${MV_WORLD}"/global_props.py \
		"${MV_COMMON}"/obj_manager.py \
		"${MV_WORLD}"/items_db.py \
		"${MV_WORLD}"/mobs_db.py \
		"${MV_WORLD}"/templates.py \
		"${MV_WORLD}"/extensions_objmgr.py \
		> "${MV_LOGS}"/objmgr.out 2>&1 &
	echo $! > "${MV_RUN}"/objmgr.pid

#        sleep 10;

	if [ $verbose -gt 0 ]; then
		check_process "object manager" $(cat "${MV_RUN}"/objmgr.pid)
		echo -en "Starting login manager: \t"
	fi
	java \
		${JAVA_FLAGS} \
		multiverse.server.engine.Engine \
		-w ${MV_WORLDNAME} \
		"${MV_BIN}"/login_manager.py \
		"${MV_COMMON}"/login_manager.py \
		"${MV_COMMON}"/character_factory.py \
		"${MV_WORLD}"/character_factory.py \
		"${MV_WORLD}"/extensions_login.py \
		> "${MV_LOGS}"/login_manager.out 2>&1 &
	echo $! > "${MV_RUN}"/login_manager.pid

	if [ $verbose -gt 0 ]; then
		check_process "login server" $(cat "${MV_RUN}"/login_manager.pid)
		echo -en "Starting world manager 1: \t"
	fi
	java \
		$JAVA_FLAGS \
		multiverse.server.engine.Engine \
		-w ${MV_WORLDNAME} \
		-i "${MV_BIN}"/wmgr_local1.js \
		"${MV_COMMON}"/global_props.py \
		"${MV_WORLD}"/global_props.py \
		"${MV_COMMON}"/world_mgr.py \
		"${MV_WORLD}"/extensions_wmgr.py \
		> "${MV_LOGS}"/wmgr1.out 2>&1 &
	echo $! > "${MV_RUN}"/wmgr1.pid

#        sleep 2;

	if [ $verbose -gt 0 ]; then
		check_process "world manager 1" $(cat "${MV_RUN}"/wmgr1.pid)
#           echo -en "Starting world manager 2: \t"
	fi
#   java \
#       ${JAVA_FLAGS} \
#       multiverse.server.engine.Engine \
#    	-w ${MV_WORLDNAME} \
#       -i "${MV_BIN}"/wmgr_local2.js \
#       "${MV_WORLD}"/world_mgr.py \
#       "${MV_COMMON}"/global_props.py \
#       "${MV_WORLD}"/global_props.py \
#       > "${MV_LOGS}"/wmgr2.out 2>&1 &
#   echo $! > ${MV_RUN}/wmgr2.pid
#   sleep 3; # wait for the world server to be ready

	if [ $verbose -gt 0 ]; then
#       check_process "world manager 2" $(cat "${MV_RUN}"/wmgr2.pid)
		echo -en "Starting proxy server:    \t"
	fi
	java \
		${JAVA_FLAGS} \
		multiverse.server.engine.Engine \
		-w ${MV_WORLDNAME} \
		-i "${MV_BIN}"/proxy.py \
		-i "${MV_COMMON}"/events.py \
		"${MV_COMMON}"/proxy.py \
		"${MV_COMMON}"/global_props.py \
		"${MV_WORLD}"/global_props.py \
		"${MV_WORLD}"/extensions_proxy.py \
		> "${MV_LOGS}"/proxy.out 2>&1 &
	echo $! > "${MV_RUN}"/proxy.pid

#   sleep 3;

	if [ $verbose -gt 0 ]; then
		check_process "proxy server" $(cat "${MV_RUN}"/proxy.pid)
		echo -en "Starting world reader:   \t"
	fi
	java \
		${JAVA_FLAGS} \
		multiverse.server.engine.Engine \
		-w ${MV_WORLDNAME} \
		-i "${MV_BIN}"/mobserver_local.js \
		"${MV_COMMON}"/global_props.py \
		"${MV_WORLD}"/global_props.py \
		"${MV_COMMON}"/worldreader.py \
		"${MV_WORLD}"/extensions_worldreader.py \
		> "${MV_LOGS}"/worldreader.out 2>&1 &
	echo $! > "${MV_RUN}"/worldreader.pid

	if [ $verbose -gt 0 ]; then
		check_process  "world reader" $(cat "${MV_RUN}"/worldreader.pid)
#           echo -en "starting bridge server:    \t"
	fi
#   java \
#       ${JAVA_FLAGS} \
#       multiverse.server.engine.Engine \
#       -i "${MV_BIN}"/mobserver_local.js \
#       "${MV_COMMON}"/bridge.py \
#       > "${MV_LOGS}"/bridge.out 2>&1 &
#   echo $! > ${MV_RUN}/bridge.pid

#    sleep 1;

#   if [ $verbose -gt 0 ]; then
#       check_process "bridge server" $(cat "${MV_RUN}"/bridge.pid)
#       echo -en "starting bridge client:    \t"
#   fi
#    SampleBridgeInterface.exe localhost 9757 > "${MV_LOGS}"/bridge_client.out 2>&1 &
#    echo $! > run/bridge_client.pid

	if [ $verbose -gt 0 ]; then
#           check_process "bridge client" $(cat "${MV_RUN}"/bridge_client.pid)
		echo -en "Starting mob server:    \t"
	fi
	java \
		${JAVA_FLAGS} \
		multiverse.server.engine.Engine \
		-w ${MV_WORLDNAME} \
		-i "${MV_BIN}"/mobserver_local.js \
		"${MV_COMMON}"/mobserver_init.py \
		"${MV_WORLD}"/mobserver_init.py \
		"${MV_COMMON}"/questplugin.py \
		"${MV_COMMON}"/mobserver.py \
		"${MV_WORLD}"/mobserver.py \
		"${MV_COMMON}"/extensions_mobserver.py \
		> "${MV_LOGS}"/mobserver.out 2>&1 &
	echo $! > ${MV_RUN}/mobserver.pid
	if [ $verbose -gt 0 ]; then
		check_process "mob server" $(cat "${MV_RUN}"/mobserver.pid)
	fi

#
# special plugin to wait until all initialization is done
#
	echo "Wait for finished initializing msg..."
	java \
		${JAVA_FLAGS} \
		multiverse.server.engine.Engine \
		-w ${MV_WORLDNAME} \
		-i "${MV_BIN}"/mobserver_local.js \
		"${MV_COMMON}"/startup.py \
		> "${MV_LOGS}"/startup.out &
	echo $! > ${MV_RUN}/startup.pid
}

function stop_server () {
	echo "*** Stopping world $MV_WORLDNAME ***"
	kill_process "login server" $(cat "${MV_RUN}"/login_manager.pid)
	kill_process "animation server" $(cat "${MV_RUN}"/anim.pid)
	kill_process "combat server" $(cat "${MV_RUN}"/combat.pid)
	kill_process "object manager" $(cat "${MV_RUN}"/objmgr.pid)
	kill_process "world manager 1" $(cat "${MV_RUN}"/wmgr1.pid)
#   kill_process "world manager 2" $(cat "${MV_RUN}"/wmgr2.pid)
	kill_process "proxy server" $(cat "${MV_RUN}"/proxy.pid)
	kill_process "mob server" $(cat "${MV_RUN}"/mobserver.pid)
	kill_process "message server" $(cat "${MV_RUN}"/msgsvr.pid)
	kill_process "world reader" $(cat "${MV_RUN}"/worldreader.pid)
kill_process "startup plugin" $(cat "${MV_RUN}"/startup.pid)
#   kill_process "bridge server" $(cat "${MV_RUN}"/bridge.pid)
#   kill_process "bridge client" $(cat "${MV_RUN}"/bridge_client.pid)
}

function status_server () {
	down=0
	status_process "message server" $(cat "${MV_RUN}"/msgsvr.pid)  
	status_process "login server" $(cat "${MV_RUN}"/login_manager.pid) 
	status_process "animation server" $(cat "${MV_RUN}"/anim.pid) 
	status_process "combat server" $(cat "${MV_RUN}"/combat.pid)                
	status_process "object manager" $(cat "${MV_RUN}"/objmgr.pid)
	status_process "world manager 1" $(cat "${MV_RUN}"/wmgr1.pid)
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

RHINO=${RHINO:-"${MV_HOME}/other/rhino1_5R5/js.jar"}
GETOPT=${GETOPT:-"${MV_HOME}/other/java-getopt-1.0.11.jar"}
JYTHON=${JYTHON:-"${MV_HOME}/other/jython_2_1.jar"}
MV_JAR=${MV_JAR:-"${MV_HOME}/dist/lib/multiverse.jar"}
MARS_JAR=${MARS_JAR:-"${MV_HOME}/dist/lib/mars.jar"}

if [ $(uname -o) == "Cygwin" ]; then
    export PATH=$(cygpath "$JAVA_HOME"/bin):$(cygpath "$MYSQL_HOME")/bin:.:$PATH
    MV_CLASSPATH="$RHINO;$JDBC;$MV_JAR;$MARS_JAR;$GETOPT;$JYTHON;."
    #MV_CLASSPATH="$MV_HOME/build;$RHINO;$JDBC;$GETOPT;$JYTHON;."
else
    MV_CLASSPATH="${RHINO}:${JDBC}:${MV_JAR}:${MARS_JAR}:${GETOPT}:${JYTHON}:."
fi
        
JAVA_FLAGS="-cp ${MV_CLASSPATH} -Dmultiverse.propertyfile=${MV_PROPERTYFILE}"

#Get world name from properties file, and construct path to world script dir if not set from env var.
MV_WORLDNAME=${MV_WORLDNAME:-$(java $JAVA_FLAGS multiverse.scripts.PropertyGetter multiverse.worldname)}
MV_WORLD=${MV_WORLD:-"$MV_HOME/config/$MV_WORLDNAME"}

# Get path to mvw file if set explicitly in multiverse.mvwfile, otherwise, construct path to mvwfile.
MVW_FILENAME=$(java $JAVA_FLAGS multiverse.scripts.PropertyGetter multiverse.mvwfile)

if [ "$MVW_FILENAME" = "null" ]; then
    MVW_FILENAME=$MV_HOME/config/$MV_WORLDNAME/$MV_WORLDNAME.mvw
fi

# This should always be in "unix" format
MV_LOGS=${MV_LOGS:-"${MV_HOME_UNIX}/logs/${MV_WORLDNAME}"}

# This should always be in "unix" format
if [ $(uname -o) = "Cygwin" ]; then
    MV_RUN=${MV_RUN:-$(cygpath ${MV_BIN}/run/${MV_WORLDNAME})}
else
    MV_RUN=${MV_RUN:-${MV_BIN}/run/${MV_WORLDNAME}}
fi

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
 
