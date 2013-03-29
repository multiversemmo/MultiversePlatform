#!/bin/sh

if [ $# -lt 6 ]
  then
    echo "usage: clear_obj_store.sh <dbname> <dbhost> <dbuser> <dbpassword>"
    exit
fi

DBNAME=$1
DBHOST=$2
DBUSER=$3
DBPASSWORD=$4

WORLDID=3

mysql $DBNAME -h $DBHOST -u $DBUSER -p$DBPASSWORD -e "delete from player_character where world_id=$WORLDID"

mysql $DBNAME -h $DBHOST -u $DBUSER -p$DBPASSWORD -e "delete from objstore where world_id=$WORLDID"
