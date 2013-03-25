#!/bin/bash

# this checks if the Worlds/<worldname> has any files that IS NOT 
# in the Media directory

if [ $# -lt 1 ]; then
    echo "specify directory name under Worlds (eg: mv_social)"
    exit 0
fi

WORLD_DIR=$1
WORLDS=../../Worlds/$WORLD_DIR
MEDIA=../../Media

date=`date "+%y%m%d_%H%M"`

echo "Files missing in Media:"
for file in `cd $WORLDS; find . -type f|grep -v '.svn/'`; do
    if [ ! -f $MEDIA/$file ]; then
	echo "$file"
    fi
done
