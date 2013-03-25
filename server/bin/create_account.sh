#!/bin/bash

if [ $# -lt 6 ]
  then
    echo "usage: create_account.sh <dbname> <dbhost> <dbuser> <dbpassword> <account_username> <account_password>"
    exit
fi

DBNAME=$1
DBHOST=$2
DBUSER=$3
DBPASSWORD=$4

USERNAME=$5
PASSWORD=$6


mysql $DBNAME -h $DBHOST -u $DBUSER -p$DBPASSWORD -e "insert into account (username, password) values (\"$USERNAME\", \"$PASSWORD\");"
mysql $DBNAME -h $DBHOST -u $DBUSER -p$DBPASSWORD -e "select account_id from account where username=\"$USERNAME\"";
