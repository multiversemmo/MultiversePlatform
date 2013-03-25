#!/bin/sh


if [ "X"$MV_HOME = "X" ] ; then
    echo "$0: MV_HOME not set"
    exit 1
fi

if [ $# -ge 1 ]; then
    LOGIN_PORT=$1
else
    LOGIN_PORT=5040
fi

shift
# args after the login port are forwarded to SimpleClient

BOT_SCRIPT=$MV_HOME/src/multiverse/simpleclient/partybots.sh
ACCOUNT_START=990000000
ROOM_COUNT=5
BOTS_PER_ROOM=10
LOGIN_INTERVAL=2
NAME_PREFIX=BOT
RUNTIME=15
SENTINEL_FILE=/tmp/delete.to.stop.places.exercise.bots

touch $SENTINEL_FILE

$BOT_SCRIPT --login localhost:$LOGIN_PORT \
        --account $ACCOUNT_START \
        --rooms $ROOM_COUNT \
        --bots-per-room $BOTS_PER_ROOM \
        --interval $LOGIN_INTERVAL \
        --name-prefix $NAME_PREFIX \
        -- -f $SENTINEL_FILE "$@"

echo "Bots started, sleeping for $RUNTIME seconds"
sleep $RUNTIME
echo "Stopping bots"
rm -f $SENTINEL_FILE

