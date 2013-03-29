#!/bin/sh
(shopt -s igncr) 2>/dev/null && shopt -s igncr; # Workaround Cygwin line-ending issue

###
### PLACES server control script
### Linux only
###

# This batch file runs the Multiverse sever processes on Linux in bash shell or on Windows/Cygwin
# You must have installed Java, a database, JDBC driver, and the Multiverse servers
# Copyright 2007 The Multiverse Network, Inc.
# Thanks to Judd-MGT for contributions.

# Optional: Set MV_HOME env. variable to be able to run this script from an arbitrary directory.
# Otherwise, this script assumes it is being run from MV_HOME/bin working directory
# NOTE: Doesn't work if you set MV_HOME.
export MV_HOME=${MV_HOME:-".."}

### PLACES config

MV_WORLDNAME=friendworld
ENABLE_MGMT=true
DEPLOY_MODE=true
AGGRESIVE="-XX:CompileThreshold=200 -Xnoclassgc -XX:+RelaxAccessControlCheck"

# Optional: Set DEFAULT_MV_PROPERTYFILE env. variable to set the default property file, otherwise use multiverse.properties
# when no property file is specified as final argument on command line.
DEFAULT_MV_PROPERTYFILE=${DEFAULT_MV_PROPERTYFILE:-"multiverse.properties"}

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

function alloc_domain_name () {

    type=$1
    name=$2

    if [ $(uname -o) == "Cygwin" ]; then
	CMD_CLASSPATH="$MV_JAR;$GETOPT;$LOG4J;$BCEL;."
    else
	CMD_CLASSPATH="${MV_JAR}:${GETOPT}:${LOG4J}:${BCEL}:."
    fi

    # not currently using CMD_CLASSPATH

    java -client ${JAVA_FLAGS} \
	    -Dmultiverse.disable_logs=true \
	    -Dmultiverse.log_level=3 \
	    multiverse.msgsys.DomainCommand \
	    $CMDLINE_PROPS \
        -t "${MV_COMMON}"/typenumbers.txt \
	    -n ${type},${name}

    if [ $? -ne 0 ]; then
	echo "alloc_domain_name failed" 1>&2
	exit 1
    fi
}

