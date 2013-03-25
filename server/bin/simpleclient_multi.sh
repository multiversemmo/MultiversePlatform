#!/bin/sh

ACCOUNTID=1234
COUNT=10

if [ $# -gt 0 ] ; then
    COUNT=$1
    shift
fi

trap "echo Control-C ; killChildren ; exit 2" 2
killChildren() {
    for pid in $PIDS
    do
	echo Killing pid $pid
	kill $pid
    done
}

PIDS=

while [ $COUNT -gt 0 ]
do
   # sleep 10 &
   sh ./simpleclient.sh -a $ACCOUNTID -n GOOBER1 "$@" &
   echo "Started simpleclient.sh with account id " $ACCOUNTID " pid=" $!
   PIDS="$PIDS $!"
   COUNT=`expr $COUNT - 1`
   ACCOUNTID=`expr $ACCOUNTID + 1`
done

wait
