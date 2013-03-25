#!/bin/bash

file="/tmp/dumpfile.sql"
user="--user=multiverse"
pass="--password=foobar123"
db="scheduler"

cd /home/multiverse

mysqldump -v $user $pass --host=render1 $db > $file

if [ $? -eq 0 ]; then
  mysql $user $pass $db < $file
  rm -f $file
fi
