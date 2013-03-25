#!/bin/sh

if [ $(uname -o) = "Cygwin" ]; then
    MV_HOME_UNIX=$(cygpath -u "${MV_HOME}")
else
    MV_HOME_UNIX="$MV_HOME"
fi

MV_BIN=${MV_BIN:-"${MV_HOME}/bin"}
MV_MAILCMD=${MV_MAILCMD:-"/usr/sbin/sendmail"}

function health_check() {
    local rv
    ${MV_BIN}/multiverse.sh status > /dev/null
    rv=$?
    if [ $verbose -gt 1 ]; then
	if [ $rv -eq 0 ]; then
	    echo "process test: PASS"
	else
	    echo "process test: FAIL"
	fi
    fi
    if [ $rv -ne 0 ]; then
	if [ $verbose -gt 0 ]; then
	    echo "server health check failed"
	fi
	return $rv
    fi

    if [ $verbose -gt 1 ]; then
	${MV_BIN}/multiverse.sh -v test
    else
	${MV_BIN}/multiverse.sh test
    fi
    rv=$?
    if [ $rv -ne 0 ]; then
	if [ $verbose -gt 0 ]; then
	    echo "server health check failed"
	fi
	return $rv
    fi

    if [ $verbose -gt 0 ]; then
	echo "server health check passed"
    fi
    return 0
}

function restart_server() {
    if [ $verbose -gt 0 ]; then
	echo -n "stopping server: "
    fi
    ${MV_BIN}/multiverse.sh stop
    if [ $verbose -gt 0 ]; then
	echo "DONE"
	echo -n "rotating logs: "
    fi
    ${MV_BIN}/rotate-logs.sh
    if [ $verbose -gt 0 ]; then
	echo "DONE"
	echo -n "restarting: "
    fi
    if [ $verbose -gt 1 ]; then
	echo
	${MV_BIN}/multiverse.sh -v start
    else
	${MV_BIN}/multiverse.sh start
    fi
    if [ $verbose -gt 0 ]; then
	echo "DONE"
    fi
}

function send_alert() {
    if [ -n "$MV_MAILTO" ]; then
	$MV_MAILCMD $MV_MAILTO <<EOF
Subject: Multiverse Server restart

$(date)
The Multiverse Server is being restarted because it failed a health check.

Current status:
$(${MV_BIN}/multiverse.sh status)
EOF
    fi
}

verbose=0
while getopts "hv" arg; do
    case "$arg" in
	h)
	    echo "$0: usage: $0 [-hv]"
	    ;;
	v)
	    let verbose++
	    ;;
    esac
done
shift $((OPTIND-1))

if [ $verbose -gt 0 ]; then
    date
fi

health_check
if [ $? -ne 0 ]; then
    send_alert
    restart_server
fi