function start_world_manager () {

    AGENT_TYPE=wmgr
    AGENT_NAME=$(alloc_domain_name AGENT ${AGENT_TYPE}_# )

    if [ $verbose -gt 0 ]; then
        echo -en "Starting $AGENT_NAME ...     \t"
    fi

    mkdir -p ${MV_RUN}

    java \
        $JAVA_FLAGS \
	$JMX_FLAGS \
        -Dmultiverse.agenttype=${AGENT_TYPE} \
        -Dmultiverse.loggername=${AGENT_NAME} \
        multiverse.server.engine.Engine \
	$CMDLINE_PROPS \
        -i "${MV_BIN}"/wmgr_local1.py \
        -i "${MV_COMMON}"/mvmessages.py \
        -i "${MV_WORLD}"/worldmessages.py \
        -t "${MV_COMMON}"/typenumbers.txt \
		"${MV_COMMON}"/global_props.py \
        "${MV_WORLD}"/global_props.py \
        "${MV_COMMON}"/world_mgr1.py \
        "${MV_WORLD}"/extensions_wmgr.py \
        &

    PID=$!
    echo $PID > "${MV_RUN}"/${AGENT_NAME}.pid

    if [ $verbose -gt 0 ]; then
        check_process $PID
    fi
}

function start_proxy () {

    AGENT_TYPE=proxy
    AGENT_NAME=$(alloc_domain_name AGENT ${AGENT_TYPE}_# )

    if [ $verbose -gt 0 ]; then
        echo -en "Starting $AGENT_NAME ...     \t"
    fi

    mkdir -p ${MV_RUN}

    java  \
        $JAVA_FLAGS \
	$JMX_FLAGS \
        -Dmultiverse.agenttype=$AGENT_TYPE \
        -Dmultiverse.loggername=$AGENT_NAME \
        multiverse.server.engine.Engine \
	$CMDLINE_PROPS \
        -i "${MV_BIN}"/proxy.py \
        -i "${MV_COMMON}"/events.py \
        -i "${MV_COMMON}"/mvmessages.py \
        -i "${MV_WORLD}"/worldmessages.py \
        -t "${MV_COMMON}"/typenumbers.txt \
        "${MV_COMMON}"/proxy.py \
        "${MV_COMMON}"/global_props.py \
        "${MV_WORLD}"/global_props.py \
        "${MV_WORLD}"/extensions_proxy.py \
        &

    PID=$!
    echo $PID > "${MV_RUN}"/${AGENT_NAME}.pid

    if [ $verbose -gt 0 ]; then
        check_process $PID
    fi
}

function archive_log_dir () {
    rm -rf ${MV_LOGS}.old/*
    mkdir -p ${MV_LOGS}.old
    mv ${MV_LOGS}/* ${MV_LOGS}.old
}

function start_server () {

    if [ X$ARCHIVE_LOG_DIR = X"true" ]; then
	if [ -d "${MV_LOGS}" ]; then
	    archive_log_dir
	fi
    fi

    # Do marshalling code injection if USE_CLASS_FILES is true
    if [ X$USE_CLASS_FILES = X"true" ]; then
        ./performinjection.sh $MV_WORLDNAME
    fi
	
    # Use these flags for profiling
    HPROF_FLAGS="-agentlib:hprof=heap=sites,depth=8"
    GCDETAILS_FLAGS="-XX:+PrintGC -XX:+PrintGCDetails"

    # Linux: Use strace with the following flags to monitor one of the
    # multiverse server processes.
    #    strace -f -e trace=\!futex,gettimeofday,clock_gettime java \

    echo "*** Starting world $MV_WORLDNAME ***"

    rm -f $multiverse_world_available_file
    if [ $DEPLOY_MODE = "true" ]; then
       if [ -x $MV_HOME/config/$MV_WORLDNAME/close_ports.sh ] ; then
           echo "Closing network ports for external access"
           $MV_HOME/config/$MV_WORLDNAME/close_ports.sh \
		$multiverse_worldmgrport \
		$multiverse_proxyport \
		$multiverse_voiceport
       fi
    fi

    if [ ! -d "${MV_RUN}" ]
        then
        mkdir -p "${MV_RUN}"
    fi

    if [ ! -d "${MV_LOGS}" ]
        then
        mkdir -p "${MV_LOGS}"
    fi

    rm -f ${MV_RUN}/*.pid

    if [ X$DELETE_LOGS_ON_STARTUP = X"true" ]; then
        rm "${MV_LOGS}"/*.out*
    fi

    if [ X$ENABLE_MGMT = X"true" ]; then
        echo "Enabling JMX mgmt & monitoring"
        JAVA_FLAGS="${JAVA_FLAGS} $JMX_FLAGS"
    fi        

    if [ $verbose -gt 0 ]; then
        echo MV_HOME is $MV_HOME
        if [ X$USE_CLASS_FILES = X"true" ]; then
            echo "Using .class files from the /build hierarchy"
        else
            echo "Using .jar files from the /dist hierarchy"
        fi
        echo Using property file $MV_PROPERTYFILE  
        echo Using world file $MVW_FILENAME
        echo Using world script directory $MV_WORLD
        echo Using log directory $MV_LOGS
        echo Using common directory $MV_COMMON, bin directory $MV_BIN     
        echo "JAVA_FLAGS=\"${JAVA_FLAGS}\""
    fi

    # Increase the file descriptor limit up to the hard limit
    # Linux: Use /etc/security/limits.conf to set the hard limit
    ulimit -n hard

    AGENT_NAMES="-a combat -a wmgr_1 -a mobserver -a objmgr -a login_manager -a proxy_1 -a instance -a voiceserver"
    PLUGIN_TYPES="-p Login,1 -p Proxy,1 -p ObjectManager,1 -p WorldManager,1 -p Inventory,1 -p MobManager,1 -p Quest,1 -p Instance,1 -p Voice,1 -p Trainer,1 -p Group,1 -p Combat,1 -p ClassAbility,1 -p Domain,1"

    if [ $verbose -gt 0 ]; then
        echo -en "Starting domain server: \t"
    fi

    java ${JAVA_FLAGS} \
        -Dmultiverse.loggername=domain \
        multiverse.msgsys.DomainServer \
	$CMDLINE_PROPS \
        -t "${MV_COMMON}"/typenumbers.txt \
        ${AGENT_NAMES} ${PLUGIN_TYPES} \
        &

    echo $! > "${MV_RUN}"/domain.pid

    if [ $verbose -gt 0 ]; then
        check_process $(cat "${MV_RUN}"/domain.pid)
    fi

    start_world_manager

    if [ $verbose -gt 0 ]; then
        echo -en "Starting combat server: \t"
    fi
    java \
        $JAVA_FLAGS \
        -Dmultiverse.loggername=combat \
        multiverse.server.engine.Engine \
	$CMDLINE_PROPS \
        -i "${MV_BIN}"/wmgr_local1.py \
        -i "${MV_COMMON}"/mvmessages.py \
        -i "${MV_WORLD}"/worldmessages.py \
        -t "${MV_COMMON}"/typenumbers.txt \
        "${MV_COMMON}"/global_props.py \
        "${MV_WORLD}"/global_props.py \
        "${MV_COMMON}"/skill_db.py \
        "${MV_WORLD}"/skill_db.py \
        "${MV_COMMON}"/ability_db.py \
        "${MV_WORLD}"/ability_db.py \
        "${MV_WORLD}"/classabilityplugin.py \
        "${MV_WORLD}"/combat.py \
        "${MV_WORLD}"/extensions_combat.py \
        "${MV_COMMON}"/profession_db.py \
        "${MV_WORLD}"/profession_db.py \
        "${MV_COMMON}"/groupplugin.py \
        "${MV_WORLD}"/group.py \
        &

    echo $! > "${MV_RUN}"/combat.pid

    if [ $verbose -gt 0 ]; then
        check_process $(cat "${MV_RUN}"/combat.pid)
        echo -en "Starting instance server: \t"
    fi
    java \
        $JAVA_FLAGS \
        -Dmultiverse.loggername=instance \
        multiverse.server.engine.Engine \
	$CMDLINE_PROPS \
        -i "${MV_COMMON}"/mvmessages.py \
        -i "${MV_WORLD}"/worldmessages.py \
        -t "${MV_COMMON}"/typenumbers.txt \
        "${MV_COMMON}"/global_props.py \
        "${MV_WORLD}"/global_props.py \
        "${MV_COMMON}"/instance.py \
        "${MV_WORLD}"/startup_instance.py \
        &

    echo $! > "${MV_RUN}"/instance.pid

    if [ $verbose -gt 0 ]; then
        check_process $(cat "${MV_RUN}"/instance.pid)
        echo -en "Starting object manager: \t"
    fi
    java \
        ${JAVA_FLAGS} \
        -Dmultiverse.loggername=objmgr \
        multiverse.server.engine.Engine \
	$CMDLINE_PROPS \
        -i "${MV_COMMON}"/mvmessages.py \
        -i "${MV_WORLD}"/worldmessages.py \
        -t "${MV_COMMON}"/typenumbers.txt \
        "${MV_COMMON}"/global_props.py \
        "${MV_WORLD}"/global_props.py \
        "${MV_WORLD}"/templates.py \
        "${MV_COMMON}"/obj_manager.py \
        "${MV_WORLD}"/mobs_db.py \
        "${MV_WORLD}"/items_db.py \
        "${MV_WORLD}"/extensions_objmgr.py \
        &

    echo $! > "${MV_RUN}"/objmgr.pid

    if [ $verbose -gt 0 ]; then
        check_process $(cat "${MV_RUN}"/objmgr.pid)
        echo -en "Starting login manager: \t"
    fi

    java \
        ${JAVA_FLAGS} \
        -Dmultiverse.loggername=login_manager \
        multiverse.server.engine.Engine \
	$CMDLINE_PROPS \
        -i "${MV_COMMON}"/mvmessages.py \
        -i "${MV_WORLD}"/worldmessages.py \
        -t "${MV_COMMON}"/typenumbers.txt \
        "${MV_BIN}"/login_manager.py \
        "${MV_COMMON}"/login_manager.py \
        "${MV_COMMON}"/character_factory.py \
        "${MV_WORLD}"/character_factory.py \
        "${MV_WORLD}"/extensions_login.py \
        &

    echo $! > "${MV_RUN}"/login_manager.pid

    if [ $verbose -gt 0 ]; then
        check_process $(cat "${MV_RUN}"/login_manager.pid)
    fi

    start_proxy

    if [ $verbose -gt 0 ]; then
        echo -en "Starting mob server:    \t"
    fi

    java \
        ${JAVA_FLAGS} \
        -Dmultiverse.loggername=mobserver \
        multiverse.server.engine.Engine \
	$CMDLINE_PROPS \
        -i "${MV_BIN}"/mobserver_local.py \
        -i "${MV_COMMON}"/mvmessages.py \
        -i "${MV_WORLD}"/worldmessages.py \
        -t "${MV_COMMON}"/typenumbers.txt \
        "${MV_COMMON}"/global_props.py \
        "${MV_WORLD}"/global_props.py \
        "${MV_COMMON}"/mobserver_init.py \
        "${MV_WORLD}"/mobserver_init.py \
        "${MV_COMMON}"/questplugin.py \
        "${MV_COMMON}"/trainerplugin.py \
        "${MV_COMMON}"/mobserver.py \
        "${MV_WORLD}"/mobserver.py \
        "${MV_WORLD}"/extensions_mobserver.py \
        &

    echo $! > ${MV_RUN}/mobserver.pid
    if [ $verbose -gt 0 ]; then
        check_process $(cat "${MV_RUN}"/mobserver.pid)
        echo -en "Starting voice server:    \t"
    fi

    java \
        ${JAVA_FLAGS} \
        -Dmultiverse.loggername=voiceserver \
        multiverse.server.engine.Engine \
	$CMDLINE_PROPS \
        -i "${MV_COMMON}"/mvmessages.py \
        -i "${MV_WORLD}"/worldmessages.py \
        -t "${MV_COMMON}"/typenumbers.txt \
        "${MV_COMMON}"/voice.py \
        "${MV_WORLD}"/voice.py \
        &

    echo $! > ${MV_RUN}/voiceserver.pid
    if [ $verbose -gt 0 ]; then
        check_process $(cat "${MV_RUN}"/voiceserver.pid)
    fi

    echo "Waiting for $MV_WORLDNAME to be available"
    while [ ! -f $multiverse_world_available_file ] ;
    do
        sleep 1
    done

    if [ -n "$multiverse_secondary_hosts" ]; then
        start_secondary_hosts
    fi

    if [ "$DEPLOY_MODE" = "true" ]; then
       if [ -x $MV_HOME/config/$MV_WORLDNAME/exercise.sh ] ; then
           $MV_HOME/config/$MV_WORLDNAME/exercise.sh \
		$multiverse_worldmgrport \
		$places_bot_proxy_map
       fi

       if [ -x $MV_HOME/config/$MV_WORLDNAME/open_ports.sh ] ; then
           echo "Opening network ports for external access"
           $MV_HOME/config/$MV_WORLDNAME/open_ports.sh
       fi
    fi

    echo "$MV_WORLDNAME open for login"

}

function start_secondary_hosts () {
    DIR=`( cd $MV_BIN ; pwd )`
    #DIR=/tmp/multiverse/bin
    for host in $multiverse_secondary_hosts
    do
        var=multiverse_${host}_processes
        procs=${!var}
        echo $host $procs
        echo "Archiving logs on $host"
        ssh $host "cd $DIR ; mkdir -p ../logs ; ./places.sh archive_logs >& ../logs/archive_logs.procout"
        for proc in $procs
        do
            cmd=$proc
            flags="-Pmultiverse.msgsvr_hostname=$MACHINE_HOSTNAME"
            if [ $DEPLOY_MODE == "false" ]; then
                flags="$flags -x"
            fi
            echo "Starting $proc on $host with flags $flags"
            ssh $host "cd $DIR ; ./places.sh $flags $cmd >& ../logs/${cmd}.procout"
        done
    done
}

function stop_server () {
    echo "*** Stopping world $MV_WORLDNAME ***"
    kill_process "login server   " $(cat "${MV_RUN}"/login_manager.pid)
    kill_process "combat server  " $(cat "${MV_RUN}"/combat.pid)
    kill_process "instance       " $(cat "${MV_RUN}"/instance.pid)
    kill_process "object manager " $(cat "${MV_RUN}"/objmgr.pid)
    kill_process "world manager  " $(cat "${MV_RUN}"/wmgr_1.pid)
    kill_process "proxy server   " $(cat "${MV_RUN}"/proxy_1.pid)
    kill_process "mob server     " $(cat "${MV_RUN}"/mobserver.pid)
    kill_process "voice server   " $(cat "${MV_RUN}"/voiceserver.pid)
    kill_process "domain server  " $(cat "${MV_RUN}"/domain.pid)
}

function status_server () {
    down=0
    status_process "domain server  " $(cat "${MV_RUN}"/domain.pid)  
    status_process "login server   " $(cat "${MV_RUN}"/login_manager.pid) 
    status_process "combat server  " $(cat "${MV_RUN}"/combat.pid)
    status_process "instance       " $(cat "${MV_RUN}"/instance.pid)
    status_process "object manager " $(cat "${MV_RUN}"/objmgr.pid)
    status_process "world manager  " $(cat "${MV_RUN}"/wmgr_1.pid)
    status_process "proxy server   " $(cat "${MV_RUN}"/proxy_1.pid)
    status_process "mob server     " $(cat "${MV_RUN}"/mobserver.pid)
    status_process "voice server   " $(cat "${MV_RUN}"/voiceserver.pid)
    exit ${down}
}

function test_server () {
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

function expand_file_name () {
    file=`echo $1 | \
	sed -e "s,\\$MV_HOME,$MV_HOME," \
	    -e "s,\\$WORLD_NAME,$MV_WORLDNAME," \
	    -e "s,\\$MV_LOGS,$MV_LOGS,"`
    echo $file
}

verbose=0

rm -f _cmdline_props_

while getopts "hvw:p:CMP:Aax" arg; do
    case "$arg" in
        h)
            echo "$0: usage: $0 [-hvCM] [-w worldname] [-p propertyfilename] (start|stop|status|test)"
            ;;
        v)
            let verbose++
            ;;
	w)
	    MV_WORLDNAME=$OPTARG
	    ;;
	p)
	    if [ -z $MV_PROPERTYFILE ]; then
		MV_PROPERTYFILE=$OPTARG
	    else
		CMDLINE_PROPS="$CMDLINE_PROPS -p $OPTARG"
		PROPERTY_FILES="$PROPERTY_FILES $OPTARG"
	    fi
	    ;;
	C)
	    JVM_FLAG=-client
	    ;;
	M)
	    ENABLE_MGMT=true
	    ;;
	P)
	    CMDLINE_PROPS="$CMDLINE_PROPS -P$OPTARG"
	    echo $OPTARG >> _cmdline_props_
	    ;;
	A)
	    AGGRESIVE="-XX:CompileThreshold=200 -Xnoclassgc -XX:+RelaxAccessControlCheck"
	    ;;
	a)
	    ARCHIVE_LOG_DIR=true
	    ;;
	x)
	    DEPLOY_MODE=false
	    AGGRESIVE=
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

if [ X$ENABLE_MGMT == "Xtrue" ] ; then
    JMX_FLAGS="-Dcom.sun.management.jmxremote"
fi

if [ -n $MV_WORLDNAME ]; then
    JAVA_FLAGS="$JAVA_FLAGS -Dmultiverse.worldname=$MV_WORLDNAME"
    CMDLINE_PROPS="$CMDLINE_PROPS -Pmultiverse.worldname=$MV_WORLDNAME"
fi

MV_PROPERTYFILE=${MV_PROPERTYFILE:-"${MV_BIN}/${DEFAULT_MV_PROPERTYFILE}"}
MACHINE_HOSTNAME=`hostname`

import_property_file $MV_PROPERTYFILE
if [ -n "$MV_WORLDNAME" ] ; then
    multiverse_worldname=$MV_WORLDNAME
fi

import_property_file \
        "$MV_HOME/config/$multiverse_worldname/world.properties" \
        "$MV_HOME/config/$multiverse_worldname/${MACHINE_HOSTNAME}.properties" \
        $PROPERTY_FILES \
        _cmdline_props_
rm -f _cmdline_props_

if [ -z "$MV_HOSTNAME" -a -n "$multiverse_hostname" ] ; then
    MV_HOSTNAME=$multiverse_hostname
fi

if [ -z "$ARCHIVE_LOG_DIR" -a -n "$multiverse_archive_logs_on_startup" ] ; then
    ARCHIVE_LOG_DIR=$multiverse_archive_logs_on_startup
fi

# Determine if we should use .class files from the build hierarchy,
# or .jar files from the dist hierarchy.  To run the property getter
# before MV_JAR is set, we always use the dist version of the property
# getter.
USE_CLASS_FILES=${USE_CLASS_FILES:-$multiverse_use_class_files}

RHINO=${RHINO:-"${MV_HOME}/other/rhino1_5R5/js.jar"}
GETOPT=${GETOPT:-"${MV_HOME}/other/java-getopt-1.0.11.jar"}
JYTHON=${JYTHON:-"${MV_HOME}/other/jython.jar"}
LOG4J=${LOG4J:-"${MV_HOME}/other/log4j-1.2.14.jar"}
BCEL=${BCEL:-"${MV_HOME}/other/bcel-5.2.jar"}

if  [ X$USE_CLASS_FILES = X"true" ]; then
    MV_JAR=${MV_JAR:-"${MV_HOME}/build"}
    MARS_JAR=${MARS_JAR:-"${MV_HOME}/build"}
    INJECTED_JAR=${INJECTED_JAR:-"${MV_HOME}/inject"}
else
    MV_JAR=${MV_JAR:-"${MV_HOME}/dist/lib/multiverse.jar"}
    MARS_JAR=${MARS_JAR:-"${MV_HOME}/dist/lib/mars.jar"}
    INJECTED_JAR=${INJECTED_JAR:-"${MV_HOME}/dist/lib/injected.jar"}
fi

EXT_JAR=${EXT_JAR:="${MV_HOME}/dist/lib/${MV_WORLDNAME}.jar"}
JDBC=${JDBC:-$multiverse_jdbcJarPath}

if [ $(uname -o) == "Cygwin" ]; then
    export PATH=$(cygpath "$JAVA_HOME"/bin):.:$PATH
    MV_CLASSPATH="$RHINO;$JDBC;$INJECTED_JAR;$MV_JAR;$MARS_JAR;$EXT_JAR;$GETOPT;$JYTHON;$LOG4J;$BCEL;."
else
    MV_CLASSPATH="${RHINO}:${JDBC}:${INJECTED_JAR}:${MV_JAR}:${MARS_JAR}:${EXT_JAR}:${GETOPT}:${JYTHON}:${LOG4J}:${BCEL}:."
fi
        
# HotSpot tracking flags: -XX:+PrintCompilation -XX:+CITime
JVM_FLAG="${JVM_FLAG:-"-server"} $AGGRESIVE"
JAVA_FLAGS="-cp ${MV_CLASSPATH} -Dmultiverse.propertyfile=${MV_PROPERTYFILE} ${JAVA_FLAGS}"
JAVA_FLAGS="${JVM_FLAG} ${JAVA_FLAGS}"

#Get world name from properties file, and construct path to world script dir if not set from env var.
MV_WORLDNAME=${MV_WORLDNAME:-$multiverse_worldname}
MV_WORLD=${MV_WORLD:-"$MV_HOME/config/$MV_WORLDNAME"}

# Get path to mvw file if set explicitly in multiverse.mvwfile, otherwise, construct path to mvwfile.
MVW_FILENAME=${MVW_FILENAME:-$multiverse_mvwfile}

if [ "$MVW_FILENAME" = "null" ]; then
    MVW_FILENAME=$MV_HOME/config/$MV_WORLDNAME/$MV_WORLDNAME.mvw
fi

if [ X"$MV_HOSTNAME" != "X" ]; then
    CMDLINE_PROPS="$CMDLINE_PROPS -Pmultiverse.hostname=${MV_HOSTNAME}"
    JAVA_FLAGS="$JAVA_FLAGS -Dmultiverse.hostname=${MV_HOSTNAME}"
fi

# This is in local OS format
MV_LOGS=${MV_LOGS:-"${MV_HOME}/logs/${MV_WORLDNAME}"}
DELETE_LOGS_ON_STARTUP=${DELETE_LOGS_ON_STARTUP:-$multiverse_delete_logs_on_startup}

# This should always be in "unix" format
if [ $(uname -o) = "Cygwin" ]; then
    MV_RUN=${MV_RUN:-$(cygpath -w ${MV_BIN}/run/${MV_WORLDNAME})}
else
    MV_RUN=${MV_RUN:-${MV_BIN}/run/${MV_WORLDNAME}}
fi

if [ -n "$multiverse_world_available_file" ]; then
    multiverse_world_available_file=`expand_file_name $multiverse_world_available_file`
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

    proxy)
        start_proxy
        ;;

    wmgr)
        start_world_manager
        ;;

    archive_logs)
	if [ -d "${MV_LOGS}" ]; then
            archive_log_dir
        fi
        ;;

    test)
        test_server
        ;;
esac 
 
