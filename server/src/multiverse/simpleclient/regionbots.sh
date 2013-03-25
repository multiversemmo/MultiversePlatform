#!/bin/sh

WORLD_LOGIN=localhost:5040
ACCOUNT_START=5100

REGION_START=1
REGION_END=10
BOTS_PER_REGION=100
LOGIN_INTERVAL=1

region=$REGION_START
account=$ACCOUNT_START
player_letter=65

while [ $region -le $REGION_END ]; do

    letter=\\`printf \\%03o $player_letter`
    letter=`printf $letter`
    name=$letter$letter$letter
    echo STARTING region $region with name $name

    ./runplayerclients.sh $account $BOTS_PER_REGION $LOGIN_INTERVAL -n $name --login $WORLD_LOGIN -X "--polygon_region bot$region" --tcp "$@"

    let "account += BOTS_PER_REGION"
    let "region += 1"
    let "player_letter += 1"


    sleeptime=$(python -c "print int($BOTS_PER_REGION * $LOGIN_INTERVAL * 1.05)")
    sleep $sleeptime
done

