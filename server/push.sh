#!/bin/sh

if [ $# == 0 ]; then
    hosts="perf1"
    cascade="perf1"
else
    hosts="$@"
fi


if [ X$1 == X"all" ]; then
    hosts="perf2 perf3 perf4 perf5"
fi

EXCLUDES="--exclude=bin/run --exclude=other/cachedir --exclude=logs --exclude=src/multiverse/simpleclient/logs --exclude=javadoc --exclude=*.swp"

for host in $hosts
do
    rsync -av $EXCLUDES ./ multiverse@$host:steve
done


if [ X$cascade != X ]; then
    ssh multiverse@$host cd steve \; sh push.sh all
fi
