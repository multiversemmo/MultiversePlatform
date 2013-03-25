#!/bin/bash

# Args are:
#  # of groups to start; required arg
#  count of test clients to start in each group; default 100
#  seconds between starts within a group; default 20
#  seconds between starting each group; default 13.  The point of this parm is to try to make sure
#          that we don't start 20 clients in the same second

numgroups=$1
shift
countingroup=100
secondsbetweenstartsingroup=20
secondsbetweengroups=13

arg=1
while [ $# != 0 ]; do
    flag="$1"
	if [ $arg = 1 ]; then
	    countingroup=$flag
	fi
	if [ $arg = 2 ]; then
	    secondsbetweenstartsingroup=$flag
	fi
    if [ $arg = 3 ]; then
        secondsbetweengroups=$flag
        fi
    shift
    arg=`expr $arg + 1`
done

echo numgroups $numgroups 
echo countingroup $countingroup
echo secondsbetweenstartsingroup $secondsbetweenstartsingroup
echo secondsbetweengroups $secondsbetweengroups

for ((group=1; group <= numgroups ; group++))
do
    ./runclientgroup.sh 1000 $countingroup $secondsbetweenstartsingroup $group --tcp &
    sleep $secondsbetweengroups
done
